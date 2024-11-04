using BotPrecios.Model;

namespace BotPrecios.Bots
{
    public interface IBot
    {
        public Task<List<Product>> GetProductsData();
        public void Dispose();
    }
}
