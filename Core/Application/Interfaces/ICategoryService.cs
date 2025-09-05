using Application.DTOs.Common;
using Application.DTOs.Requests;
using Application.DTOs.Responses;

namespace Application.Interfaces;

public interface ICategoryService
{
    Task<CategoryVm> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<PaginatedResult<CategoryVm>> GetPagedAsync(int pageNumber = 1, int pageSize = 10, string? searchTerm = null, CancellationToken cancellationToken = default);
    Task CreateAsync(CreateCategoryRequest createCategoryRequest, CancellationToken cancellationToken = default);
    Task UpdateAsync(Guid id, UpdateCategoryRequest updateCategoryRequest, CancellationToken cancellationToken = default);
    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}