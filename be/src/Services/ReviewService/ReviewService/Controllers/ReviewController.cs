using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ReviewService.Models.Entities;

namespace ReviewService.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class ReviewController : ControllerBase
    {
        private readonly ReviewService.Services.ReviewService _reviewService;
        private readonly ILogger<ReviewController> _logger;
        
        public ReviewController(ReviewService.Services.ReviewService reviewService, ILogger<ReviewController> logger)
        {
            _reviewService = reviewService;
            _logger = logger;
        }

        [HttpGet("detail")]
        public async Task<IActionResult> GetReviewsByProduct(Guid productId)
        {
            var reviews = await _reviewService.GetReviewsByProductIdAsync(productId);
            return Ok(reviews);
        }

        [HttpGet("detail/id")]
        public async Task<IActionResult> GetReviewById(string id)
        {
            var review = await _reviewService.GetReviewByIdAsync(id);
            return review != null ? Ok(review) : NotFound();
        }

        [HttpPost("create")]
        public async Task<IActionResult> CreateReview([FromBody] Review reviewDto)
        {
            var review = await _reviewService.CreateReviewAsync(reviewDto);
            return CreatedAtAction(nameof(GetReviewById), new { id = review.Id }, review);
        }

        [HttpDelete("delete")]
        public async Task<IActionResult> DeleteReview(string id)
        {
            var success = await _reviewService.DeleteReviewAsync(id);
            return success ? NoContent() : NotFound();
        }
    }
}
