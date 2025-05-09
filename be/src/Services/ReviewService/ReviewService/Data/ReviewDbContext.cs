using Microsoft.Extensions.Configuration;
using MongoDB.Driver;
using ReviewService.Models.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReviewService.Data
{
    public class ReviewDbContext
    {
        private readonly IConfiguration _configuration;
        private readonly IMongoDatabase _database;

        public ReviewDbContext(IConfiguration configuration)
        {
            _configuration = configuration;

            var connectStrings = _configuration.GetConnectionString("DbConnection");
            var mongourl = MongoUrl.Create(connectStrings);
            var mongoClient = new MongoClient(mongourl);
            _database = mongoClient.GetDatabase(mongourl.DatabaseName);
        }

        public IMongoCollection<Review> Reviews => _database.GetCollection<Review>("Reviews");
    }
}
