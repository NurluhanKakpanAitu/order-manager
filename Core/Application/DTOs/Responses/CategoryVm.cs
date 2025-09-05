using Domain.Entities.Base;

namespace Application.DTOs.Responses;

public record CategoryVm(
    Guid Id,
    Translation Name,
    Translation? Description
);