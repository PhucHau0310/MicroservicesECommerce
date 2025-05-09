using ReviewService.Models.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReviewService.Repositories.Interfaces
{
    public interface IReviewRepository
    {
        Task<List<Review>> GetReviewsByProductIdAsync(Guid productId);
        Task<Review?> GetReviewByIdAsync(string id);
        Task<Review> CreateReviewAsync(Review review);
        Task<bool> DeleteReviewAsync(string id);
    }
}
