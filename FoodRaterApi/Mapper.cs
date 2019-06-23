using FoodRaterApi.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace FoodRaterApi
{
    public static class Mapper
    {
        public static RestaurantTableEntity ToNewRestaurantTableEntity(this Restaurant restaurant)
        {
            return new RestaurantTableEntity()
            {
                PartitionKey = "Partition1",
                RowKey = Guid.NewGuid().ToString(),
                Name = restaurant.Name,
                Address = restaurant.Address,
                Description = restaurant.Description,
                Rating = restaurant.Rating.ToString(),
                Hours = restaurant.Hours
            };
        }

        public static Restaurant ToRestaurant(this RestaurantTableEntity restaurantTableEntity)
        {
            return new Restaurant()
            {
                Name = restaurantTableEntity.Name,
                Address = restaurantTableEntity.Address,
                Description = restaurantTableEntity.Description,
                Rating = float.Parse(restaurantTableEntity.Rating),
                Hours = restaurantTableEntity.Hours
            };
        }
    }
}
