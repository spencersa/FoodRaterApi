using FoodRaterApi.Data;
using Moq;
using System;
using Xunit;
using FoodRaterApi;
using AutoFixture;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using FluentAssertions;

namespace FoodRaterApiTests
{
    public class RestaurantFunctionsTests
    {
        private Fixture _fixture;
        private Mock<CloudTableRepository> _mockCloudTableRepository;

        public RestaurantFunctionsTests()
        {
            _fixture = new Fixture();

            _mockCloudTableRepository = new Mock<CloudTableRepository>();
        }

        [Fact]
        public async Task GetAllRestaurants_ShouldGetGetAllRestaurants()
        {
            var expectedRestaurants = _fixture.CreateMany<RestaurantTableEntity>();

            _mockCloudTableRepository.Setup(x => x.GetAllRestaurantsAsync()).ReturnsAsync(expectedRestaurants);

            var restaurantFunctions = new RestaurantFunctions(_mockCloudTableRepository.Object);

            var result = await restaurantFunctions.GetAllRestaurants(null) as OkObjectResult;

            Assert.Equal(200, result.StatusCode);
            result.Value.Should().BeEquivalentTo(expectedRestaurants);
        }
    }
}
