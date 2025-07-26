namespace InventoryService.Models
{
    public class ReserveRequest
    {
        public Guid ProductId { get; set; }
        public int Quantity { get; set; }
    }
}
