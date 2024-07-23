using Microsoft.EntityFrameworkCore;
using RMTasks.Models;

namespace RMTasks.Service
{
    public class DepartmentService : IDepartmentService
    {
        private readonly ApplicationDbContext _db;

        public DepartmentService(ApplicationDbContext db) 
        {
        _db = db;
        }

        private async Task<Department?> GetByIdWithParentsAsync(int id)
        {
            return await _db.Departments
                .Include(d => d.ParentDepartment)
                .FirstOrDefaultAsync(d => d.Id == id);
        }



        public async Task AddAsync(Department department)
        {
            await _db.Departments.AddAsync(department);
            await _db.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var department = await _db.Departments.FindAsync(id);
            if (department != null)
            {
                _db.Departments.Remove(department);
                await _db.SaveChangesAsync();
            }
        }

        public async Task<IEnumerable<Department>> GetAllAsync()
        {
            return await _db.Departments.ToListAsync();
        }

        public async Task<Department?> GetByIdAsync(int id)
        {
            return await _db.Departments.Include(d => d.SubDepartments)
                .FirstOrDefaultAsync(d => d.Id == id);
        }

        public async Task<IEnumerable<Department>> GetParentDepartmentsAsync(int id)
        {
            var department = await _db.Departments
                .Include(d => d.ParentDepartment)
                .FirstOrDefaultAsync(d => d.Id == id);

            var parents = new List<Department>();
            while (department?.ParentDepartment != null)
            {
                parents.Add(department.ParentDepartment);
                department = await GetByIdWithParentsAsync(department.ParentDepartment.Id);
            }
            return parents;
        }

        public async Task<IEnumerable<Department>> GetSubDepartmentsAsync(int id)
        {
            return await _db.Departments
                  .Where(d => d.ParentDepartmentId == id)
                  .ToListAsync();
        }

        public async Task UpdateAsync(Department department)
        {
            _db.Entry(department).State = EntityState.Modified;
            await _db.SaveChangesAsync();
        }

        public async Task<Department?> GetByIdWithHierarchyAsync(int id)
        {
            return await _db.Departments
                .Include(d => d.SubDepartments)
                .Include(d => d.ParentDepartment)
                .ThenInclude(p => p.ParentDepartment) 
                .FirstOrDefaultAsync(d => d.Id == id);
        }






        public async Task<bool> HasCircularParenting(Department department, Department potentialParent)
        {

            if (potentialParent == null)
            {
                return false;
            }


            var currentParent = potentialParent.ParentDepartment;
            while (currentParent != null)
            {
                if (currentParent.Id == department.Id)
                {
                    return true; 
                }
                currentParent = await GetByIdWithParentsAsync(Convert.ToInt32(currentParent.ParentDepartmentId));
            }

            if (potentialParent.ParentDepartment != null)
                return await HasCircularParenting(department, potentialParent.ParentDepartment);
            else
                return false;
        }
      
    }
}
