using ReviewService.Models.Entities;
using ReviewService.Repositories.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReviewService.Services
{
    public class ReviewService
    {
        private readonly IReviewRepository _reviewRepository;

        public ReviewService(IReviewRepository reviewRepository)
        {
            _reviewRepository = reviewRepository;
        }

        public async Task<Review> CreateReviewAsync(Review review)
        {
            return await _reviewRepository.CreateReviewAsync(review);
        }

        public async Task<bool> DeleteReviewAsync(string id)
        {
            return await _reviewRepository.DeleteReviewAsync(id);
        }

        public async Task<Review?> GetReviewByIdAsync(string id)
        {
            return await _reviewRepository.GetReviewByIdAsync(id);
        }

        public async Task<List<Review>> GetReviewsByProductIdAsync(Guid productId)
        {
            return await _reviewRepository.GetReviewsByProductIdAsync(productId);
        }
    }
}
