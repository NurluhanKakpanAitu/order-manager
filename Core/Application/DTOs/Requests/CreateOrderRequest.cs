namespace Application.DTOs.Requests;

public record CreateOrderRequest(
    CreateOrderItemRequest[] Items
);

public record CreateOrderItemRequest(
    Guid ProductId,
    int Quantity
);