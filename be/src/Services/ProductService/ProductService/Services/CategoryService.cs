using Microsoft.Extensions.Logging;
using ProductService.Services.Interfaces;
using ProductService.Models.Entities;
using ProductService.Repositories.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProductService.Services
{
    public class CategoryService : ICategoryService
    {
        private readonly ICategoryRepository _categoryRepository;
        private readonly ILogger<CategoryService> _logger;

        public CategoryService(ICategoryRepository categoryRepository, ILogger<CategoryService> logger)
        {
            _categoryRepository = categoryRepository;
            _logger = logger;
        }

        public async Task<bool> AddCategoryAsync(Category category)
        {
            if (category == null)
            {
                _logger.LogError("Category is null");
                return false;
            }

            await _categoryRepository.AddCategoryAsync(category);
            return true;
        }

        public async Task<bool> DeleteCategoryAsync(Guid id)
        {
            if (id == Guid.Empty)
            {
                _logger.LogError("Category id is empty");
                return false;
            }

            await _categoryRepository.DeleteCategoryAsync(id);
            return true;
        }

        public async Task<IEnumerable<Category>> GetAllCategoriesAsync()
        {
            return await _categoryRepository.GetAllCategoriesAsync();
        }

        public async Task<Category> GetCategoryByIdAsync(Guid id)
        {
           if (id == Guid.Empty)
            {
                _logger.LogError("Category id is empty");
                return null;
            }
            return await _categoryRepository.GetCategoryByIdAsync(id);
        }

        public async Task<bool> UpdateCategoryAsync(Category category)
        {
            if (category == null)
            {
                _logger.LogError("Category is null");
                return false;
            }

            await _categoryRepository.UpdateCategoryAsync(category);
            return true;
        }
    }
}
