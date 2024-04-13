using BotPrecios.Bots;
using BotPrecios.Interfaces;
using BotPrecios.Model;

string market = string.Empty;
if (args.Length > 0)
    market = args[0];

List<Product> products = new List<Product>();
IBot bot;

if (market == Constants.Jumbo || string.IsNullOrEmpty(market))
{
    bot = new Jumbo();
    bot.GetProductsData();
    bot.Dispose();
}
if (market == Constants.ChangoMas || string.IsNullOrEmpty(market))
{
    bot = new ChangoMas();
    bot.GetProductsData();
    bot.Dispose();
}
if (market == Constants.Carrefour || string.IsNullOrEmpty(market))
{
    bot = new Carrefour();
    bot.GetProductsData();
    bot.Dispose();
}
if (market == Constants.Coto || string.IsNullOrEmpty(market))
{
    bot = new Coto();
    bot.GetProductsData();
    bot.Dispose();
}

Console.WriteLine("Fin de la obtención de datos");