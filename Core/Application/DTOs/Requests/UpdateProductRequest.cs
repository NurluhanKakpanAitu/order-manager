using Domain.Entities.Base;

namespace Application.DTOs.Requests;

public record UpdateProductRequest(
    Translation Name,
    Translation? Description,
    decimal Price,
    int Quantity
);