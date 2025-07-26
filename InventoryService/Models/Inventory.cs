namespace InventoryService.Models
{
    public class Inventory
    {
        public int Id { get; set; }

        public Guid ProductId { get; set; } 

        public Guid StoreId { get; set; } 

        public int Quantity { get; set; }
    }
}
