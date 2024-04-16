using Dapper;
using System.Data.SQLite;

namespace BotPrecios.Model
{
    public class Category
    {
        public string name { get; set; }
        public string url { get; set; }

        public bool AddToDatabase(string superMarket)
        {
            using var con = new SQLiteConnection($"Data Source={AppDomain.CurrentDomain.BaseDirectory}Precios.sqlite");
            con.Open();//conn.ExecuteScalar<bool>("select count(1) from Table where Id=@id", new {id});
            bool exists = con.ExecuteScalar<bool>($"SELECT Count(1) FROM Categories WHERE Name = @name", new { name });
            if (!exists)
            {
                return con.Execute($"INSERT INTO Categories VALUES (@superMarket,@name,@url)",
                    new { superMarket, name, url }) > 1;
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
