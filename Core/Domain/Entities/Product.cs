using Domain.Entities.Base;
using Domain.Interfaces;

namespace Domain.Entities;

public class Product : BaseEntity, IAuditableEntity
{
    private Product()
    {
    }

    public Product(Translation name, decimal price, int quantity, Guid categoryId, Translation? description = null)
    {
        Id = Guid.NewGuid();
        Name = name;
        Price = price;
        Quantity = quantity;
        CategoryId = categoryId;
        Description = description;
    }

    public Translation Name { get; set; }
    public Translation? Description { get; set; }
    public decimal Price { get; set; }
    public int Quantity { get; set; }
    public Guid CategoryId { get; set; }
    public Category? Category { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    public void UpdateProduct(Translation name, decimal price, int quantity, Translation? description = null)
    {
        Name = name;
        Price = price;
        Quantity = quantity;
        Description = description;
    }

    public void UpdateQuantity(int quantity)
    {
        if (quantity < 0)
            throw new ArgumentException("Quantity cannot be negative");

        Quantity = quantity;
    }

    public bool IsAvailable()
    {
        return Quantity > 0;
    }
}