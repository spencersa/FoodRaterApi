using FoodRaterApi.Data;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Text;

[assembly: FunctionsStartup(typeof(FoodRaterApi.Startup))]
namespace FoodRaterApi
{
    public class Startup : FunctionsStartup
    {
        public override void Configure(IFunctionsHostBuilder builder)
        {
            builder.Services.AddSingleton((s) =>
            {
                return new CloudTableRepository(Environment.GetEnvironmentVariable("CLOUDTABLE_CONNECTIONSTRING"));
            });
        }
    }
}
