namespace ProductService.Service.Inventory
{
    public interface IInventoryService
    {
        Task<int> GetQuantityAsync(Guid productId);
        Task<Dictionary<Guid, int>> GetAllQuantitiesAsync();
    }
}
