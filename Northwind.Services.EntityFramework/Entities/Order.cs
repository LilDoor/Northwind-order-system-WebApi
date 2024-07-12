using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Northwind.Services.Repositories;

namespace Northwind.Services.EntityFramework.Entities
{
    public class Order
    {
        [Key]
        public long OrderId { get; set; }

        public string? CustomerId { get; set; }

        public long EmployeeId { get; set; }

        public DateTime OrderDate { get; set; }

        public DateTime RequiredDate { get; set; }

        public DateTime? ShippedDate { get; set; }

        public long ShipVia { get; set; }

        public double Freight { get; set; }

        public string? ShipName { get; set; }

        public string? ShipAddress { get; set; }

        public string? ShipCity { get; set; }

        public string? ShipRegion { get; set; }

        public string? ShipPostalCode { get; set; }

        public string? ShipCountry { get; set; }

        [ForeignKey("EmployeeId")]
        public Employee Employee { get; set; } = default!;

        [ForeignKey("CustomerId")]
        public Customer Customer { get; set; } = default!;

        [ForeignKey("ShipVia")]
        public Shipper Shipper { get; set; } = default!;

        public IList<OrderDetail> OrderDetails { get; } = new List<OrderDetail>();
    }
}
