using Application.DTOs.Common;
using Application.DTOs.Requests;
using Application.DTOs.Responses;
using Domain.Enums;

namespace Application.Interfaces;

public interface IOrderService
{
    Task<OrderVm> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<PaginatedResult<OrderVm>> GetPagedAsync(int pageNumber = 1, int pageSize = 10, OrderStatus? status = null, CancellationToken cancellationToken = default);
    Task CreateAsync(CreateOrderRequest createOrderRequest, CancellationToken cancellationToken = default);
    Task PayOrderAsync(Guid id, CancellationToken cancellationToken = default);
    Task CancelOrderAsync(Guid id, CancellationToken cancellationToken = default);
}