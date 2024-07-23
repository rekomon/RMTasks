using System.ComponentModel.DataAnnotations;

namespace RMTasks.Models
{
    public class Department
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Please enter the department name.")]
        public string Name { get; set; } = null!;
        public string Logo { get; set; } = string.Empty!;
        public int? ParentDepartmentId { get; set; } = null!;
        public virtual Department? ParentDepartment { get; set; } = null!;
        public virtual ICollection<Department>? SubDepartments { get; set; } = null!;
    }
}
