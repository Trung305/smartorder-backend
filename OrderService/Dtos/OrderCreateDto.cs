namespace OrderService.Dtos
{
    public class OrderCreateDto
    {
        public string CustomerName { get; set; } = string.Empty;
        public DateTime OrderDate { get; set; }
        public List<OrderItemDto> Items { get; set; } = new();
        public bool IsCanceled { get; set; } = false;
    }
}
