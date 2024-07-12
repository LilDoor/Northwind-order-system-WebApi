using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Northwind.Services.Repositories;

namespace Northwind.Services.EntityFramework.Entities
{
    public class OrderDetail
    {
        [Key, Column(Order = 0)]
        public int OrderId { get; set; }

        [Key, Column(Order = 1)]
        public int ProductId { get; set; }

        [Required]
        public decimal UnitPrice { get; set; }

        [Required]
        public short Quantity { get; set; }

        [Required]
        public float Discount { get; set; }

        [ForeignKey("OrderId")]
        public Order Order { get; set; } = default!;

        [ForeignKey("ProductId")]
        public Product Product { get; set; } = default!;
    }
}
