using Microsoft.EntityFrameworkCore;
using ProductService.Data;
using ProductService.Models.Entities;
using ProductService.Repositories.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProductService.Repositories
{
    public class CategoryRepository : ICategoryRepository
    {
        private readonly ProductDbContext _dbContext;

        public CategoryRepository(ProductDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task AddCategoryAsync(Category category)
        {
            if (category == null) throw new ArgumentNullException(nameof(category));

            await _dbContext.Categories.AddAsync(category);
            await _dbContext.SaveChangesAsync();
        }

        public async Task DeleteCategoryAsync(Guid id)
        {
            if (id == Guid.Empty) throw new ArgumentNullException(nameof(id));

            var category = await _dbContext.Categories.Include(p => p.Products)
                                                      .FirstOrDefaultAsync(c => c.Id == id);

            if (category == null) throw new Exception($"Category with id {id} not found");

            _dbContext.Categories.Remove(category);
            await _dbContext.SaveChangesAsync();
        }

        public async Task<IEnumerable<Category>> GetAllCategoriesAsync()
        {
            return await _dbContext.Categories.Include(p => p.Products)
                                              .ToListAsync(); 
        }

        public async Task<Category> GetCategoryByIdAsync(Guid id)
        {
            if (id == Guid.Empty) throw new ArgumentNullException(nameof(id));

            var category = await _dbContext.Categories.Include(p => p.Products)
                                                      .FirstOrDefaultAsync(c => c.Id == id);

            if (category == null) throw new Exception($"Category with id {id} not found");
            return category;
        }

        public async Task UpdateCategoryAsync(Category category)
        {
            if (category == null) throw new ArgumentNullException(nameof(category));

            _dbContext.Categories.Update(category);
            await _dbContext.SaveChangesAsync();
        }
    }
}
