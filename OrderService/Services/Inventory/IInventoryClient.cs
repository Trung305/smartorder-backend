namespace OrderService.Services.Inventory
{
    public interface IInventoryClient
    {
        Task<bool> CheckAndReserveInventoryAsync(Guid ProductId, int quantity, string token);
        Task<bool> ReleaseInventoryAsync(Guid productId, int quantity, string token);
    }
}
