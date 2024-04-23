using System.Text.Json;
using System.Text.RegularExpressions;

namespace BotPrecios.Helpers
{
    public static class Utilities
    {
        public static List<T> LoadJSONFile<T>(string path)
        {
            using StreamReader reader = new StreamReader(path);
            string json = reader.ReadToEnd();
            return JsonSerializer.Deserialize<List<T>>(json);
        }        
    }
}
