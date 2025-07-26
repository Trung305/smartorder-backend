namespace InventoryService.Dtos
{
    public class InventoryUpdateDto
    {
        public int Id { get; set; }

        public Guid ProductId { get; set; }

        public int Quantity { get; set; }
    }
}
