
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProductService.Services.Interfaces;
using ProductService.Models.Entities;

namespace ProductService.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class CategoryController : ControllerBase
    {
        private readonly ICategoryService _categoryService;
        private readonly ILogger<CategoryController> _logger;
        public CategoryController(ICategoryService categoryService, ILogger<CategoryController> logger)
        {
            _categoryService = categoryService;
            _logger = logger;
        }

        [AllowAnonymous]
        [HttpGet("all")]
        public async Task<IActionResult> GetAllCategories()
        {
            try
            {
                var categories = await _categoryService.GetAllCategoriesAsync();
                return Ok(categories);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while fetching all categories");
                return StatusCode(500, "Internal server error");
            }
        }

        [AllowAnonymous]
        [HttpGet("detail")]
        public async Task<IActionResult> GetCategoryById(Guid id)
        {
            try
            {
                var category = await _categoryService.GetCategoryByIdAsync(id);
                if (category == null)
                {
                    return NotFound();
                }
                return Ok(category);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while fetching category by id");
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpPost("create")]
        public async Task<IActionResult> AddCategory([FromBody] Category category)
        {
            try
            {
                var result = await _categoryService.AddCategoryAsync(category);
                if (!result)
                {
                    return BadRequest("Failed to add category");
                }
                return Ok("Category added successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while adding category");
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpPut("update")]
        public async Task<IActionResult> UpdateCategory([FromBody] Category category)
        {
            try
            {
                var result = await _categoryService.UpdateCategoryAsync(category);
                if (!result)
                {
                    return BadRequest("Failed to update category");
                }
                return Ok("Category updated successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while updating category");
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpDelete("delete")]
        public async Task<IActionResult> DeleteCategory(Guid id)
        {
            try
            {
                var result = await _categoryService.DeleteCategoryAsync(id);
                if (!result)
                {
                    return BadRequest("Failed to delete category");
                }
                return Ok("Category deleted successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while deleting category");
                return StatusCode(500, "Internal server error");
            }
        }
    }
}
