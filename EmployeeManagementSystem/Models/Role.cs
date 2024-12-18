﻿namespace EmployeeManagementSystem.Models
{
    public class Role
    {
        public int Id { get; set; }
        public required string Name { get; set; }
        public ICollection<Employee> Employees { get; set; }
    }
}
