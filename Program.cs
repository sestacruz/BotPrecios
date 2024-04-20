using BotPrecios.Bots;
using BotPrecios.Helpers;
using BotPrecios.Interfaces;
using BotPrecios.Model;

string option = string.Empty;
if (args.Length > 0)
    option = args[0];

List<Product> products = new List<Product>();
IBot bot;

if (option == Constants.Jumbo || string.IsNullOrEmpty(option))
{
    bot = new Jumbo();
    products.AddRange(bot.GetProductsData());
    bot.Dispose();
}
if (option == Constants.ChangoMas || string.IsNullOrEmpty(option))
{
    bot = new ChangoMas();
    products.AddRange(bot.GetProductsData());
    bot.Dispose();
}
if (option == Constants.Carrefour || string.IsNullOrEmpty(option))
{
    bot = new Carrefour();
    products.AddRange(bot.GetProductsData());
    bot.Dispose();
}
if (option == Constants.Coto || string.IsNullOrEmpty(option))
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
    Utilities.WriteColor($"{category.name}: [{cant}]", ConsoleColor.Cyan);
}
if (option == "statistics" || string.IsNullOrEmpty(option))
{
    List<CBA> CBAs = StatisticsHelper.GetCBAStatistics();
    StatisticsHelper.GetMostsCBAs(CBAs);
    StatisticsHelper.GetTop5Categories();
    StatisticsHelper.GetTop5Products();

}
