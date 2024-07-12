using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Northwind.Services.EntityFramework.Entities
{
    public class Shipper
    {
        [Key]
        public int ShipperId { get; set; }

        [Required, MaxLength(40)]
        public string? CompanyName { get; set; }

        [MaxLength(24)]
        public string? Phone { get; set; }

        public ICollection<Order> Orders { get; set; } = default!;
    }
}
