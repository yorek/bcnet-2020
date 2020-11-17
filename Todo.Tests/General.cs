using System;
using NUnit.Framework;
using DotNetEnv;
using Microsoft.Data.SqlClient;
using Dapper;
using Newtonsoft.Json.Linq;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Console;
using Microsoft.Extensions.Configuration;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using Todo.API.Controllers;

namespace Todo.Tests
{
    public class Tests
    {
        private TodoController _controller;

        [OneTimeSetUp]
        public void Initialize()
        {
            // Access to .env file used for local development
            DotNetEnv.Env.Load(Environment.CurrentDirectory + "/../../../" + Env.DEFAULT_ENVFILENAME);
           
            // Create logger
            var loggerFactory = LoggerFactory.Create(b => b.AddConsole());
            var logger = loggerFactory.CreateLogger<TodoController>();

            // Create In-Memory configuration
            var connectionString = Environment.GetEnvironmentVariable("ConnectionString");
            var inMemoryConfiguration = new Dictionary<string, string>()
            {
                { "ConnectionStrings:AzureSQL", connectionString }                
            };
            var config = new ConfigurationBuilder().AddInMemoryCollection(inMemoryConfiguration).Build();

            // Create controller
            _controller = new TodoController(config, logger);        
        }

        [SetUp]
        public void Setup()
        {
        }

        [Test, Order(100)]
        public async Task Respond_To_GET()
        {
            var result = await _controller.Get(null);

            Assert.IsInstanceOf(typeof(JArray), result);            
        }
    }
}