using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BotPrecios.Model
{
    internal class XResponse
    {
        public Data data { get; set; }

        public class Data
        {
            public string[] EditHistoryTweetIds { get; set; }
            public string Id { get; set; }
            public string Text { get; set; }
        }
    }
}
