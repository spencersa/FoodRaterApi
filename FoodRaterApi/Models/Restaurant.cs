using System;
using System.Collections.Generic;
using System.Text;

namespace FoodRaterApi.Models
{
    public class Restaurant
    {
        public string Name { get; set; }
        public string Address { get; set; }
        public string Description { get; set; }
        public string Hours { get; set; }
        public float Rating { get; set; }
        public DateTimeOffset LastUpdated { get; set; }
    }
}
