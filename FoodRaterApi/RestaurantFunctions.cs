using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using FoodRaterApi.Models;
using FoodRaterApi.Data;

namespace FoodRaterApi
{
    public class RestaurantFunctions
    {
        private CloudTableRepository _cloudTableRepository;

        public RestaurantFunctions(CloudTableRepository cloudTableRepository)
        {
            _cloudTableRepository = cloudTableRepository;
        }

        [FunctionName("GetAllRestaurants")]
        public async Task<IActionResult> GetAllRestaurants(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = null)]
            HttpRequest req)
        {
            var entities = await _cloudTableRepository.GetAllRestaurantsAsync();
            return new OkObjectResult(entities);
        }

        [FunctionName("GetRestaurant")]
        public async Task<IActionResult> GetRestaurant(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = null)]
            HttpRequest req)
        {
            string rowKey = req.Query["rowKey"];

            var restaurant = await _cloudTableRepository.GetRestaurantAsync(rowKey);

            return new OkObjectResult(restaurant);
        }

        [FunctionName("InsertRestaurant")]
        public async Task<IActionResult> InsertRestaurant(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = null)]
            HttpRequest req)
        {
            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            var restaurant = JsonConvert.DeserializeObject<Restaurant>(requestBody).ToNewRestaurantTableEntity();

            restaurant = await _cloudTableRepository.InsertRestaurantAsync(restaurant);

            return new OkObjectResult(restaurant);
        }

        [FunctionName("UpdateRestaurant")]
        public async Task<IActionResult> UpdateRestaurant(
            [HttpTrigger(AuthorizationLevel.Function, "put", Route = null)]
            HttpRequest req)
        {
            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            var restaurantToUpdate = JsonConvert.DeserializeObject<RestaurantTableEntity>(requestBody);

            var existingResult = await _cloudTableRepository.GetRestaurantAsync(restaurantToUpdate.RowKey);

            if (existingResult != null)
            {
                await _cloudTableRepository.UpdateRestaurantAsync(restaurantToUpdate);
            }
            else
            {
                return new NotFoundObjectResult($"Restaurant not found, Name: {restaurantToUpdate.Name}");
            }

            return new NoContentResult();
        }

        [FunctionName("AddRestaurantRating")]
        public async Task<IActionResult> AddRestaurantRating(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = null)]
            HttpRequest req)
        {
            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            var restaurantToUpdate = JsonConvert.DeserializeObject<RestaurantTableEntity>(requestBody);
            var existingResult = await _cloudTableRepository.GetRestaurantAsync(restaurantToUpdate.RowKey);

            if (existingResult != null)
            {
                var rating = new RestaurantRatingTableEntity().ToNewRestaurantRatingTableEntity();
                rating.RestaurantRowKey = restaurantToUpdate.RowKey;
                rating.Rating = restaurantToUpdate.Rating;

                await _cloudTableRepository.AddRestaurantRatingAsync(rating, existingResult);
            }
            else
            {
                return new NotFoundObjectResult($"Restaurant not found, Name: {restaurantToUpdate.Name}");
            }

            return new NoContentResult();
        }

        [FunctionName("DeleteRestaurant")]
        public async Task<IActionResult> DeleteRestaurant(
            [HttpTrigger(AuthorizationLevel.Function, "delete", Route = null)]
            HttpRequest req)
        {
            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            var restaurant = JsonConvert.DeserializeObject<RestaurantTableEntity>(requestBody);

            var existingResult = await _cloudTableRepository.GetRestaurantAsync(restaurant.RowKey);

            if (existingResult != null)
            {
                await _cloudTableRepository.DeleteRestaurantAsync(existingResult);
            }
            else
            {
                return new NotFoundObjectResult($"Restaurant not found, Name: {restaurant.Name}");
            }

            return new OkResult();
        }

        [FunctionName("SeedTable")]
        public async Task<IActionResult> SeedTable(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = null)]
            HttpRequest req)
        {
            await _cloudTableRepository.SeedTableAsync();
            return new OkResult();
        }

    }
}
