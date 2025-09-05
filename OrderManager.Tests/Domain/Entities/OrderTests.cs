using Domain.Entities;
using Domain.Enums;
using FluentAssertions;

namespace OrderManager.Tests.Domain.Entities;

public class OrderTests
{
    [Fact]
    public void Order_Constructor_ShouldCreateOrderWithNewStatus()
    {
        // Arrange
        var items = new List<OrderItem>
        {
            new(Guid.NewGuid(), 2, 100m),
            new(Guid.NewGuid(), 1, 50m)
        };
        
        // Act
        var order = new Order(items);
        
        // Assert
        order.Id.Should().NotBeEmpty();
        order.Status.Should().Be(OrderStatus.New);
        order.TotalAmount.Should().Be(250m);
        order.Items.Should().HaveCount(2);
    }
    
    [Fact]
    public void AddItem_WhenOrderStatusIsNew_ShouldAddItemAndRecalculateTotal()
    {
        // Arrange
        var order = CreateTestOrder();
        var newItem = new OrderItem(Guid.NewGuid(), 1, 30m);
        var initialTotal = order.TotalAmount;
        
        // Act
        order.AddItem(newItem);
        
        // Assert
        order.Items.Should().Contain(newItem);
        order.TotalAmount.Should().Be(initialTotal + 30m);
    }
    
    [Fact]
    public void AddItem_WhenOrderStatusIsNotNew_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var order = CreateTestOrder();
        order.Pay();
        var newItem = new OrderItem(Guid.NewGuid(), 1, 30m);
        
        // Act & Assert
        var action = () => order.AddItem(newItem);
        action.Should().Throw<InvalidOperationException>()
            .WithMessage("Cannot modify order that is not in New status");
    }
    
    [Fact]
    public void Pay_WhenOrderStatusIsNew_ShouldChangeStatusToPaid()
    {
        // Arrange
        var order = CreateTestOrder();
        
        // Act
        order.Pay();
        
        // Assert
        order.Status.Should().Be(OrderStatus.Paid);
    }
    
    [Fact]
    public void Pay_WhenOrderStatusIsNotNew_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var order = CreateTestOrder();
        order.Cancel();
        
        // Act & Assert
        var action = () => order.Pay();
        action.Should().Throw<InvalidOperationException>()
            .WithMessage("Only new orders can be paid");
    }
    
    [Fact]
    public void Cancel_WhenOrderStatusIsNew_ShouldChangeStatusToCancelled()
    {
        // Arrange
        var order = CreateTestOrder();
        
        // Act
        order.Cancel();
        
        // Assert
        order.Status.Should().Be(OrderStatus.Cancelled);
    }
    
    [Fact]
    public void Cancel_WhenOrderStatusIsNotNew_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var order = CreateTestOrder();
        order.Pay();
        
        // Act & Assert
        var action = () => order.Cancel();
        action.Should().Throw<InvalidOperationException>()
            .WithMessage("Only new orders can be cancelled");
    }
    
    private static Order CreateTestOrder()
    {
        var items = new List<OrderItem>
        {
            new(Guid.NewGuid(), 2, 100m)
        };
        return new Order(items);
    }
}