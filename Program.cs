using BotPrecios.Bots;
using BotPrecios.Helpers;
using BotPrecios.Interfaces;
using BotPrecios.Model;
using Microsoft.Extensions.Configuration;

string option = string.Empty;
if (args.Length > 0)
    option = args[0];

var builder = new ConfigurationBuilder();
builder.SetBasePath(Directory.GetCurrentDirectory())
   .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);

IConfiguration config = builder.Build();

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
    StatisticsHelper.GetTop5Categories(out List<CBA> topPositiveCat, out List<CBA> topNegativeCat);
    StatisticsHelper.GetTop5Products(out List<CBA> topPositiveProd, out List<CBA> topNegativeProd);
    StatisticsHelper.GetMostsCBAs(CBAs,out string expensive,out string cheapest);
    if (args.Length > 1)
    {
        Console.WriteLine("Posteando en X");
        string apiUrl = config["ApiURL"];
        PostsHelper postsHelper = new(apiUrl);
        postsHelper.PublishMontlyCBA(CBAs);
        postsHelper.PublishTop5CheapestCategory(topPositiveCat);
        postsHelper.PublishTop5MostExpensiveCategory(topNegativeCat);
        //postsHelper.PublishTop5CheapestProduct(topPositiveProd);
        //postsHelper.PublishTop5MostExpensiveProduct(topNegativeProd);
        DateTime today = DateTime.Now;
        DateTime lastDayOfMonth = new (today.Year,today.Month,DateTime.DaysInMonth(today.Year,today.Month));
        if (today.Date == lastDayOfMonth.Date)
            postsHelper.PublisTopMonthCBAs(expensive, cheapest);
        
    }
}

