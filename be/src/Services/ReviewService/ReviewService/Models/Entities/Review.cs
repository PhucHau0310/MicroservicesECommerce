using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReviewService.Models.Entities
{
    public class Review
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? Id { get; set; }

        [BsonGuidRepresentation(GuidRepresentation.Standard)]
        public required Guid ProductId { get; set; }

        [BsonGuidRepresentation(GuidRepresentation.Standard)]
        public required Guid UserId { get; set; }
        public required string Comment { get; set; }
        public int Rating { get; set; }  // Values from 1-5
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdateAt {  get; set; }
    }
}
