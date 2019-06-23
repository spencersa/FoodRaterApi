using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Collections.Generic;
using System.Text;

namespace FoodRaterApi.Models
{
    public class RestaurantRatingTableEntity : TableEntity
    {
        public string RestaurantRowKey { get; set; }
        public string Rating { get; set; }
    }
}
