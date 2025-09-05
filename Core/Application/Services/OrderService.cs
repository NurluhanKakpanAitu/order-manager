using System.Linq.Expressions;
using Application.DTOs.Common;
using Application.DTOs.Requests;
using Application.DTOs.Responses;
using Application.Interfaces;
using Application.Interfaces.Repositories;
using Domain.Entities;
using Domain.Enums;
using Domain.Interfaces;
using Microsoft.Extensions.Logging;

namespace Application.Services;

public class OrderService(
    IGenericRepository<Order> orderRepository,
    IGenericRepository<Product> productRepository,
    IPaymentService paymentService,
    IUnitOfWork unitOfWork,
    ILogger<OrderService> logger)
    : IOrderService
{
    public async Task<OrderVm> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var order = await orderRepository.GetByIdMappedAsync<OrderVm>(id, cancellationToken);
        
        if (order == null)
        {
            logger.LogWarning("Order with ID: {OrderId} not found", id);
            throw new ArgumentException("Order not found");
        }
        
        return order;
    }

    public async Task<PaginatedResult<OrderVm>> GetPagedAsync(int pageNumber = 1, int pageSize = 10, OrderStatus? status = null, CancellationToken cancellationToken = default)
    {
        Expression<Func<Order, bool>> predicate = o => true;
     
        if (status.HasValue) 
            predicate = o => o.Status == status.Value;
        
        var orders = await orderRepository.GetPaginatedAsync<OrderVm>(pageNumber, pageSize, predicate, cancellationToken);

        return orders;
    }

    public async Task<IEnumerable<Order>> GetByStatusAsync(OrderStatus status,
        CancellationToken cancellationToken = default)
    {
        var allOrders = await orderRepository.GetAllAsync(cancellationToken);
        return allOrders.Where(o => o.Status == status);
    }

    public async Task CreateAsync(CreateOrderRequest createOrderRequest, CancellationToken cancellationToken = default)
    {
        logger.LogInformation("Creating order with {ItemsCount} items", createOrderRequest.Items.Count());
        await unitOfWork.BeginTransactionAsync(cancellationToken);
        
        try
        {
            var orderItems = new List<OrderItem>();

            foreach (var item in createOrderRequest.Items)
            {
                var product = await productRepository.GetByIdAsync(item.ProductId, cancellationToken);
                if (product == null)
                    throw new ArgumentException($"Product with id {item.ProductId} not found");

                if (product.Quantity < item.Quantity)
                    throw new InvalidOperationException(
                        $"Insufficient quantity for product {product.Name.En}");
                
                var orderItem = new OrderItem(item.ProductId,item.Quantity, product.Price);

                orderItems.Add(orderItem);

                product.UpdateQuantity(product.Quantity - item.Quantity);
                await productRepository.UpdateAsync(product, cancellationToken);
            }

            var order = new Order(orderItems);
            await orderRepository.AddAsync(order, cancellationToken);
            await unitOfWork.SaveChangesAsync(cancellationToken);
            await unitOfWork.CommitTransactionAsync(cancellationToken);
            logger.LogInformation("Order created successfully with ID: {OrderId}", order.Id);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to create order");
            await unitOfWork.RollbackTransactionAsync(cancellationToken);
            throw;
        }
    }

    public async Task PayOrderAsync(Guid id, CancellationToken cancellationToken = default)
    {
        logger.LogInformation("Processing payment for order: {OrderId}", id);
        await unitOfWork.BeginTransactionAsync(cancellationToken);
        
        try
        {
            var order = await orderRepository.GetByIdAsync(id, cancellationToken);
            if (order == null)
                throw new ArgumentException("Order not found");

            var paymentSuccessful =
                await paymentService.ProcessPaymentAsync(order.Id, order.TotalAmount, cancellationToken);
            if (!paymentSuccessful)
                throw new InvalidOperationException("Payment failed");

            order.Pay();
            await orderRepository.UpdateAsync(order, cancellationToken);
            await unitOfWork.SaveChangesAsync(cancellationToken);
            await unitOfWork.CommitTransactionAsync(cancellationToken);
            logger.LogInformation("Payment processed successfully for order: {OrderId}", id);
            
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to process payment for order: {OrderId}", id);
            await unitOfWork.RollbackTransactionAsync(cancellationToken);
            throw;
        }
    }

    public async Task CancelOrderAsync(Guid id, CancellationToken cancellationToken = default)
    {
        logger.LogInformation("Cancelling order: {OrderId}", id);
        await unitOfWork.BeginTransactionAsync(cancellationToken);
        
        try
        {
            var order = await orderRepository.GetByIdAsync(id, cancellationToken);
            if (order == null)
                throw new ArgumentException("Order not found");

            foreach (var item in order.Items)
            {
                var product = await productRepository.GetByIdAsync(item.ProductId, cancellationToken);
                if (product != null)
                {
                    product.UpdateQuantity(product.Quantity + item.Quantity);
                    await productRepository.UpdateAsync(product, cancellationToken);
                }
            }

            order.Cancel();
            await orderRepository.UpdateAsync(order, cancellationToken);
            await unitOfWork.SaveChangesAsync(cancellationToken);
            await unitOfWork.CommitTransactionAsync(cancellationToken);
            logger.LogInformation("Order cancelled successfully: {OrderId}", id);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to cancel order: {OrderId}", id);
            await unitOfWork.RollbackTransactionAsync(cancellationToken);
            throw;
        }
    }

}