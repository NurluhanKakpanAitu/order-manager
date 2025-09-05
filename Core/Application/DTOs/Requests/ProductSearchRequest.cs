namespace Application.DTOs.Requests;

public record ProductSearchRequest(
    string? SearchTerm = null,
    Guid? CategoryId = null,
    decimal? MinPrice = null,
    decimal? MaxPrice = null
);