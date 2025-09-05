using Domain.Entities.Base;

namespace Application.DTOs.Requests;

public class CreateCategoryRequest
{
    public Translation Name { get; set; }
    public Translation? Description { get; set; }
}