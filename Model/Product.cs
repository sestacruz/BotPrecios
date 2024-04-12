using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BotPrecios.Model
{
    public class Product
    {
        private string _price;
        public string name { get; set; }
        public string presentation { get; set; }
        public string price 
        { 
            get => _price;
            set => _price = value.Replace("$","").Replace(".","").Trim();
        }

        public override string ToString()
        {
            return $"{name};{presentation};{price}";
        }
    }
}
