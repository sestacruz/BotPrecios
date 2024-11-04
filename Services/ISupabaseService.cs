using Supabase.Postgrest.Models;

namespace BotPrecios.Services
{
    public interface ISupabaseService
    {
        Task<List<T>> FetchData<T>(T model) where T : BaseModel, new();
        Task<bool> InsertData<T>(T model) where T : BaseModel, new();
        Task<bool> InsertData<T>(List<T> model) where T : BaseModel, new();
        Task<bool> UpsertData<T>(T model) where T : BaseModel, new();
    }
}
