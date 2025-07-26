namespace InventoryService.Dtos
{
    public class InventoryReadDto
    {
        public int Id { get; set; }

        public Guid ProductId { get; set; }

        public int Quantity { get; set; }
    }
}
