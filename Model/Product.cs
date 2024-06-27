using Dapper;
using System.ComponentModel.DataAnnotations;
using System.Data;
using System.Data.SQLite;
using System.Diagnostics;
using System.Text.RegularExpressions;

namespace BotPrecios.Model
{
    public class Product
    {
        public string superMarket {  get; set; }
        public string name { get; set; }
        public string category { get; set; }
        public decimal? price { get; set; }
        public decimal todayPrice { get; set; }               
        public string priceDate { get; set; }
        public DateTime? PriceDate => priceDate == null ? null : DateTime.ParseExact(priceDate,"yyyy-MM-dd",null);
        public decimal variation { get; set; }

        public override string ToString()
        {
            return $"{superMarket};{name};{category};{price}";
        }

        public bool AddToDataBase()
        {
            bool result = false;
            using var con = new SQLiteConnection($"Data Source={AppDomain.CurrentDomain.BaseDirectory}Precios.sqlite");
            con.Open();
            using var trx = con.BeginTransaction();
            result = con.Execute($"INSERT INTO Products VALUES (@superMarket,@category,@name,@price,@priceDate)", 
            new { superMarket, name, category, price, priceDate = DateTime.Now.ToString(Constants.dateFormat) }) > 1;
            trx.Commit();
            return result;
        }

        public static bool AddAllToDataBase(List<Product> products)
        {
            bool result = false;
            using var con = new SQLiteConnection($"Data Source={AppDomain.CurrentDomain.BaseDirectory}Precios.sqlite");
            con.Open();
            using var trx = con.BeginTransaction();
            foreach (Product product in products)
            {
                result &= con.Execute($"INSERT INTO Products VALUES (@superMarket,@category,@name,@price,@priceDate)",
                new { product.superMarket, product.name, product.category, product.price, priceDate = DateTime.Now.ToString(Constants.dateFormat) }) > 1;
            }
            trx.Commit();
            return result;
        }

        public static Product GetProductByDate(Product product, DateTime date)
        {
            using var con = new SQLiteConnection($"Data Source={AppDomain.CurrentDomain.BaseDirectory}Precios.sqlite");
            con.Open();
            Product? result = con.QueryFirstOrDefault<Product>("SELECT * FROM Products " +
                                                "WHERE Supermarket = @superMarket " +
                                                "AND Category = @category " +
                                                "AND Name = @name " +
                                                "AND PriceDate = @priceDate",
                                                new { product.superMarket, product.category, product.name, priceDate = date.ToString(Constants.dateFormat) });
            return result;
        }

        public static List<Product> GetProductsByCategory(string category, DateTime? date = null)
        {
            using var con = new SQLiteConnection($"Data Source={AppDomain.CurrentDomain.BaseDirectory}Precios.sqlite");
            con.Open();

            string sql = "SELECT * FROM Products WHERE Category = @category";
            if (date.HasValue)
                sql += " AND PriceDate = @priceDate";

            object parameters = new { category };
            if (date.HasValue)
                parameters = new { category, priceDate = date.Value.ToString(Constants.dateFormat) };

            List<Product> products = con.Query<Product>(sql, parameters).ToList();

            return products;
        }

        public static async Task CleanProducts(string superMarket, DateTime date)
        {
            string connectionString = $"Data Source={AppDomain.CurrentDomain.BaseDirectory}Precios.sqlite";
            await ExecuteWithRetryAsync(async transaction =>
            {
                var parameters = new { superMarket, priceDate = date.ToString(Constants.dateFormat) };
                string sql = "DELETE FROM Products WHERE Supermarket = @superMarket AND PriceDate = @priceDate";
                await transaction.Connection.ExecuteAsync(sql, parameters, transaction);
            }, connectionString);
        }

        private static async Task ExecuteWithRetryAsync(Func<IDbTransaction, Task> action, string connectionString, int maxRetries = 5)
        {
            using var connection = new SQLiteConnection(connectionString);
            await connection.OpenAsync();
            using var transaction = connection.BeginTransaction();
            int retries = 0;
            while (true)
            {
                try
                {
                    await action(transaction);
                    transaction.Commit();
                    break;
                }
                catch (SQLiteException ex) when (ex.ResultCode == SQLiteErrorCode.Busy || ex.ResultCode == SQLiteErrorCode.Locked)
                {
                    retries++;
                    if (retries >= maxRetries)
                        throw;

                    await Task.Delay(100 * (int)Math.Pow(2, retries));
                }
                catch
                {
                    transaction.Rollback();
                    throw;
                }
            }
        }

    }
}
