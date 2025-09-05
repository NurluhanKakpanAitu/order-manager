using Domain.Entities.Base;

namespace Domain.Entities;

public class OrderItem : BaseEntity
{
    private OrderItem()
    {
    }

    public OrderItem(Guid productId, int quantity, decimal price)
    {
        Id = Guid.NewGuid();
        ProductId = productId;
        Quantity = quantity;
        Price = price;

        if (quantity <= 0)
            throw new ArgumentException("Quantity must be greater than zero");

        if (price < 0)
            throw new ArgumentException("Price cannot be negative");
    }

    public Guid OrderId { get; set; }
    public Order? Order { get; set; }
    public Guid ProductId { get; set; }
    public Product? Product { get; set; }
    public int Quantity { get; set; }
    public decimal Price { get; set; }

    public decimal GetTotal()
    {
        return Price * Quantity;
    }
}