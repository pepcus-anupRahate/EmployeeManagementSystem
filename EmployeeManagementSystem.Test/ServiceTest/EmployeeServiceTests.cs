using EmployeeManagementSystem.Data;
using EmployeeManagementSystem.Models;
using EmployeeManagementSystem.Services;
using Microsoft.EntityFrameworkCore;

namespace EmployeeManagementSystem.Test.ServiceTest
{
    [TestFixture]
    public class EmployeeServiceTests
    {
        private EmployeeService _employeeService;
        private AppDbContext _dbContext;

        [SetUp]
        public void SetUp()
        {
            // Set up an in-memory database
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            _dbContext = new AppDbContext(options);
            _dbContext.Database.EnsureCreated();           

            _dbContext.Employees.AddRange(new List<Employee>
                {
                    new() { Id = 1, Name = "Anup Rahate", RoleId = 1, Email = "anup@jn.com" },
                    new() { Id = 2, Name = "Shubham Prasad", RoleId = 2, Email = "shubham@jn.com" }
                });
            _dbContext.SaveChanges();

            _employeeService = new EmployeeService(_dbContext);
        }

        [TearDown]
        public void TearDown()
        {
            _dbContext.Database.EnsureDeleted();
            _dbContext.Dispose();
        }

        [Test]
        public async Task GetAllEmployeesAsync_ShouldReturnListOfEmployees()
        {
            // Arrange

            // Act
            var result = await _employeeService.GetAllEmployeesAsync();

            // Assert
            Assert.That(result.Count(), Is.EqualTo(2));
        }

        [Test]
        public async Task GetEmployeeByIdAsync_ShouldReturnEmployee_WhenEmployeeExists()
        {
            // Act
            var result = await _employeeService.GetEmployeeByIdAsync(1);

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.Multiple(() =>
            {
                Assert.That(result.Id, Is.EqualTo(1));
                Assert.That(result.Name, Is.EqualTo("Anup Rahate"));
            });
        }

        [Test]
        public async Task AddEmployeeAsync_ShouldAddEmployeeToDb()
        {
            // Arrange
            var newEmployee = new Employee { Id = 3, Name = "Sam Smith", Email = "sam@sam.com"};

            // Act
            await _employeeService.AddEmployeeAsync(newEmployee);

            // Assert
            var savedEmployee = await _dbContext.Employees.FindAsync(3);
            Assert.That(savedEmployee, Is.Not.Null);
            Assert.That(savedEmployee.Name, Is.EqualTo("Sam Smith"));
        }

        [Test]
        public async Task UpdateEmployeeAsync_ShouldUpdateEmployeeInDb()
        {
            // Arrange
            var employee = _dbContext.Employees.FirstOrDefault(x => x.Id == 1);
            employee.Name = "Updated Name";

            // Act
            await _employeeService.UpdateEmployeeAsync(employee);

            // Assert
            var updatedEmployee = await _dbContext.Employees.FindAsync(1);
            Assert.That(updatedEmployee.Name, Is.EqualTo("Updated Name"));
        }

        [Test]
        public async Task DeleteEmployeeAsync_ShouldDeleteEmployee_WhenEmployeeExists()
        {
            // Arrange
            var employee = await _dbContext.Employees.FindAsync(1);

            // Act
            await _employeeService.DeleteEmployeeAsync(1);

            // Assert
            var deletedEmployee = await _dbContext.Employees.FindAsync(1);
            Assert.That(deletedEmployee, Is.Null);
        }

        [Test]
        public async Task GetRolesAsync_ShouldReturnListOfRoles()
        {
            // Act
            var result = await _employeeService.GetRolesAsync();

            // Assert
            Assert.That(result, Has.Count.EqualTo(3));
        }
    }
}
