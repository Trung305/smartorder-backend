namespace InventoryService.Dtos
{
    public class InventoryCreateDto
    {
        public Guid ProductId { get; set; }

        public int Quantity { get; set; }
    }
}
