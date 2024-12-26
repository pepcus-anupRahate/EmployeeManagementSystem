using EmployeeManagementSystem.Models;
using EmployeeManagementSystem.Services;
using EmployeeManagementSystem.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace EmployeeManagementSystem.Controllers
{
    [Authorize]
    public class EmployeeController(IEmployeeService employeeService, ILogger<EmployeeController> logger) : Controller
    {
        private readonly IEmployeeService _employeeService = employeeService;
        private readonly ILogger<EmployeeController> _logger = logger;

        public async Task<IActionResult> Index()
        {
            var employees = await _employeeService.GetAllEmployeesAsync();
            return View(employees);
        }

        public async Task<IActionResult> Create()
        {
            ViewBag.Roles = new SelectList(await _employeeService.GetRolesAsync(), "Id", "Name");
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create(CreateEmployeeViewModel employeeVM)
        {
            if (ModelState.IsValid)
            {
                // Map CreateEmployeeViewModel to Employee domain model
                var employee = new Employee
                {
                    Name = employeeVM.Name,
                    Email = employeeVM.Email,
                    RoleId = employeeVM.RoleId,
                    Role = employeeVM.Role
                };

                await _employeeService.AddEmployeeAsync(employee);
                return RedirectToAction(nameof(Index));
            }
            return View(employeeVM);
        }

        public async Task<IActionResult> Edit(int id)
        {
            var employee = await _employeeService.GetEmployeeByIdAsync(id);
            if (employee is null) return NotFound();
            ViewBag.Roles = new SelectList(await _employeeService.GetRolesAsync(), "Id", "Name");
            return View(employee);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Employee employee)
        {
            if (id != employee.Id)
            {
                return BadRequest();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    await _employeeService.UpdateEmployeeAsync(employee);
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    // Log the exception for debugging

                    ModelState.AddModelError(string.Empty, "An error occurred while updating the employee. Please try again later.");
                    return View(employee);
                }
            }

            return View(employee);
        }

        // GET: Employees/Details/5
        public async Task<IActionResult> Details(int id)
        {
            var employee = await _employeeService.GetEmployeeByIdAsync(id);
            if (employee is null)
            {
                return NotFound();
            }

            return View(employee);
        }

        public async Task<IActionResult> Delete(int id)
        {
            var employee = await _employeeService.GetEmployeeByIdAsync(id);
            if (employee is null) return NotFound();
            return View(employee);
        }

        [HttpPost, ActionName("Delete")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            await _employeeService.DeleteEmployeeAsync(id);
            return RedirectToAction(nameof(Index));
        }

        [Route("Home/Error")]
        public IActionResult Error()
        {
            var exceptionDetails = HttpContext.Features.Get<IExceptionHandlerPathFeature>();

            // Log the exception (optional)
            if (exceptionDetails != null)
            {
                var path = exceptionDetails.Path;
                var message = exceptionDetails.Error.Message;
                var stackTrace = exceptionDetails.Error.StackTrace;

                // Log exception details
                _logger.LogError($"Exception caught in Error action. Path: {path}, Message: {message}, StackTrace: {stackTrace}");
            }

            return View("Error");
        }
    }
}
