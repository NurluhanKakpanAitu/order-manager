using Application.DTOs.Requests;
using Application.Interfaces.Repositories;
using Application.Services;
using Domain.Entities;
using Domain.Entities.Base;
using Domain.Enums;
using Domain.Interfaces;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;

namespace OrderManager.Tests.Application.Services;

public class OrderServiceTests
{
    private readonly Mock<IGenericRepository<Order>> _orderRepositoryMock;
    private readonly Mock<IGenericRepository<Product>> _productRepositoryMock;
    private readonly Mock<IPaymentService> _paymentServiceMock;
    private readonly OrderService _orderService;
    private readonly IUnitOfWork _unitOfWork;
    
    public OrderServiceTests()
    {
        _orderRepositoryMock = new Mock<IGenericRepository<Order>>();
        _productRepositoryMock = new Mock<IGenericRepository<Product>>();
        _paymentServiceMock = new Mock<IPaymentService>();
        var unitOfWorkMock = new Mock<IUnitOfWork>();
        unitOfWorkMock.Setup(u => u.BeginTransactionAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        unitOfWorkMock.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);
        _unitOfWork = unitOfWorkMock.Object;
        var loggerMock = new Mock<ILogger<OrderService>>();
        _orderService = new OrderService(_orderRepositoryMock.Object, _productRepositoryMock.Object, _paymentServiceMock.Object, _unitOfWork, loggerMock.Object);
    }
    
    [Fact]
    public async Task CreateAsync_WithValidItems_ShouldCreateOrderAndReduceProductQuantity()
    {
        // Arrange
        var productId1 = Guid.NewGuid();
        var productId2 = Guid.NewGuid();
        var product1 = CreateTestProduct(productId1, 50m, 10);
        var product2 = CreateTestProduct(productId2, 30m, 5);
        
        var createOrderRequest = new CreateOrderRequest(new[]
        {
            new CreateOrderItemRequest(productId1, 2),
            new CreateOrderItemRequest(productId2, 1)
        });
        
        _productRepositoryMock.Setup(x => x.GetByIdAsync(productId1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(product1);
        _productRepositoryMock.Setup(x => x.GetByIdAsync(productId2, It.IsAny<CancellationToken>()))
            .ReturnsAsync(product2);
        _orderRepositoryMock.Setup(x => x.AddAsync(It.IsAny<Order>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Order order, CancellationToken _) => order);
        
        // Act
        await _orderService.CreateAsync(createOrderRequest);
        
        // Assert - verify order was created
        _orderRepositoryMock.Verify(x => x.AddAsync(It.IsAny<Order>(), It.IsAny<CancellationToken>()), Times.Once);
        
        // Verify product quantities were reduced
        _productRepositoryMock.Verify(x => x.UpdateAsync(It.Is<Product>(p => p.Id == productId1 && p.Quantity == 8), It.IsAny<CancellationToken>()), Times.Once);
        _productRepositoryMock.Verify(x => x.UpdateAsync(It.Is<Product>(p => p.Id == productId2 && p.Quantity == 4), It.IsAny<CancellationToken>()), Times.Once);
    }
    
    [Fact]
    public async Task CreateAsync_WithInsufficientQuantity_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var productId = Guid.NewGuid();
        var product = CreateTestProduct(productId, 50m, 2); // Only 2 in stock
        
        var createOrderRequest = new CreateOrderRequest(new[]
        {
            new CreateOrderItemRequest(productId, 5) // Trying to order 5
        });
        
        _productRepositoryMock.Setup(x => x.GetByIdAsync(productId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(product);
        
        // Act & Assert
        var action = async () => await _orderService.CreateAsync(createOrderRequest);
        await action.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("Insufficient quantity for product Test Product");
    }
    
    [Fact]
    public async Task PayOrderAsync_WithValidOrder_ShouldProcessPaymentAndUpdateStatus()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        var order = CreateTestOrder(orderId);
        
        _orderRepositoryMock.Setup(x => x.GetByIdAsync(orderId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(order);
        _paymentServiceMock.Setup(x => x.ProcessPaymentAsync(orderId, order.TotalAmount, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);
        _productRepositoryMock.Setup(x => x.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(CreateTestProduct());
        
        // Act
        await _orderService.PayOrderAsync(orderId);
        
        // Assert - verify payment was processed and order was updated
        _paymentServiceMock.Verify(x => x.ProcessPaymentAsync(orderId, order.TotalAmount, It.IsAny<CancellationToken>()), Times.Once);
        _orderRepositoryMock.Verify(x => x.UpdateAsync(order, It.IsAny<CancellationToken>()), Times.Once);
    }
    
    [Fact]
    public async Task PayOrderAsync_WithFailedPayment_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        var order = CreateTestOrder(orderId);
        
        _orderRepositoryMock.Setup(x => x.GetByIdAsync(orderId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(order);
        _paymentServiceMock.Setup(x => x.ProcessPaymentAsync(orderId, order.TotalAmount, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);
        
        // Act & Assert
        var action = async () => await _orderService.PayOrderAsync(orderId);
        await action.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("Payment failed");
    }
    
    [Fact]
    public async Task CancelOrderAsync_WithValidOrder_ShouldCancelOrderAndRestoreQuantity()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        var productId = Guid.NewGuid();
        var product = CreateTestProduct(productId, 50m, 8); // Current quantity after order
        var order = CreateTestOrderWithSpecificProduct(orderId, productId, 2, 50m);
        
        _orderRepositoryMock.Setup(x => x.GetByIdAsync(orderId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(order);
        _productRepositoryMock.Setup(x => x.GetByIdAsync(productId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(product);
        
        // Act
        await _orderService.CancelOrderAsync(orderId);
        
        // Assert - verify order was cancelled
        
        // Verify product quantity was restored
        _productRepositoryMock.Verify(x => x.UpdateAsync(It.Is<Product>(p => p.Id == productId && p.Quantity == 10), It.IsAny<CancellationToken>()), Times.Once);
        _orderRepositoryMock.Verify(x => x.UpdateAsync(order, It.IsAny<CancellationToken>()), Times.Once);
    }
    
    private static Product CreateTestProduct(Guid? id = null, decimal price = 100m, int quantity = 10)
    {
        var name = Translation.Create("Тест товар", "Test Product", "Test Product");
        var product = new Product(name, price, quantity, Guid.NewGuid());
        if (id.HasValue)
        {
            typeof(Product).GetProperty("Id")?.SetValue(product, id.Value);
        }
        return product;
    }
    
    private static Order CreateTestOrder(Guid? id = null)
    {
        var items = new List<OrderItem>
        {
            new(Guid.NewGuid(), 2, 100m)
        };
        var order = new Order(items);
        if (id.HasValue)
        {
            typeof(Order).GetProperty("Id")?.SetValue(order, id.Value);
        }
        return order;
    }
    
    private static Order CreateTestOrderWithSpecificProduct(Guid orderId, Guid productId, int quantity, decimal price)
    {
        var items = new List<OrderItem>
        {
            new(productId, quantity, price)
        };
        var order = new Order(items);
        typeof(Order).GetProperty("Id")?.SetValue(order, orderId);
        return order;
    }
}