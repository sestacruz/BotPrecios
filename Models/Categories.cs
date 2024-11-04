using BotPrecios.Services;
using Dapper;
using Microsoft.Extensions.Configuration;
using Supabase.Postgrest.Attributes;
using System.Data.SQLite;

namespace BotPrecios.Model
{
    [Table("Categories")]
    public class Category: Supabase.Postgrest.Models.BaseModel
    {
        [PrimaryKey("Id")]
        public int Id { get; set; }
        [Column("Supermarket")]
        public string supermarket { get; set; }
        [Column("Name")]
        public string name { get; set; }
        [Column("Uri")]
        public string url { get; set; }

        public async Task<bool> AddToDatabase(IConfiguration configuration)
        {
            SupabaseService supabaseHelper = new(configuration);
            return await supabaseHelper.UpsertData(this);
        }

        public static List<Category> GetAllCategories()
        {
            using var con = new SQLiteConnection($"Data Source={AppDomain.CurrentDomain.BaseDirectory}Precios.sqlite");
            con.Open();
            return con.Query<Category>("SELECT DISTINCT name FROM Categories").ToList();
        }
    }
}
