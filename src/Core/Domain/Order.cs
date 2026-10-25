using Core.Dto;

namespace Core.Domain;

public sealed class Order
{
    private readonly List<OrderLine> _lines = [];

    public string Id { get; }
    public string CustomerId { get; }
    public OrderStatus Status { get; private set; }
    public IReadOnlyList<OrderLine> Lines => _lines.AsReadOnly();
    public decimal Total => _lines.Sum(l => l.Total);

    private Order(string id, string customerId)
    {
        Id = id;
        CustomerId = customerId;
        Status = OrderStatus.Draft;
    }

    
    public static Order Create(string id, string customerId)
    {
        if (string.IsNullOrWhiteSpace(id))
            throw new ArgumentException("Номер замовлення обов'язковий", nameof(id));
        if (string.IsNullOrWhiteSpace(customerId))
            throw new ArgumentException($"Клієнт замовлення {id} обов'язковий", nameof(customerId));

        return new Order(id.Trim(), customerId.Trim());
    }

    public void AddLine(string productId, string name, decimal price, int quantity)
    {
        if (Status != OrderStatus.Draft)
            throw new InvalidOperationException(
                $"Замовлення {Id} має статус {Status}, рядки можна додавати лише до чернетки");

        OrderLine line = OrderLine.Create(productId, name, price, quantity);
        _lines.Add(line);
    }

    public void Confirm()
    {
        if (Status == OrderStatus.Draft && _lines.Count == 0)
            throw new InvalidOperationException(
                $"Замовлення {Id} порожнє, підтвердити його неможливо");

        MoveTo(OrderStatus.Confirmed);
    }

    public void Cancel() => MoveTo(OrderStatus.Cancelled);

    
    private static bool IsAllowed(OrderStatus from, OrderStatus to) => (from, to) switch
    {
        (OrderStatus.Draft, OrderStatus.Confirmed) => true,
        (OrderStatus.Draft, OrderStatus.Cancelled) => true,
        (OrderStatus.Confirmed, OrderStatus.Cancelled) => true,
        _ => false
    };

    private void MoveTo(OrderStatus next)
    {
        if (!IsAllowed(Status, next))
            throw new InvalidOperationException(
                $"Замовлення {Id}: перехід {Status} -> {next} неможливий");

        Status = next;
    }


    public OrderDto ToDto() => new(
        Id,
        CustomerId,
        Status.ToString(),
        _lines.Select(l => new OrderLineDto(l.ProductId, l.Name, l.Price, l.Quantity)).ToList());

    
    public static Order FromDto(OrderDto dto)
    {
        if (!Enum.TryParse(dto.Status, ignoreCase: true, out OrderStatus status) || !Enum.IsDefined(status))
            throw new ArgumentException($"Невідомий статус замовлення {dto.Id}: '{dto.Status}'", nameof(dto));

        Order order = Create(dto.Id, dto.CustomerId);
        foreach (OrderLineDto line in dto.Lines ?? [])
            order.AddLine(line.ProductId, line.Name, line.Price, line.Quantity);

        switch (status)
        {
            case OrderStatus.Confirmed:
                order.Confirm();
                break;
            case OrderStatus.Cancelled:
                order.Cancel();
                break;
        }

        return order;
    }

    public override string ToString() =>
        $"{Id} (клієнт {CustomerId}) [{Status}] — рядків: {_lines.Count}, сума {Total:F2}";
}