using Microsoft.Extensions.Configuration;

namespace BotPrecios.Services
{
    internal class SupabaseService(IConfiguration _configuration) : ISupabaseService
    {
        readonly string url = _configuration["SupabaseUrl"]!;
        readonly string key = _configuration["SupabaseKey"]!;

        public async Task<List<T>> FetchData<T>(T model) where T : Supabase.Postgrest.Models.BaseModel, new()
        {
            var options = new Supabase.SupabaseOptions
            {
                AutoConnectRealtime = false
            };

            var supabase = new Supabase.Client(url, key, options);
            await supabase.InitializeAsync();
            var result = await supabase.From<T>().Get();
            return result.Models;
        }

        public async Task<bool> InsertData<T>(T model) where T : Supabase.Postgrest.Models.BaseModel, new()
        {
            var options = new Supabase.SupabaseOptions
            {
                AutoConnectRealtime = false
            };

            var supabase = new Supabase.Client(url, key, options);
            await supabase.InitializeAsync();
            await supabase.From<T>().Insert(model);

            return true;
        }

        public async Task<bool> InsertData<T>(List<T> model) where T : Supabase.Postgrest.Models.BaseModel, new()
        {
            var options = new Supabase.SupabaseOptions
            {
                AutoConnectRealtime = false
            };

            var supabase = new Supabase.Client(url, key, options);
            await supabase.InitializeAsync();
            await supabase.From<T>().Insert(model);

            return true;
        }

        public async Task<bool> UpsertData<T>(T model) where T : Supabase.Postgrest.Models.BaseModel, new()
        {
            var options = new Supabase.SupabaseOptions
            {
                AutoConnectRealtime = false
            };

            var supabase = new Supabase.Client(url, key, options);
            await supabase.InitializeAsync();
            await supabase.From<T>().Upsert(model);        
            
            return true;
        }
    }
}
