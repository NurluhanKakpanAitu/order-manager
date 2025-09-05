using Domain.Entities.Base;

namespace Application.DTOs.Requests;

public record CreateProductRequest(
    Translation Name,
    Translation? Description,
    decimal Price,
    int Quantity,
    Guid CategoryId
);