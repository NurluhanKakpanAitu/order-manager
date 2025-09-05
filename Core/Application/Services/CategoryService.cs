using System.Linq.Expressions;
using Application.DTOs.Common;
using Application.DTOs.Requests;
using Application.DTOs.Responses;
using Application.Interfaces;
using Application.Interfaces.Repositories;
using AutoMapper;
using Domain.Entities;
using Domain.Interfaces;
using Microsoft.Extensions.Logging;

namespace Application.Services;

public class CategoryService(
    IUnitOfWork unitOfWork,
    IGenericRepository<Category> categoryRepository,
    IGenericRepository<Product> productRepository,
    IMapper mapper,
    ILocalizationService localizationService,
    ILogger<CategoryService> logger)
    : ICategoryService
{
    public async Task<CategoryVm> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var category = await categoryRepository.GetByIdMappedAsync<CategoryVm>(id, cancellationToken);
        if (category == null)
            throw new ArgumentException("Category not found");
        
        return category;
    }

    public async Task<PaginatedResult<CategoryVm>> GetPagedAsync(int pageNumber = 1, int pageSize = 10, string? searchTerm = null, CancellationToken cancellationToken = default)
    {
        if (!string.IsNullOrEmpty(searchTerm))
        {
            var languageCode = localizationService.GetCurrentLanguageCode();
            var predicate = CategorySearchPredicate(searchTerm, languageCode);
            return await categoryRepository.GetPaginatedAsync<CategoryVm>(pageNumber, pageSize, predicate, cancellationToken);
        }
        
        return await categoryRepository.GetPaginatedAsync<CategoryVm>(pageNumber, pageSize, cancellationToken);
    }

    public async Task CreateAsync(CreateCategoryRequest createCategoryRequest, CancellationToken cancellationToken = default)
    {
        logger.LogInformation("Creating category with name: {CategoryName}", createCategoryRequest.Name.En);
        var category = mapper.Map<Category>(createCategoryRequest);
        await categoryRepository.AddAsync(category, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        logger.LogInformation("Category created successfully with ID: {CategoryId}", category.Id);
    }

    public async Task UpdateAsync(Guid id, UpdateCategoryRequest updateCategoryRequest,
        CancellationToken cancellationToken = default)
    {
        logger.LogInformation("Updating category with ID: {CategoryId}", id);
        var category = await categoryRepository.GetByIdAsync(id, cancellationToken);
        
        if (category == null)
        {
            logger.LogWarning("Category update failed: Category {CategoryId} not found", id);
            throw new ArgumentException("Category not found");
        }

        category.UpdateCategory(updateCategoryRequest.Name, updateCategoryRequest.Description);
        await categoryRepository.UpdateAsync(category, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        logger.LogInformation("Category updated successfully: {CategoryId}", id);
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        logger.LogInformation("Deleting category with ID: {CategoryId}", id);
        
        var hasProducts = await productRepository.AnyAsync(p => p.CategoryId == id, cancellationToken);
        if (hasProducts)
        {
            logger.LogWarning("Category deletion failed: Category {CategoryId} has associated products", id);
            throw new InvalidOperationException("Cannot delete category that has products");
        }

        await categoryRepository.DeleteAsync(id, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        logger.LogInformation("Category deleted successfully: {CategoryId}", id);
    }
    
    private static Expression<Func<Category, bool>> CategorySearchPredicate(string searchTerm, string languageCode)
    {
        return languageCode.ToLower() switch
        {
            "kz" => c => c.Name.Kz.Contains(searchTerm) || (c.Description != null && c.Description.Kz != null && c.Description.Kz.Contains(searchTerm)),
            "en" => c => c.Name.En.Contains(searchTerm) || (c.Description != null && c.Description.En != null && c.Description.En.Contains(searchTerm)),
            _ => c => c.Name.Ru.Contains(searchTerm) || (c.Description != null && c.Description.Ru != null && c.Description.Ru.Contains(searchTerm))
        };
    }
}