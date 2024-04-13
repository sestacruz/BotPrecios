using BotPrecios.Model;

namespace BotPrecios.Interfaces
{
    public interface IBot
    {
        public List<Product> GetProductsData();
        public void Dispose();
    }
}
