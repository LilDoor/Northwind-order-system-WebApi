using System.ComponentModel.DataAnnotations;
using Northwind.Services.Repositories;

namespace Northwind.Services.EntityFramework.Entities
{
    public class Category
    {
        [Key]
        public int CategoryId { get; set; }

        [Required, MaxLength(15)]
        public string? CategoryName { get; set; }

        public string? Description { get; set; }

        private byte[] Picture { get; set; } = default!;

        public byte[] GetPicture()
        {
            return this.Picture;
        }
    }
}
