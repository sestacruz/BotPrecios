﻿using BotPrecios.Model;

namespace BotPrecios.Logging
{
    public interface ILogService
    {
        public Task ConsoleLog(string message, string errorLevel = Constants.ErrorLevel.Info, ConsoleColor foreColor = ConsoleColor.Gray, ConsoleColor backColor = ConsoleColor.Black);
        public void PrintProgressBar(string legend, int current, int total);
    }
}
