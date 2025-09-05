using Domain.Entities.Base;
using Domain.Interfaces;

namespace Domain.Entities;

public class Category : BaseEntity, IAuditableEntity
{
    private Category()
    {
    }

    public Category(Translation name, Translation? description = null)
    {
        Id = Guid.NewGuid();
        Name = name;
        Description = description;
    }

    public Translation Name { get; set; }
    public Translation? Description { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public ICollection<Product> Products { get; set; } = new List<Product>();

    public void UpdateCategory(Translation name, Translation? description = null)
    {
        Name = name;
        Description = description;
    }
}