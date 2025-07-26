namespace InventoryService.Dtos
{
    public class InventoryWithProductNameDto
    {
        public Guid ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public int Quantity { get; set; }
    }
}
