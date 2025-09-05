using Domain.Enums;

namespace Application.DTOs.Responses;

public record OrderVm(
    Guid Id,
    decimal TotalAmount,
    OrderStatus Status,
    DateTime CreatedAt,
    DateTime? UpdatedAt,
    OrderItemVm[] Items
);

public record OrderItemVm(
    Guid Id,
    ProductVm Product,
    int Quantity,
    decimal Price,
    decimal Total
);