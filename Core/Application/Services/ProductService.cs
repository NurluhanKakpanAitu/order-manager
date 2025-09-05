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

public class ProductService(
    IGenericRepository<Product> productRepository,
    IGenericRepository<Category> categoryRepository,
    IUnitOfWork unitOfWork,
    ILogger<ProductService> logger,
    IMapper mapper,
    ILocalizationService localizationService)
    : IProductService
{
    public async Task<ProductVm> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var product = await productRepository.GetByIdMappedAsync<ProductVm>(id, cancellationToken);
        if (product == null)
        {
            logger.LogWarning("Product with ID: {ProductId} not found", id);
            throw new ArgumentException("Product not found");
        }
        
        return product;
    }

    public async Task<PaginatedResult<ProductVm>> GetAllAsync(int pageNumber = 1, int pageSize = 10, string? searchTerm = null, Guid? categoryId = null, decimal? minPrice = null, decimal? maxPrice = null, CancellationToken cancellationToken = default)
    {
        if (HasFilters(searchTerm, categoryId, minPrice, maxPrice))
        {
            var languageCode = localizationService.GetCurrentLanguageCode();
            var predicate = ProductSearchPredicate(searchTerm, languageCode, categoryId, minPrice, maxPrice);
            return await productRepository.GetPaginatedAsync<ProductVm>(pageNumber, pageSize, predicate, cancellationToken);
        }
        
        return await productRepository.GetPaginatedAsync<ProductVm>(pageNumber, pageSize, cancellationToken);
    }

    public async Task CreateAsync(CreateProductRequest createProductRequest, CancellationToken cancellationToken = default)
    {
        logger.LogInformation("Creating product with name: {ProductName}", createProductRequest.Name.En);
        
        var categoryExists = await categoryRepository.ExistsAsync(createProductRequest.CategoryId, cancellationToken);
        if (!categoryExists)
        {
            logger.LogWarning("Product creation failed: Category {CategoryId} not found", createProductRequest.CategoryId);
            throw new ArgumentException("Category not found");
        }
        
        var product = mapper.Map<Product>(createProductRequest);

        await productRepository.AddAsync(product, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        
        logger.LogInformation("Product created successfully with ID: {ProductId}", product.Id);
    }

    public async Task UpdateAsync(Guid id, UpdateProductRequest updatedProductRequest, CancellationToken cancellationToken = default)
    {
        var product = await productRepository.GetByIdAsync(id, cancellationToken);
        if (product == null)
            throw new ArgumentException("Product not found");

        product.UpdateProduct(updatedProductRequest.Name, updatedProductRequest.Price, updatedProductRequest.Quantity, updatedProductRequest.Description);
        await productRepository.UpdateAsync(product, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        logger.LogInformation("Deleting product with ID: {ProductId}", id);
        
        var exists = await productRepository.ExistsAsync(id, cancellationToken);
        if (!exists)
        {
            logger.LogWarning("Product deletion failed: Product {ProductId} not found", id);
            throw new ArgumentException("Product not found");
        }

        await productRepository.DeleteAsync(id, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        
        logger.LogInformation("Product deleted successfully: {ProductId}", id);
    }
    
    private static bool HasFilters(string? searchTerm, Guid? categoryId, decimal? minPrice, decimal? maxPrice)
    {
        return !string.IsNullOrEmpty(searchTerm) || categoryId.HasValue || minPrice.HasValue || maxPrice.HasValue;
    }
    
    private static System.Linq.Expressions.Expression<Func<Product, bool>> ProductSearchPredicate(string? searchTerm, string? languageCode, Guid? categoryId, decimal? minPrice, decimal? maxPrice)
    {
        return p => 
            (string.IsNullOrEmpty(searchTerm) || string.IsNullOrEmpty(languageCode) ||
             (languageCode.ToLower() == "kz" && p.Name.Kz.Contains(searchTerm)) ||
             (languageCode.ToLower() == "en" && p.Name.En.Contains(searchTerm)) ||
             (languageCode.ToLower() != "kz" && languageCode.ToLower() != "en" && p.Name.Ru.Contains(searchTerm))) &&
            (!categoryId.HasValue || p.CategoryId == categoryId.Value) &&
            (!minPrice.HasValue || p.Price >= minPrice.Value) &&
            (!maxPrice.HasValue || p.Price <= maxPrice.Value);
    }
}