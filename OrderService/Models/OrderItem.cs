namespace OrderService.Models
{
    public class OrderItem
    {
        public int Id { get; set; }

        public Guid ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;

        public int Quantity { get; set; }

        public decimal UnitPrice { get; set; }

        // Navigation property
        public Guid OrderId { get; set; }
    }
}
