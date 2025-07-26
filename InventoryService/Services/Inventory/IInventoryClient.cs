namespace OrderService.Services.Inventory
{
    public interface IInventoryClient
    {
        Task<bool> CheckAndReserveInventoryAsync(string productName, int quantity);
    }
}
