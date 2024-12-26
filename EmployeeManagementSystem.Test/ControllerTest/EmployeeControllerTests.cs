using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EmployeeManagementSystem.Controllers;
using EmployeeManagementSystem.Models;
using EmployeeManagementSystem.Services;
using EmployeeManagementSystem.ViewModels;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Logging;
using Moq;

namespace EmployeeManagementSystem.Test.ControllerTest
{
    [TestFixture]
    public class EmployeeControllerTests
    {
        private Mock<IEmployeeService> _mockEmployeeService;
        private Mock<ILogger<EmployeeController>> _mockLogger;
        private EmployeeController _controller;

        [SetUp]
        public void Setup()
        {
            _mockEmployeeService = new Mock<IEmployeeService>();
            _mockLogger = new Mock<ILogger<EmployeeController>>();
            _controller = new EmployeeController(_mockEmployeeService.Object, _mockLogger.Object);
        }

        [TearDown]
        public void TearDown()
        {
            _controller.Dispose();
        }

        [Test]
        public async Task Index_ShouldReturnViewWithEmployees()
        {
            // Arrange
            List<Employee> employees =
            [
                new() { Id = 1, Name = "Anup", Email = "anup@ra.com"},
                new() { Id = 2, Name = "Shubhan", Email = "shubham@pr.com"}
            ];
            _mockEmployeeService.Setup(service => service.GetAllEmployeesAsync()).ReturnsAsync(employees);

            // Act
            var result = await _controller.Index() as ViewResult;

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.Multiple(() =>
            {
                Assert.That(result.Model, Is.InstanceOf<List<Employee>>());
                Assert.That((List<Employee>)result.Model, Has.Count.EqualTo(2));
            });
        }

        [Test]
        public async Task Create_ShouldReturnViewWithRoles()
        {
            // Arrange
            List<Role> roles =
            [
                new() { Id = 1, Name = "Admin", Employees = [] },
                new() { Id = 2, Name = "User" }
            ];
            _mockEmployeeService.Setup(service => service.GetRolesAsync()).ReturnsAsync(roles);

            // Act
            var result = await _controller.Create() as ViewResult;

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.ViewData["Roles"], Is.InstanceOf<SelectList>());
        }

