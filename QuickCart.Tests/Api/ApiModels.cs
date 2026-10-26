namespace QuickCart.Tests.Api;

public sealed record ProductDto(int Id, string Name, string Category, decimal Price, int Stock);

public sealed record LoginResponse(string Token, string Name);

public sealed record OrderItem(int ProductId, int Quantity);

public sealed record Customer(string Name, string Email, string Postcode);

public sealed record OrderRequest(IReadOnlyList<OrderItem> Items, Customer Customer);

public sealed record OrderResponse(int Id, IReadOnlyList<OrderItem> Items, Customer Customer, decimal Total, string Status);
