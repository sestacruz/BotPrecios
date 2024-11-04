using BotPrecios.Services;
using Dapper;
using Microsoft.Extensions.Configuration;
using System.Data.SQLite;

namespace BotPrecios.Model
{
    public class Product : Supabase.Postgrest.Models.BaseModel
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

        public static async Task<bool> AddAllToDataBase(IConfiguration configuration, List<Product> products)
        {
            SupabaseService supabaseHelper = new(configuration);
            return await supabaseHelper.InsertData(products);
        }

        public static async void CleanProducts(IConfiguration configuration, string superMarket, DateTime date)
        {
            var options = new Supabase.SupabaseOptions
            {
                AutoConnectRealtime = false
            };

            var supabase = new Supabase.Client(configuration["SupabaseUrl"]!, configuration["SupabaseKey"], options);
            await supabase.InitializeAsync();
            await supabase.From<Product>()
                          .Where(x => x.priceDate.Equals(date) && x.superMarket == superMarket)
                          .Delete();
        }
    }
}
