using AutoFixture;
using FoodRaterApi.Models;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FoodRaterApi.Data
{
    public class CloudTableRepository
    {
        private CloudTableClient _tableClient;

        public CloudTableRepository() { }

        public CloudTableRepository(string connectionString)
        {
            _tableClient = CloudStorageAccount.Parse(connectionString).CreateCloudTableClient();
        }

        public virtual async Task<IEnumerable<RestaurantTableEntity>> GetAllRestaurantsAsync()
        {
            CloudTable table = _tableClient.GetTableReference("restaurants");

            TableContinuationToken token = null;
            var entities = new List<RestaurantTableEntity>();
            do
            {
                var queryResult = await table.ExecuteQuerySegmentedAsync(new TableQuery<RestaurantTableEntity>(), token);
                entities.AddRange(queryResult.Results);
                token = queryResult.ContinuationToken;
            } while (token != null);

            return entities;
        }

        public virtual async Task<RestaurantTableEntity> GetRestaurantAsync(string rowKey)
        {
            CloudTable table = _tableClient.GetTableReference("restaurants");

            //TODO: Stop hard coding partition
            var restaurant = (RestaurantTableEntity)(await table.ExecuteAsync(TableOperation.Retrieve<RestaurantTableEntity>("Partition1", rowKey))).Result;
            return restaurant;
        }

        public virtual async Task<RestaurantTableEntity> InsertRestaurantAsync(RestaurantTableEntity restaurant)
        {
            CloudTable table = _tableClient.GetTableReference("restaurants");

            var insertOperation = TableOperation.Insert(restaurant);
            await table.ExecuteAsync(insertOperation);

            return restaurant;
        }

        public virtual async Task UpdateRestaurantAsync(RestaurantTableEntity restaurantTableEntity)
        {
            CloudTable table = _tableClient.GetTableReference("restaurants");

            var mergeOperation = TableOperation.InsertOrMerge(restaurantTableEntity);
            await table.ExecuteAsync(mergeOperation);
        }

        public virtual async Task AddRestaurantRatingAsync(RestaurantRatingTableEntity restaurantRatingTableEntity, RestaurantTableEntity restaurantTableEntity)
        {
            CloudTable tableRestaurantsRatings = _tableClient.GetTableReference("restaurantsRatings");

            var insertOperation = TableOperation.Insert(restaurantRatingTableEntity);
            await tableRestaurantsRatings.ExecuteAsync(insertOperation);

            TableContinuationToken token = null;
            var restaurantRatingTableEntities = new List<RestaurantRatingTableEntity>();
            do
            {
                var queryResult = await tableRestaurantsRatings.ExecuteQuerySegmentedAsync(new TableQuery<RestaurantRatingTableEntity>(), token);
                restaurantRatingTableEntities.AddRange(queryResult.Results);
                token = queryResult.ContinuationToken;
            } while (token != null);

            restaurantTableEntity.Rating = CalculateAverage(restaurantRatingTableEntities.Where(x => x.RestaurantRowKey == restaurantTableEntity.RowKey).Select(x => x.Rating));

            await UpdateRestaurantAsync(restaurantTableEntity);
        }

        public virtual async Task DeleteRestaurantAsync(RestaurantTableEntity restaurantTableEntity)
        {
            CloudTable tableRestaurants = _tableClient.GetTableReference("restaurants");

            var deleteOperation = TableOperation.Delete(restaurantTableEntity);
            await tableRestaurants.ExecuteAsync(deleteOperation);
        }

        public virtual async Task SeedTableAsync()
        {
            CloudTable tableRestaurants = _tableClient.GetTableReference("restaurants");
            CloudTable tableRestaurantRatings = _tableClient.GetTableReference("restaurantsRatings");

            await tableRestaurants.CreateIfNotExistsAsync();
            await tableRestaurantRatings.CreateIfNotExistsAsync();

            var fixture = new Fixture();
            var restaurants = fixture
                .Build<Restaurant>()
                .With(x => x.Rating, 0)
                .CreateMany()
                .Select(Mapper.ToNewRestaurantTableEntity);

            TableBatchOperation batchOperation = new TableBatchOperation();
            foreach (var restaurant in restaurants)
            {
                batchOperation.Insert(restaurant);
            }

            await tableRestaurants.ExecuteBatchAsync(batchOperation);
        }

        private static string CalculateAverage(IEnumerable<string> ratings)
        {
            return ratings.Select(x => float.Parse(x)).Average().ToString();
        }

    }
}
