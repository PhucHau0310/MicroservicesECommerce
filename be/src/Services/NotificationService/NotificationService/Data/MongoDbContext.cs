using Microsoft.Extensions.Configuration;
using MongoDB.Driver;
using NotificationService.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NotificationService.Data
{
    public class MongoDbContext
    {
        private readonly IConfiguration _configuration;
        private readonly IMongoDatabase _database;

        public MongoDbContext(IConfiguration configuration)
        {
            _configuration = configuration;

            var connectStrings = _configuration.GetConnectionString("DbConnection");
            var mongourl = MongoUrl.Create(connectStrings);
            var mongoClient = new MongoClient(mongourl);
            _database = mongoClient.GetDatabase(mongourl.DatabaseName);
        }

        public IMongoCollection<Notification> Notifications => _database.GetCollection<Notification>("Notifications");
    }
}