        [Test]
        public async Task Create_ValidEmployee_ShouldRedirectToIndex()
        {
            // Arrange
            var employee = new Employee { Id = 1, Name = "Anup", Email = "anup@ra.com", RoleId = 1, Role = new Role { Id = 1, Name = "Admin"} };
            var employeeVM = new CreateEmployeeViewModel { Id = 1, Name = "Anup", Email = "anup@ra.com", RoleId = 1, Role = new Role { Id = 1, Name = "Admin" } };
            _mockEmployeeService.Setup(service => service.AddEmployeeAsync(employee)).Returns(Task.CompletedTask);

            // Act
            var result = await _controller.Create(employeeVM) as RedirectToActionResult;

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.ActionName, Is.EqualTo("Index"));
        }

        [Test]
        public async Task Create_InvalidModelState_ShouldReturnViewWithEmployee()
        {
            // Arrange
            var employee = new Employee { Id = 1, Name = "Anup", Email = "anup@ra.com", RoleId = 1, Role = new Role { Id = 1, Name = "Admin" } };
            var employeeVM = new CreateEmployeeViewModel { Id = 1, Name = "Anup", Email = "anup@ra.com", RoleId = 1, Role = new Role { Id = 1, Name = "Admin" } };
            _controller.ModelState.AddModelError("Name", "Name is required");

            // Act
            var result = await _controller.Create(employeeVM) as ViewResult;

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Model, Is.EqualTo(employeeVM));
            _mockEmployeeService.Verify(service => service.AddEmployeeAsync(It.IsAny<Employee>()), Times.Never);
        }

        [Test]
        public async Task Edit_EmployeeExists_ShouldReturnViewWithEmployeeAndRoles()
        {
            // Arrange
            var employee = new Employee { Id = 1, Name = "Anup", Email = "anup@ra.com" };
            List<Role> roles =
            [
                new() { Id = 1, Name = "Admin" },
                new() { Id = 2, Name = "Manager" }
            ];

            _mockEmployeeService.Setup(service => service.GetEmployeeByIdAsync(1)).ReturnsAsync(employee);
            _mockEmployeeService.Setup(service => service.GetRolesAsync()).ReturnsAsync(roles);

            // Act
            var result = await _controller.Edit(1) as ViewResult;

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.Multiple(() =>
            {
                Assert.That(result.Model, Is.EqualTo(employee));
                Assert.That(result.ViewData["Roles"], Is.Not.Null);
            });
            var selectList = result.ViewData["Roles"] as SelectList;
            Assert.That(selectList.Count(), Is.EqualTo(roles.Count));
        }

        [Test]
        public async Task Edit_EmployeeDoesNotExist_ShouldReturnNotFound()
        {
            // Arrange
            Employee? emloyee = null;
            _mockEmployeeService.Setup(service => service.GetEmployeeByIdAsync(1)).ReturnsAsync(emloyee);

            // Act
            var result = await _controller.Edit(1);

            // Assert
            Assert.That(result, Is.InstanceOf<NotFoundResult>());
        }
        [Test]
        public async Task Edit_IdDoesNotMatchEmployeeId_ShouldReturnBadRequest()
        {
            // Arrange
            var employee = new Employee { Id = 1, Name = "Anup", Email = "anup@ra.com" };

            // Act
            var result = await _controller.Edit(2, employee);

            // Assert
            Assert.That(result, Is.InstanceOf<BadRequestResult>());
        }

        [Test]
        public async Task Edit_InvalidModelState_ShouldReturnViewWithEmployee()
        {
            // Arrange
            var employee = new Employee { Id = 1, Name = "Anup", Email = "anup@ra.com" };
            _controller.ModelState.AddModelError("Name", "Name is required");

            // Act
            var result = await _controller.Edit(1, employee) as ViewResult;

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Model, Is.EqualTo(employee));
        }

        [Test]
        public async Task Edit_ValidModelState_UpdateSucceeds_ShouldRedirectToIndex()
        {
            // Arrange
            var employee = new Employee { Id = 1, Name = "Anup", Email = "anup@ra.com" };
            _mockEmployeeService.Setup(service => service.UpdateEmployeeAsync(employee)).Returns(Task.CompletedTask);

            // Act
            var result = await _controller.Edit(1, employee) as RedirectToActionResult;

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.ActionName, Is.EqualTo("Index"));
        }

        [Test]
        public async Task Edit_ValidModelState_UpdateFails_ShouldAddModelErrorAndReturnView()
        {
            // Arrange
            var employee = new Employee { Id = 1, Name = "Anup", Email = "anup@ra.com" };
            _mockEmployeeService.Setup(service => service.UpdateEmployeeAsync(employee))
                .Throws(new Exception("Database error"));

            // Act
            var result = await _controller.Edit(1, employee) as ViewResult;

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.Multiple(() =>
            {
                Assert.That(result.Model, Is.EqualTo(employee));
                Assert.That(_controller.ModelState.ContainsKey(string.Empty), Is.True);
                Assert.That(
                    _controller.ModelState[string.Empty].Errors[0].ErrorMessage, 
                    Is.EqualTo("An error occurred while updating the employee. Please try again later."));
            });
        }

        [Test]
        public async Task Details_EmployeeFound_ShouldReturnViewWithEmployee()
        {
            // Arrange
            var employeeId = 1;
            var employee = new Employee { Id = 1, Name = "Anup", Email = "anup@ra.com" };
            _mockEmployeeService.Setup(service => service.GetEmployeeByIdAsync(employeeId))
                .ReturnsAsync(employee);

            // Act
            var result = await _controller.Details(employeeId) as ViewResult;

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Model, Is.EqualTo(employee));
        }

        [Test]
        public async Task Details_EmployeeNotFound_ShouldReturnNotFound()
        {
            // Arrange
            var employeeId = 1;
            Employee? employee = null;
            _mockEmployeeService.Setup(service => service.GetEmployeeByIdAsync(employeeId))
                .ReturnsAsync(employee);

            // Act
            var result = await _controller.Details(employeeId);

            // Assert
            Assert.That(result, Is.InstanceOf<NotFoundResult>());
        }

        [Test]
        public async Task Delete_EmployeeFound_ShouldReturnViewWithEmployee()
        {
            // Arrange
            var employeeId = 1;
            var employee = new Employee { Id = 1, Name = "Anup", Email = "anup@ra.com" };
            _mockEmployeeService.Setup(service => service.GetEmployeeByIdAsync(employeeId))
                .ReturnsAsync(employee);

            // Act
            var result = await _controller.Delete(employeeId) as ViewResult;

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Model, Is.EqualTo(employee));
            _mockEmployeeService.Verify(service => service.GetEmployeeByIdAsync(employeeId), Times.Once);
        }

        [Test]
        public async Task Delete_EmployeeNotFound_ShouldReturnNotFound()
        {
            // Arrange
            Employee? employee = null;
            var employeeId = 1;
            _mockEmployeeService.Setup(service => service.GetEmployeeByIdAsync(employeeId))
                .ReturnsAsync(employee);

            // Act
            var result = await _controller.Delete(employeeId);

            // Assert
            Assert.That(result, Is.InstanceOf<NotFoundResult>());
        }

        [Test]
        public async Task DeleteConfirmed_EmployeeFound_ShouldRedirectToIndex()
        {
            // Arrange
            var employeeId = 1;
            _mockEmployeeService.Setup(service => service.DeleteEmployeeAsync(employeeId))
                .Returns(Task.CompletedTask);

            // Act
            var result = await _controller.DeleteConfirmed(employeeId) as RedirectToActionResult;

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.ActionName, Is.EqualTo("Index"));
        }

        [Test]
        public void Error_ShouldLogExceptionDetails()
        {
            // Arrange
            var exceptionFeatureMock = new Mock<IExceptionHandlerPathFeature>();
            var exception = new Exception("Test error message");
            exceptionFeatureMock.Setup(x => x.Error).Returns(exception);
            exceptionFeatureMock.Setup(x => x.Path).Returns("/Home/Error");

            var mockHttpContext = new Mock<HttpContext>();
            mockHttpContext.Setup(x => x.Features.Get<IExceptionHandlerPathFeature>())
                .Returns(exceptionFeatureMock.Object);

            _controller = new EmployeeController(_mockEmployeeService.Object, _mockLogger.Object)
            {
                ControllerContext = new ControllerContext()
                {
                    HttpContext = mockHttpContext.Object
                }
            };

            // Act
            var result = _controller.Error() as ViewResult;

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.ViewName, Is.EqualTo("Error"));
        }
    }
}
