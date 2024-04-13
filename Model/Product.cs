using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace BotPrecios.Model
{
    public class Product
    {
        private string _price;
        public string name { get; set; }
        public string category { get; set; }
        public string price 
        { 
            get => _price;
            set => _price = Regex.Replace(value, @"[^\d,]", "").Trim();
        }

        public override string ToString()
        {
            return $"{name};{category};{price}";
        }
    }
}
