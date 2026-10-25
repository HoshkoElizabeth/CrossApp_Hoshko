namespace Core.Dto;

// Status зберігається рядком ("Draft", "Confirmed", "Cancelled")
public record OrderDto(string Id, string CustomerId, string Status, IReadOnlyList<OrderLineDto> Lines);