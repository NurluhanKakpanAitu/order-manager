using Domain.Interfaces;

namespace Infrastructure.Persistence.Services;

public class PaymentService : IPaymentService
{
    public async Task<bool> ProcessPaymentAsync(Guid orderId, decimal amount,
        CancellationToken cancellationToken = default)
    {
        await Task.Delay(100, cancellationToken);

        if (amount <= 0)
            return false;

        var random = new Random();
        var success = random.Next(1, 11) > 1;

        if (success)
            Console.WriteLine($"Payment processed successfully for order {orderId}. Amount: {amount:C}");
        else
            Console.WriteLine($"Payment failed for order {orderId}. Amount: {amount:C}");

        return success;
    }

    public async Task<bool> RefundPaymentAsync(Guid orderId, CancellationToken cancellationToken = default)
    {
        await Task.Delay(100, cancellationToken);

        var random = new Random();
        var success = random.Next(1, 11) > 1;

        if (success)
            Console.WriteLine($"Refund processed successfully for order {orderId}");
        else
            Console.WriteLine($"Refund failed for order {orderId}");

        return success;
    }
}