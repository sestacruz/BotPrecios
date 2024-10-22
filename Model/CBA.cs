using BotPrecios.Helpers;
using Dapper;
using System.Data.SQLite;

namespace BotPrecios.Model
{
    internal class CBA
    {
        public string SuperMarket { get; set; }
        public string Category { get; set; }
        public string Product { get; set; }
        public decimal TotalPrice { get; set; }
        public int ProductCount { get; set; }
        public decimal Variation { get; set; }

        public void GetAccumCBABySupermarket(string superMarket)
        {
            List<CBAData> CBAProducts = Utilities.LoadJSONFile<CBAData>(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, $"Categories\\{superMarket}CBAData.json"));
            this.SuperMarket = superMarket;
            CBA cbaYesterday = new() { SuperMarket = superMarket};

            DateTime firstDay = new(DateTime.Now.Year, DateTime.Now.Month, 1);
            string sql = "SELECT Supermarket,SUM(price) as totalPrice, COUNT(*) as ProductCount FROM Products " +
                         "WHERE Supermarket = @superMarket " +
                         "AND Name IN @names " +
                         "AND UPPER(Category) = @category " +
                         "AND PriceDate = @priceDate " +
                         "GROUP BY Supermarket";

            using var con = new SQLiteConnection($"Data Source={AppDomain.CurrentDomain.BaseDirectory}Precios.sqlite");
            con.Open();
            foreach (CBAData item in CBAProducts)
            {
                
                CBA? resultToday = con.QueryFirstOrDefault<CBA>(sql, 
                    new
                    {
                        superMarket,
                        category = item.category.ToUpper(),
                        item.names,
                        priceDate = DateTime.Now.ToString(Constants.dateFormat)
                    });
                
                CBA? resultYesterday = con.QueryFirstOrDefault<CBA>(sql,
                    new
                    {
                        superMarket,
                        category = item.category.ToUpper(),
                        item.names,
                        priceDate = firstDay.ToString(Constants.dateFormat)
                    });
                
                //Si el producto no se encuentra en alguna de las 2 consultas, se excluye para no interferir en la medicion
                if (resultToday == null || resultYesterday == null)
                    continue;
                
                if(resultToday != null )
                    TotalPrice += resultToday.TotalPrice / resultToday.ProductCount;
                if (resultYesterday != null)
                    cbaYesterday.TotalPrice += resultYesterday.TotalPrice / resultYesterday.ProductCount;
            }
            Variation = CalculateVariation(TotalPrice, cbaYesterday.TotalPrice);
        }

        public static List<CBA> GetCategoriesVariation()
        {
            DateTime firstDate = new(DateTime.Now.Year, DateTime.Now.Month, 1);
            string sql = "SELECT Supermarket, Category, ROUND(SUM(Price),2) as Price, PriceDate " +
                         "FROM Products " +
                         "WHERE PriceDate IN (@firstDate, @today) "+
                         "GROUP BY Supermarket,Category,PriceDate";
            using var con = new SQLiteConnection($"Data Source={AppDomain.CurrentDomain.BaseDirectory}Precios.sqlite");
            con.Open();
            List<Product> products = con.Query<Product>(sql, new { today = DateTime.Now.ToString(Constants.dateFormat), firstDate = firstDate.ToString(Constants.dateFormat) }).ToList();
            List<Product> todayProducts = products.Where(x => x.PriceDate == DateTime.Now.Date).ToList();
            List<Product> firstDayProducts = products.Where(x => x.PriceDate == firstDate).ToList();

            List<CBA> result = [];
            foreach (Product product in firstDayProducts)
            {
                if (todayProducts.Exists(x => x.category == product.category))
                {
                    decimal todayPrice = (decimal)todayProducts.Where(x => x.category == product.category).First().price;
                    decimal originalPrice = (decimal)product.price;
                    decimal variation = CalculateVariation(todayPrice,originalPrice);
                    result.Add(new CBA
                    {
                        Category = product.category,
                        SuperMarket = product.superMarket,
                        TotalPrice = todayPrice,
                        Variation = variation
                    });
                }
            }
            return result;
        }

        public static List<CBA> GetProductsVariation(out List<CBA> categoriesVariation)
        {
            DateTime firstDate = new(DateTime.Now.Year, DateTime.Now.Month, 1);
            string sql = @"SELECT p1.Name, p1.Category, p1.Supermarket,
	                               ROUND(p2.Price) AS Price, 
	                               ROUND(p1.Price) AS TodayPrice
                            FROM Products p1
                            JOIN (SELECT Name,Category,Supermarket,Price,PriceDate
                                  FROM Products
	                              WHERE PriceDate=@firstDate
	                              GROUP BY Name,Category,Supermarket
                            ) AS p2
                            ON p1.Name = p2.Name AND p1.Category = p2.Category AND p1.Supermarket = p2.Supermarket
                            WHERE p1.PriceDate = @today
                            AND p1.Price > 0
                            AND p2.Price > 0;";
            using var con = new SQLiteConnection($"Data Source={AppDomain.CurrentDomain.BaseDirectory}Precios.sqlite");
            con.Open();

            List<CBA> result = con.Query<Product>(sql, new { today = DateTime.Now.ToString(Constants.dateFormat), firstDate = firstDate.ToString(Constants.dateFormat) })
               .Select(p => new CBA
               {
                   Category = p.category,
                   Product = p.name,
                   SuperMarket = p.superMarket,
                   TotalPrice = p.todayPrice,
                   Variation = CalculateVariation(p.todayPrice,(decimal)p.price)
               })
               .ToList();

            // Calcular la variación de precios de las categorías
            categoriesVariation = result.GroupBy(r => new { r.Category, r.SuperMarket })
                                 .Select(group => new CBA
                                 {
                                     Category = group.Key.Category,
                                     SuperMarket = group.Key.SuperMarket,
                                     Variation = group.Average(p => p.Variation)
                                 })
                                 .ToList();
            return result;
        }

        private static decimal CalculateVariation(decimal actualPrice, decimal previousPrice)
        {
            return actualPrice == 0 ? 0 : decimal.Divide((actualPrice- previousPrice) *100, previousPrice);
        }
    }

    internal class CBAData
    {
        public string category { get; set; }
        public string[] names { get; set; }
    }
}
