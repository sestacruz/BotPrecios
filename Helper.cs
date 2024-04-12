using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

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

        public static void WriteColor(string message, ConsoleColor color)
        {
            var pieces = Regex.Split(message, @"(\[[^\]]*\])");
            foreach (var piece in pieces)
            {
                var textToWrite = piece;
                if (piece.StartsWith("[")  && piece.EndsWith("]"))
                {
                    Console.ForegroundColor = color;
                    textToWrite = piece.Substring(1, piece.Length - 2);
                }
                Console.Write(textToWrite);
                Console.ResetColor();
            }
            Console.WriteLine();
        }
    }
}
