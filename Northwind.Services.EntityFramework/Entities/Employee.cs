using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Northwind.Services.Repositories;

namespace Northwind.Services.EntityFramework.Entities
{
    public class Employee
    {
        [Key]
        public int EmployeeId { get; set; }

        [Required, MaxLength(20)]
        public string? LastName { get; set; }

        [Required, MaxLength(10)]
        public string? FirstName { get; set; }

        [MaxLength(30)]
        public string Title { get; set; } = default!;

        [MaxLength(25)]
        public string TitleOfCourtesy { get; set; } = default!;

        public DateTime? BirthDate { get; set; }

        public DateTime? HireDate { get; set; }

        [MaxLength(60)]
        public string? Address { get; set; }

        [MaxLength(15)]
        public string? City { get; set; }

        [MaxLength(15)]
        public string? Region { get; set; }

        [MaxLength(10)]
        public string? PostalCode { get; set; }

        [MaxLength(15)]
        public string? Country { get; set; }

        [MaxLength(24)]
        public string? HomePhone { get; set; }

        public string? Extension { get; set; }

        public string? Notes { get; set; }

        public int? ReportsTo { get; set; }

        [MaxLength(255)]
        public string PhotoPath { get; set; } = default!;

        [ForeignKey("ReportsTo")]
        public Employee Manager { get; set; } = default!;

        public ICollection<Order> Orders { get; set; } = default!;

        public ICollection<Employee> Subordinates { get; set; } = default!;

        private byte[] Photo { get; set; } = default!;

        public byte[] GetPhoto()
        {
            return this.Photo;
        }
    }
}
