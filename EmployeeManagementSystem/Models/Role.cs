using System.ComponentModel.DataAnnotations;

namespace EmployeeManagementSystem.Models
{
    public class Role : BaseModel
    {
        [Required(ErrorMessage = "Name is required.")]
        [StringLength(100, ErrorMessage = "Name must not exceed 100 characters.")]
        public required string Name { get; set; }
        public ICollection<Employee> Employees { get; set; }
    }
}
