using BotPrecios.Bots;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BotPrecios.Model
{
    internal class Constants
    {
        internal const string Jumbo = "Jumbo";
        internal const string ChangoMas = "ChangoMas";
        internal const string Carrefour = "Carrefour";
        internal const string Coto = "Coto";

        internal const string dateFormat = "yyyy-MM-dd";

        internal struct ErrorLevel
        {
            internal const string Error = "ERROR";
            internal const string Debug = "DEBUG";
            internal const string Info = "INFO";
            internal const string Warning = "WARN";
        }
    }
}
