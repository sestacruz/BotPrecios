using Dapper;
using System.Data.SQLite;

namespace BotPrecios.Model
{
    public class Category
    {
        public string name { get; set; }
        public string url { get; set; }
        public decimal variation { get; set; }

        public bool AddToDatabase(string superMarket)
        {
            using var con = new SQLiteConnection($"Data Source={AppDomain.CurrentDomain.BaseDirectory}Precios.sqlite");
            con.Open();
            using var trx = con.BeginTransaction();
            bool exists = con.ExecuteScalar<bool>($"SELECT Count(1) FROM Categories WHERE Name = @name", new { name });
            if (!exists)
            {
                bool result = con.Execute($"INSERT INTO Categories VALUES (@superMarket,@name,@url)",
                    new { superMarket, name, url }) > 1;
                trx.Commit();
                return result;
            }
            else return true;
        }

        public static List<Category> GetAllCategories()
        {
            using var con = new SQLiteConnection($"Data Source={AppDomain.CurrentDomain.BaseDirectory}Precios.sqlite");
            con.Open();
            return con.Query<Category>("SELECT DISTINCT name FROM Categories").ToList();
        }
    }
}
