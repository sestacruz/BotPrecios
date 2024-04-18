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
if (option == "statistics" || !string.IsNullOrEmpty(option))
{
    Statistics.GetCBAStatistics();
}

// Para obtener los 5 productos con mayor variación negativa y postiva de la semana, se deben
// obtener los precios de los productos de la semana anterior, calcular todas las variaciones,
// ordenar de mayor a menor y tomar los 5 primeros y los 5 últimos.

// Para obtener el costo de la CBA más alta y más baja del mes, se debe tomar el precio de la
// CBA a fin de mes y comparar todos los supermercados, ambas puntas serán los elegidos.

// Para obtener las 5 categorías con mayor variación negativa y postiva de la semana, se deben
// obtener los precios de los productos de la semana anterior, calcular todas las variaciones,
// ordenar de mayor a menor y tomar los 5 primeros y los 5 últimos.
