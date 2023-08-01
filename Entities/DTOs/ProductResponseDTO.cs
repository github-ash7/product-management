namespace Entities.DTOs
{
    public class ProductResponseDto
    {
        public Guid ID { get; set; }

        public string ProductName { get; set; } = string.Empty;

        public Guid CategoryID { get; set; }
        
        public Guid SupplierID { get; set; }
        
        public decimal UnitPrice { get; set; }
        
        public int UnitsInStock { get; set; }

        public bool Discontinued { get; set; }
    }
}