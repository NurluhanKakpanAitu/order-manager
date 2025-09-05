using Domain.Entities.Base;
using Domain.Enums;
using Domain.Interfaces;

namespace Domain.Entities;

public class Order : BaseEntity, IAuditableEntity
{
    private Order()
    {
    }

    public Order(IEnumerable<OrderItem> items)
    {
        Id = Guid.NewGuid();
        Status = OrderStatus.New;

        foreach (var item in items) AddItem(item);

        CalculateTotal();
    }

    public decimal TotalAmount { get; private set; }
    public OrderStatus Status { get; private set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public ICollection<OrderItem> Items { get; } = new List<OrderItem>();

    public void AddItem(OrderItem item)
    {
        if (Status != OrderStatus.New)
            throw new InvalidOperationException("Cannot modify order that is not in New status");

        Items.Add(item);
        CalculateTotal();
    }

    public void RemoveItem(Guid productId)
    {
        if (Status != OrderStatus.New)
            throw new InvalidOperationException("Cannot modify order that is not in New status");

        var item = Items.FirstOrDefault(x => x.ProductId == productId);
        if (item != null)
        {
            Items.Remove(item);
            CalculateTotal();
        }
    }

    public void Pay()
    {
        if (Status != OrderStatus.New)
            throw new InvalidOperationException("Only new orders can be paid");

        Status = OrderStatus.Paid;
    }

    public void Cancel()
    {
        if (Status != OrderStatus.New)
            throw new InvalidOperationException("Only new orders can be cancelled");

        Status = OrderStatus.Cancelled;
    }

    private void CalculateTotal()
    {
        TotalAmount = Items.Sum(x => x.Price * x.Quantity);
    }
}