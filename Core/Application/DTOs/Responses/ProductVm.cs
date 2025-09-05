using Domain.Entities.Base;

namespace Application.DTOs.Responses;

public record ProductVm(
    Guid Id,
    Translation Name,
    Translation? Description,
    decimal Price,
    int Quantity,
    CategoryVm Category,
    DateTime CreatedAt,
    DateTime? UpdatedAt
);