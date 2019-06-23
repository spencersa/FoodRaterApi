using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Microsoft.WindowsAzure.Storage.Table;
using System.Linq;
using Microsoft.WindowsAzure.Storage;
using System.Collections.Generic;
using FoodRaterApi.Models;
using AutoFixture;

namespace FoodRaterApi
{
    public static class RestaurantFunctions
    {
        [FunctionName("SeedTable")]
        public static async Task<IActionResult> SeedTable(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = null)]
            HttpRequest req,
            ILogger log)
        {
            CloudStorageAccount storageAccount = CloudStorageAccount.Parse("DefaultEndpointsProtocol=https;AccountName=foodraterapibbd2;AccountKey=6KUW7ksyYI2L9DfPQzdjo+dfF8uT53eZIdVOuJsjrxNA9vraGtDdv2n+1wmfZDBQSk0ePod+mnWk1ZfxHEtxsw==;BlobEndpoint=https://foodraterapibbd2.blob.core.windows.net/;QueueEndpoint=https://foodraterapibbd2.queue.core.windows.net/;TableEndpoint=https://foodraterapibbd2.table.core.windows.net/;FileEndpoint=https://foodraterapibbd2.file.core.windows.net/;");
            CloudTableClient tableClient = storageAccount.CreateCloudTableClient();
            CloudTable tableRestaurants = tableClient.GetTableReference("restaurants");
            CloudTable tableRestaurantRatings = tableClient.GetTableReference("restaurantsRatings");

            await tableRestaurants.CreateIfNotExistsAsync();
            await tableRestaurantRatings.CreateIfNotExistsAsync();

            var fixture = new Fixture();
            var restaurants = fixture
                .CreateMany<Restaurant>()
                .Select(Mapper.ToNewRestaurantTableEntity);


            TableBatchOperation batchOperation = new TableBatchOperation();
            foreach (var restaurant in restaurants)
            {
                batchOperation.Insert(restaurant);
            }

            await tableRestaurants.ExecuteBatchAsync(batchOperation);

            return new OkResult();
        }

        [FunctionName("GetAllRestaurants")]
        public static async Task<IActionResult> GetAllRestaurants(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = null)]
            HttpRequest req,
            ILogger log)
        {
            CloudStorageAccount storageAccount = CloudStorageAccount.Parse("DefaultEndpointsProtocol=https;AccountName=foodraterapibbd2;AccountKey=6KUW7ksyYI2L9DfPQzdjo+dfF8uT53eZIdVOuJsjrxNA9vraGtDdv2n+1wmfZDBQSk0ePod+mnWk1ZfxHEtxsw==;BlobEndpoint=https://foodraterapibbd2.blob.core.windows.net/;QueueEndpoint=https://foodraterapibbd2.queue.core.windows.net/;TableEndpoint=https://foodraterapibbd2.table.core.windows.net/;FileEndpoint=https://foodraterapibbd2.file.core.windows.net/;");
            CloudTableClient tableClient = storageAccount.CreateCloudTableClient();
            CloudTable table = tableClient.GetTableReference("restaurants");

            TableContinuationToken token = null;
            var entities = new List<RestaurantTableEntity>();
            do
            {
                var queryResult = await table.ExecuteQuerySegmentedAsync(new TableQuery<RestaurantTableEntity>(), token);
                entities.AddRange(queryResult.Results);
                token = queryResult.ContinuationToken;
            } while (token != null);


            return new OkObjectResult(entities);
        }

        [FunctionName("GetRestaurant")]
        public static async Task<IActionResult> GetRestaurant(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = null)]
            HttpRequest req,
            ILogger log)
        {
            CloudStorageAccount storageAccount = CloudStorageAccount.Parse("DefaultEndpointsProtocol=https;AccountName=foodraterapibbd2;AccountKey=6KUW7ksyYI2L9DfPQzdjo+dfF8uT53eZIdVOuJsjrxNA9vraGtDdv2n+1wmfZDBQSk0ePod+mnWk1ZfxHEtxsw==;BlobEndpoint=https://foodraterapibbd2.blob.core.windows.net/;QueueEndpoint=https://foodraterapibbd2.queue.core.windows.net/;TableEndpoint=https://foodraterapibbd2.table.core.windows.net/;FileEndpoint=https://foodraterapibbd2.file.core.windows.net/;");
            CloudTableClient tableClient = storageAccount.CreateCloudTableClient();
            CloudTable table = tableClient.GetTableReference("restaurants");

            string rowKey = req.Query["rowKey"];

            var restaurant = (RestaurantTableEntity)(await table.ExecuteAsync(TableOperation.Retrieve<RestaurantTableEntity>("Partition1", rowKey))).Result;

            return new OkObjectResult(restaurant);
        }

        [FunctionName("InsertRestaurant")]
        public static async Task<IActionResult> InsertRestaurant(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = null)]
            HttpRequest req,
            ILogger log)
        {
            CloudStorageAccount storageAccount = CloudStorageAccount.Parse("DefaultEndpointsProtocol=https;AccountName=foodraterapibbd2;AccountKey=6KUW7ksyYI2L9DfPQzdjo+dfF8uT53eZIdVOuJsjrxNA9vraGtDdv2n+1wmfZDBQSk0ePod+mnWk1ZfxHEtxsw==;BlobEndpoint=https://foodraterapibbd2.blob.core.windows.net/;QueueEndpoint=https://foodraterapibbd2.queue.core.windows.net/;TableEndpoint=https://foodraterapibbd2.table.core.windows.net/;FileEndpoint=https://foodraterapibbd2.file.core.windows.net/;");
            CloudTableClient tableClient = storageAccount.CreateCloudTableClient();
            CloudTable table = tableClient.GetTableReference("restaurants");

            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            var restaurant = JsonConvert.DeserializeObject<Restaurant>(requestBody).ToNewRestaurantTableEntity();
            var insertOperation = TableOperation.Insert(restaurant);
            await table.ExecuteAsync(insertOperation);

            return new OkObjectResult(restaurant);
        }

        [FunctionName("UpdateRestaurant")]
        public static async Task<IActionResult> UpdateRestaurant(
            [HttpTrigger(AuthorizationLevel.Function, "put", Route = null)]
            HttpRequest req,
            ILogger log)
        {
            CloudStorageAccount storageAccount = CloudStorageAccount.Parse("DefaultEndpointsProtocol=https;AccountName=foodraterapibbd2;AccountKey=6KUW7ksyYI2L9DfPQzdjo+dfF8uT53eZIdVOuJsjrxNA9vraGtDdv2n+1wmfZDBQSk0ePod+mnWk1ZfxHEtxsw==;BlobEndpoint=https://foodraterapibbd2.blob.core.windows.net/;QueueEndpoint=https://foodraterapibbd2.queue.core.windows.net/;TableEndpoint=https://foodraterapibbd2.table.core.windows.net/;FileEndpoint=https://foodraterapibbd2.file.core.windows.net/;");
            CloudTableClient tableClient = storageAccount.CreateCloudTableClient();
            CloudTable table = tableClient.GetTableReference("restaurants");

            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            var restaurantToUpdate = JsonConvert.DeserializeObject<RestaurantTableEntity>(requestBody);

            var existingResult = (RestaurantTableEntity)(await table.ExecuteAsync(TableOperation.Retrieve<RestaurantTableEntity>(restaurantToUpdate.PartitionKey, restaurantToUpdate.RowKey))).Result;

            if (existingResult != null)
            {
                var mergeOperation = TableOperation.InsertOrMerge(restaurantToUpdate);
                await table.ExecuteAsync(mergeOperation);
            }
            else
            {
                return new NotFoundObjectResult($"Restaurant not found, Name: {restaurantToUpdate.Name}");
            }

            return new NoContentResult();
        }

        [FunctionName("AddRestaurantRating")]
        public static async Task<IActionResult> AddRestaurantRating(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = null)]
            HttpRequest req,
            ILogger log)
        {
            CloudStorageAccount storageAccount = CloudStorageAccount.Parse("DefaultEndpointsProtocol=https;AccountName=foodraterapibbd2;AccountKey=6KUW7ksyYI2L9DfPQzdjo+dfF8uT53eZIdVOuJsjrxNA9vraGtDdv2n+1wmfZDBQSk0ePod+mnWk1ZfxHEtxsw==;BlobEndpoint=https://foodraterapibbd2.blob.core.windows.net/;QueueEndpoint=https://foodraterapibbd2.queue.core.windows.net/;TableEndpoint=https://foodraterapibbd2.table.core.windows.net/;FileEndpoint=https://foodraterapibbd2.file.core.windows.net/;");
            CloudTableClient tableClient = storageAccount.CreateCloudTableClient();
            CloudTable tableRestaurants = tableClient.GetTableReference("restaurants");
            CloudTable tableRestaurantsRatings = tableClient.GetTableReference("restaurantsRatings");

            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            var restaurantToUpdate = JsonConvert.DeserializeObject<RestaurantTableEntity>(requestBody);

            var existingResult = (RestaurantTableEntity)(await tableRestaurants.ExecuteAsync(TableOperation.Retrieve<RestaurantTableEntity>(restaurantToUpdate.PartitionKey, restaurantToUpdate.RowKey))).Result;

            if (existingResult != null)
            {

                var rating = new RestaurantRatingTableEntity().ToNewRestaurantRatingTableEntity();
                rating.RestaurantRowKey = restaurantToUpdate.RowKey;
                rating.Rating = restaurantToUpdate.Rating;
                var insertOperation = TableOperation.Insert(rating);

                await tableRestaurantsRatings.ExecuteAsync(insertOperation);

                TableContinuationToken token = null;
                var restaurantRatingTableEntities = new List<RestaurantRatingTableEntity>();
                do
                {
                    var queryResult = await tableRestaurantsRatings.ExecuteQuerySegmentedAsync(new TableQuery<RestaurantRatingTableEntity>(), token);
                    restaurantRatingTableEntities.AddRange(queryResult.Results);
                    token = queryResult.ContinuationToken;
                } while (token != null);

                restaurantToUpdate.Rating = CalculateAverage(restaurantRatingTableEntities.Where(x => x.RestaurantRowKey == restaurantToUpdate.RowKey).Select(x => x.Rating));

                var mergeOperation = TableOperation.InsertOrMerge(restaurantToUpdate);
                await tableRestaurants.ExecuteAsync(mergeOperation);
            }
            else
            {
                return new NotFoundObjectResult($"Restaurant not found, Name: {restaurantToUpdate.Name}");
            }

            return new NoContentResult();
        }

        private static string CalculateAverage(IEnumerable<string> ratings)
        {
            return ratings.Select(x => float.Parse(x)).Average().ToString();
        }

        [FunctionName("DeleteRestaurant")]
        public static async Task<IActionResult> DeleteRestaurant(
            [HttpTrigger(AuthorizationLevel.Function, "delete", Route = null)]
            HttpRequest req,
            ILogger log)
        {
            CloudStorageAccount storageAccount = CloudStorageAccount.Parse("DefaultEndpointsProtocol=https;AccountName=foodraterapibbd2;AccountKey=6KUW7ksyYI2L9DfPQzdjo+dfF8uT53eZIdVOuJsjrxNA9vraGtDdv2n+1wmfZDBQSk0ePod+mnWk1ZfxHEtxsw==;BlobEndpoint=https://foodraterapibbd2.blob.core.windows.net/;QueueEndpoint=https://foodraterapibbd2.queue.core.windows.net/;TableEndpoint=https://foodraterapibbd2.table.core.windows.net/;FileEndpoint=https://foodraterapibbd2.file.core.windows.net/;");
            CloudTableClient tableClient = storageAccount.CreateCloudTableClient();
            CloudTable table = tableClient.GetTableReference("restaurants");

            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            var restaurant = JsonConvert.DeserializeObject<RestaurantTableEntity>(requestBody);

            var existingResult = (RestaurantTableEntity)(await table.ExecuteAsync(TableOperation.Retrieve<RestaurantTableEntity>(restaurant.PartitionKey, restaurant.RowKey))).Result;

            if (existingResult != null)
            {
                var deleteOperation = TableOperation.Delete(existingResult);
                await table.ExecuteAsync(deleteOperation);
            }
            else
            {
                return new NotFoundObjectResult($"Restaurant not found, Name: {restaurant.Name}");
            }

            return new OkResult();
        }

    }
}
