using BotPrecios.Model;
using System.Net.NetworkInformation;
using System.Text.RegularExpressions;

namespace BotPrecios.Logging
{
    public class LogService : ILogService
    {
        private readonly string _logPath;
        private static readonly SemaphoreSlim semaphore = new SemaphoreSlim(1, 1);

        public LogService(string name)
        {
            _logPath = ".\\Logs\\";
            if (!Directory.Exists(_logPath))
                Directory.CreateDirectory(_logPath);
            _logPath += $"{DateTime.Now:yyyyMMdd}_{name}.log";
        }

        public async Task ConsoleLog(string message, string errorLevel = Constants.ErrorLevel.Info, ConsoleColor foreColor = ConsoleColor.Gray, ConsoleColor backColor = ConsoleColor.Black)
        {
            string log = $"[{DateTime.Now:yyyy/MM/dd HH:mm:ss}]|[{errorLevel}]| {message}";
            string[] pieces = log.Split('|');
            bool error = errorLevel == Constants.ErrorLevel.Error;

            foreach (var piece in pieces)
            {
                if (error)
                {
                    if (piece == $"[{Constants.ErrorLevel.Error}]")
                        Console.ForegroundColor = ConsoleColor.Red;
                    else if (piece.StartsWith(' '))
                        WriteColor(piece, ConsoleColor.White, ConsoleColor.Red);
                }
                else
                {
                    if (piece.Contains(Constants.ErrorLevel.Info))
                        Console.ForegroundColor = ConsoleColor.Cyan;
                    else if (piece == $"[{Constants.ErrorLevel.Debug}]")
                        Console.ForegroundColor = ConsoleColor.Green;
                    else if (piece == $"[{Constants.ErrorLevel.Warning}]")
                        Console.ForegroundColor = ConsoleColor.Yellow;
                    else if (piece.StartsWith(" ("))
                    {
                        string auxPiece = piece.Replace("(", "").Replace(")", " ");
                        var smForeColor = Console.ForegroundColor;
                        var smBackColor = Console.BackgroundColor;
                        string smName = auxPiece.Split(' ')[1];
                        if (smName.Contains(Constants.Coto))
                        {
                            smForeColor = ConsoleColor.White;
                            smBackColor = ConsoleColor.Red;
                        }
                        else if (smName.Contains(Constants.Jumbo))
                        {
                            smForeColor = ConsoleColor.White;
                            smBackColor = ConsoleColor.Green;
                        }
                        else if (smName.Contains(Constants.Carrefour))
                            smForeColor = ConsoleColor.Blue;
                        else if (smName.Contains(Constants.ChangoMas))
                            smForeColor = ConsoleColor.Yellow;

                        WriteColor($"[{smName}]", smForeColor, smBackColor);
                        WriteColor(auxPiece.Replace(smName,""), foreColor, backColor);
                    }

                    if (piece.StartsWith(' ') && !piece.StartsWith(" ("))
                        WriteColor(piece, foreColor, backColor);
                }
                if (!piece.StartsWith(' '))
                    Console.Write(piece);
                Console.ResetColor();
            }
            Console.WriteLine();
            await WriteLogAsync(log);
            //File.AppendAllLines(_logPath, [log]);
        }

        private async Task WriteLogAsync(string log)
        {
            await semaphore.WaitAsync();
            try
            {
                await File.AppendAllLinesAsync(_logPath, [log]);
            }
            finally
            {
                semaphore.Release();
            }
        }

        private static void WriteColor(string message, ConsoleColor foreColor, ConsoleColor backColor = ConsoleColor.Black)
        {
            var pieces = Regex.Split(message, @"(\[[^\]]*\])");
            foreach (var piece in pieces)
            {
                var textToWrite = piece;
                if (piece.StartsWith('[') && piece.EndsWith(']'))
                {
                    Console.ForegroundColor = foreColor;
                    Console.BackgroundColor = backColor;
                    textToWrite = piece[1..^1];
                }
                Console.Write(textToWrite);
                Console.ResetColor();
            }
        }

        public static string GetCenteredLegend(string legend, int totalChar = 102)
        {
            legend = legend.Length > totalChar ? string.Concat(legend.AsSpan(0, totalChar - 3), "...") : legend;
            int spacesBefore = (totalChar - legend.Length) / 2;
            int spacesAfter = totalChar - legend.Length - spacesBefore;
            return $"{new string(' ', spacesBefore)}{legend}{new string(' ', spacesAfter)}";
        }
        public void PrintProgressBar(string legend, int current, int total)
        {
            int progressLength = (int)Math.Ceiling((double)current / total * 100);
            string progressBar = "[" + new string('=', progressLength) + new string(' ', 100 - progressLength) + "]";

            Console.WriteLine();
            Console.WriteLine();
            Console.CursorLeft = 0;
            Console.CursorTop =- 3;
            Console.Write($"{progressBar}{new string(' ', 18)}{GetCenteredLegend(legend)}");
        }

    }
}
