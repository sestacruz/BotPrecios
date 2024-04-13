using System.Text.Json;
using System.Text.RegularExpressions;

namespace BotPrecios
{
    public static class Helper
    {
        public static List<T> LoadJSONFile<T>(string path)
        {
            using StreamReader reader = new StreamReader(path);
            string json = reader.ReadToEnd();
            return JsonSerializer.Deserialize<List<T>>(json);
        }

        public static void WriteColor(string message, ConsoleColor foreColor, ConsoleColor backColor = ConsoleColor.Black)
        {
            var pieces = Regex.Split(message, @"(\[[^\]]*\])");
            foreach (var piece in pieces)
            {
                var textToWrite = piece;
                if (piece.StartsWith("[")  && piece.EndsWith("]"))
                {
                    Console.ForegroundColor = foreColor;
                    Console.BackgroundColor = backColor;
                    textToWrite = piece[1..^1];
                }
                Console.Write(textToWrite);
                Console.ResetColor();
            }
            Console.WriteLine();
        }

        public static void PrintProgressBar(string legend, int current, int total)
        {
            int progressLength = (int)Math.Ceiling((double)current / total * 100);
            string progressBar = "[" + new string('=', progressLength) + new string(' ', 100 - progressLength) + "]";

            Console.WriteLine();
            Console.WriteLine();
            Console.CursorLeft = 0;
            Console.CursorTop = Console.CursorTop -3;
            Console.Write($"{progressBar}{new string(' ',18)}{GetCenteredLegend(legend)}");
        }

        private static string GetCenteredLegend(string legend)
        {
            int spacesBefore = (102 - legend.Length) / 2;
            int spacesAfter = 102 - legend.Length - spacesBefore;
            return $"{new string(' ', spacesBefore)}{legend}{new string(' ', spacesAfter)}";
        }
    }
}
