namespace Domain.Interfaces;

public interface IPaymentService
{
    Task<bool> ProcessPaymentAsync(Guid orderId, decimal amount, CancellationToken cancellationToken = default);
    Task<bool> RefundPaymentAsync(Guid orderId, CancellationToken cancellationToken = default);
}