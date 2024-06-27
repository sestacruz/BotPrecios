using BotPrecios.Model;

namespace BotPrecios.Interfaces
{
    public interface IBot
    {
        public Task<List<Product>> GetProductsData();
        public void Dispose();
    }
}
