using System.ComponentModel.DataAnnotations;

namespace Entities.DTOs
{
    public class ProductCreateDto
    {
        public Guid? ID { get; set; }

        [Required(ErrorMessage = "This field is required")]
        public string ProductName { get; set; } = string.Empty;

        [Required(ErrorMessage = "This field is required")]
        public Guid CategoryID { get; set; } 

        [Required(ErrorMessage = "This field is required")]
        public Guid SupplierID { get; set; }

        [Required(ErrorMessage = "This field is required")]
        public decimal UnitPrice { get; set; }

        [Required(ErrorMessage = "This field is required")]
        public int UnitsInStock { get; set; }

        public bool? Discontinued { get; set; } = false;
    }
}