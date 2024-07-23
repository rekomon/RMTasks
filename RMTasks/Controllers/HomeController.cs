using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using RMTasks.Models;
using RMTasks.Service;
using System.Diagnostics;
using Microsoft.AspNetCore.Hosting.Internal;
using Microsoft.AspNetCore.Hosting;

namespace RMTasks.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IDepartmentService _departmentService;
        private readonly IReminderService _reminderService;
        private readonly IEmailService _emailService;
        private Microsoft.AspNetCore.Hosting.IHostingEnvironment _environment;
        
        public HomeController(ILogger<HomeController> logger, IDepartmentService departmentService,
            Microsoft.AspNetCore.Hosting.IHostingEnvironment environment, IEmailService emailService,IReminderService reminderService)
        {
            _logger = logger;
            _departmentService = departmentService;
            _environment = environment;
            _emailService = emailService;
            _reminderService = reminderService;
        }

        public async Task<ActionResult<List<Department>>> Index()
        {
            try
            {
                return View(await _departmentService.GetAllAsync());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "");
                return RedirectToAction("Error", "Home");
            }
        }

        public async Task<IActionResult> CreateDepartment()
        {
            ViewBag.Departments = new SelectList(await _departmentService.GetAllAsync(),
                "Id", "Name");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateDepartment(Department department, IFormFile logofile)
        {
            string logoname = "";
            if (ModelState.IsValid)
            {
                if (logofile != null && logofile.Length > 0) 
                {
                    var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/uploads", department.Name.Trim()+"_"+logofile.FileName);
                    logoname = "uploads/" + department.Name + "_" + logofile.FileName;
                    using (var stream = System.IO.File.Create(filePath))
                    {
                        await logofile.CopyToAsync(stream);
                    }
                }

                    try
                {

                    if (logoname != "") 
                    {
                        department.Logo = logoname;
                    }
                    var potentialParent = await _departmentService.GetByIdWithHierarchyAsync(Convert.ToInt32(department.ParentDepartmentId));
                    await _departmentService.AddAsync(department);
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "");
                    return View(department);
                }
            }
            ViewBag.Departments = new SelectList(await _departmentService.GetAllAsync(), "Id", "Name",
                department.ParentDepartmentId);
            return View(department);
        }



        public async Task<IActionResult> DeleteDepartment(int id)
        {
            try
            {
                var department = await _departmentService.GetByIdAsync(id);
                if (department == null)
                {
                    return NotFound();
                }
                return View(department);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "");
                return RedirectToAction("Error", "Home");
            }
        }

        public async Task<IActionResult> EditDepartment(int id)
        {
            try
            {
                ViewBag.Departments = new SelectList(await _departmentService.GetAllAsync(),
                "Id", "Name");
                var department = await _departmentService.GetByIdAsync(id);
                if (department == null) { return NotFound(); }
                return View(department);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "");
                return RedirectToAction("Error", "Home");
            }
        }



        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditDepartment(int id, Department department, IFormFile logofile)
        {
          
            if (id != department.Id)
            {
                return NotFound();
            }


            
                try
                {
                    var potentialParent = await _departmentService.GetByIdWithHierarchyAsync(Convert.ToInt32(department.ParentDepartmentId));

                    


                    if (potentialParent?.Id == department.Id)
                    {
                        ModelState.AddModelError(nameof(department.ParentDepartmentId), "Circular parenting detected.");
                    }

                 
                    string logoname = department.Logo;
                   

                    if (logofile != null && logofile.Length > 0)
                    {
                        var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/uploads", department.Name.Trim() + "_" + logofile.FileName);
                        logoname = "uploads/" + department.Name + "_" + logofile.FileName;
                       
                        using (var stream = System.IO.File.Create(filePath))
                        {
                            await logofile.CopyToAsync(stream);
                        }
                    }
                    if (!await _departmentService.HasCircularParenting(department, potentialParent?.ParentDepartment!))
                    {
                        department.Logo = logoname;
                        await _departmentService.UpdateAsync(department);
                        return RedirectToAction(nameof(Index));
                    }
                    ModelState.AddModelError(nameof(department.ParentDepartmentId), "Circular parenting detected.");
                return RedirectToAction(nameof(Index));
            }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "");
                    return View(department);
                }
            
         
        }


        public async Task<IActionResult> DepartmentDetails(int id)
        {
            try
            {
                var department = await _departmentService.GetByIdWithHierarchyAsync(id);
                if (department == null)
                {
                    return NotFound();
                }

                return View(department);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "");
                return RedirectToAction("Error", "Home");
            }
        }


        [HttpPost, ActionName("DeleteDepartment")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteDepartmentConfirmed(int id)
        {
            try
            {
                var department = await _departmentService.GetByIdAsync(id);

                if (department == null)
                {
                    return NotFound();
                }
                bool hasSubDepartments;
                if (department.SubDepartments != null)
                {
                    hasSubDepartments = department.SubDepartments.Any();
                }
                else
                {
                    hasSubDepartments = false;
                }

                if (department.ParentDepartmentId == null && !hasSubDepartments)
                {
                    await _departmentService.DeleteAsync(department.Id);
                }
                else if (department.ParentDepartmentId != null && !hasSubDepartments)
                {
                    var parentDepartment = await _departmentService.GetByIdAsync(department.ParentDepartmentId.Value);
                    if (parentDepartment != null)
                    {
                        if (parentDepartment.SubDepartments != null)
                        {
                            parentDepartment.SubDepartments.Remove(department);
                            await _departmentService.UpdateAsync(parentDepartment);
                        }
                    }
                    await _departmentService.DeleteAsync(department.Id);
                }
                else if (department.ParentDepartmentId != null && hasSubDepartments)
                {
                    var dept = await _departmentService.GetByIdAsync(id);
                    dept?.ParentDepartment?.SubDepartments?.Remove(department);

                    foreach (var subDepartment in department?.SubDepartments ?? new List<Department>())
                    {
                        subDepartment.ParentDepartment = null;
                    }
                    if (dept != null)
                        await _departmentService.DeleteAsync(dept.Id);
                }

                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "");
                return RedirectToAction("Error", "Home");
            }
        }


       

        public IActionResult Privacy()
        {
            return View();
        }





        public async Task<IActionResult> Reminders()
        {
            try
            {
                var reminders = await _reminderService.GetAll();
                return View(reminders);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "");
                return RedirectToAction("Error", "Home", null);
            }
        }



        public IActionResult CreateReminder()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateReminder(Reminder reminder)
        {
            if (ModelState.IsValid)
            {
                await _reminderService.Add(reminder);
                return RedirectToAction(nameof(Reminders));
            }
            return View(reminder);
        }


        public async Task<IActionResult> EditReminder(int id)
        {
            var reminder = await _reminderService.Get(id);
            if (reminder == null)
            {
                return NotFound();
            }
            return View(reminder);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditReminder(int id, Reminder reminder)
        {
            if (id != reminder.Id)
            {
                return NotFound();
            }
            if (ModelState.IsValid)
            {
                try
                {
                    await _reminderService.Update(reminder);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "");
                    return View(reminder);
                }
                return RedirectToAction(nameof(Reminders));
            }
            return View(reminder);
        }


        public async Task<IActionResult> DeleteReminder(int id)
        {
            var reminder = await _reminderService.Get(id);
            if (reminder == null)
            {
                return NotFound();
            }

            return View(reminder);
        }

        [HttpPost, ActionName("DeleteReminder")]
        public async Task<IActionResult> DeleteReminderConfirmed(int id)
        {
            try
            {
                await _reminderService.Delete(id);
                return RedirectToAction(nameof(Reminder));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "");
                return RedirectToAction("Error", "Home", null);
            }
        }



        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
