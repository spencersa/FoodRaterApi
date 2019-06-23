using FoodRaterApi.Models;
using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Collections.Generic;
using System.Text;

namespace FoodRaterApi
{
    public class RestaurantTableEntity : TableEntity
    {
        public string Name { get; set; }
        public string Address { get; set; }
        public string Description { get; set; }
        public string Hours { get; set; }
        public string Rating { get; set; }
    }
}
