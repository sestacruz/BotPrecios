using BotPrecios;
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
    products.AddRange(bot.GetProductsData());
    bot.Dispose();
}
if (market == Constants.ChangoMas || string.IsNullOrEmpty(market))
{
    bot = new ChangoMas();
    products.AddRange(bot.GetProductsData());
    bot.Dispose();
}
if (market == Constants.Carrefour || string.IsNullOrEmpty(market))
{
    bot = new Carrefour();
    products.AddRange(bot.GetProductsData());
    bot.Dispose();
}
if (market == Constants.Coto || string.IsNullOrEmpty(market))
{
    bot = new Coto();
    products.AddRange(bot.GetProductsData());
    bot.Dispose();
}
Console.WriteLine("Fin de la obtención de datos");
Console.WriteLine(new string('-', 120));

Console.WriteLine("Se obtuvieron los siguientes productos según las categorías:");
List<Category> categories = Category.GetAllCategories();
foreach (Category category in categories)
{
    int cant = products.Count(p => p.category == category.name);
    Helper.WriteColor($"{category.name}: [{cant}]", ConsoleColor.Cyan);
}
