namespace OrderService.Dtos
{
    public class OrderReadDto
    {
        public Guid Id { get; set; }
        public string UserId { get; set; }
        public string CustomerName { get; set; }
        public DateTime OrderDate { get; set; }

        public decimal TotalAmount { get; set; }  // tính từ Items
        public List<OrderItemDto> Items { get; set; }
        public bool IsCanceled { get; set; } = false;
    }
}
