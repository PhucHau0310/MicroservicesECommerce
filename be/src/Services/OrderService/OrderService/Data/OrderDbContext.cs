//using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using MongoDB.Driver;
using OrderService.Models.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OrderService.Data
{
    public class OrderDbContext
    {
        private readonly IConfiguration _configuration;
        private readonly IMongoDatabase _database;

        public OrderDbContext(IConfiguration configuration)
        {
            _configuration = configuration;

            var connectStrings = _configuration.GetConnectionString("DbConnection");
            var mongourl = MongoUrl.Create(connectStrings);
            var mongoClient = new MongoClient(mongourl);
            _database = mongoClient.GetDatabase(mongourl.DatabaseName);
        }

        public IMongoCollection<Order> Orders => _database.GetCollection<Order>("Orders");

        public IMongoCollection<OrderItem> OrderItems => _database.GetCollection<OrderItem>("OrderItems");
    }
}
