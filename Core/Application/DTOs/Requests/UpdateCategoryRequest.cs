using Domain.Entities.Base;

namespace Application.DTOs.Requests;

public record UpdateCategoryRequest(
    Translation Name,
    Translation? Description = null
);