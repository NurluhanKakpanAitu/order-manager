using Application.DTOs.Common;
using Application.DTOs.Requests;
using Application.DTOs.Responses;

namespace Application.Interfaces;

public interface IProductService
{
    Task<ProductVm> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<PaginatedResult<ProductVm>> GetAllAsync(int pageNumber = 1, int pageSize = 10, string? searchTerm = null, Guid? categoryId = null, decimal? minPrice = null, decimal? maxPrice = null, CancellationToken cancellationToken = default);
    Task CreateAsync(CreateProductRequest createProductRequest, CancellationToken cancellationToken = default);
    Task UpdateAsync(Guid id, UpdateProductRequest updatedProductRequest, CancellationToken cancellationToken = default);
    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}