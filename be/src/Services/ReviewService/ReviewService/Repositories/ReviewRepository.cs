using MongoDB.Driver;
using ReviewService.Data;
using ReviewService.Models.Entities;
using ReviewService.Repositories.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReviewService.Repositories
{
    public class ReviewRepository : IReviewRepository
    {
        private readonly IMongoCollection<Review> _reviews;

        public ReviewRepository(ReviewDbContext dbContext)
        {
            _reviews = dbContext.Reviews;
        }

        public async Task<Review> CreateReviewAsync(Review review)
        {
            await _reviews.InsertOneAsync(review);
            return review;
        }

        public async Task<bool> DeleteReviewAsync(string id)
        {
            if (id == null) throw new ArgumentNullException("id");

            var result = await _reviews.DeleteOneAsync(o => o.Id == id);
            return result.DeletedCount > 0;
        }

        public async Task<Review?> GetReviewByIdAsync(string id)
        {
            if (id == string.Empty)
            {
                throw new ArgumentException("Invalid review id", nameof(id));
            }

            return await _reviews.Find(review => review.Id == id).FirstOrDefaultAsync();
        }

        public async Task<List<Review>> GetReviewsByProductIdAsync(Guid productId)
        {
            if (productId == Guid.Empty)
            {
                throw new ArgumentException("Invalid product id", nameof(productId));
            }

            return await _reviews.Find(review => review.ProductId == productId).ToListAsync();
        }
    }
}
