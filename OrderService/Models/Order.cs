namespace OrderService.Models
{
    public class Order
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string UserId { get; set; }
        public string CustomerName { get; set; } = string.Empty;

        public DateTime OrderDate { get; set; } = DateTime.UtcNow;

        public List<OrderItem> Items { get; set; } = new();

        public decimal TotalAmount
        {
            get => Items.Sum(item => item.Quantity * item.UnitPrice);
        }
        public bool IsCanceled { get; set; } = false;
    }
}
