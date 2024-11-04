namespace BotPrecios.Services
{
    public class SupabaseSettings : ISupabaseSettings
    {
        public string Url { get; init; }
        public string Key { get; init; }

        public SupabaseSettings(string url, string key)
        {
            Url = url;
            Key = key;
        }
    }
}
