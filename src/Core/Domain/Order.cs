using Core.Dto;

namespace Core.Domain;

public sealed class Order
{
    private readonly List<OrderLine> _lines = [];

    public string Id { get; }
    public string CustomerId { get; }
    public bool IsConfirmed { get; private set; }
    public IReadOnlyList<OrderLine> Lines => _lines.AsReadOnly();
    public decimal Total => _lines.Sum(l => l.Total);

    private Order(string id, string customerId)
    {
        Id = id;
        CustomerId = customerId;
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
        if (IsConfirmed)
            throw new InvalidOperationException(
                $"Замовлення {Id} вже підтверджене, рядки додавати не можна");

        OrderLine line = OrderLine.Create(productId, name, price, quantity);
        _lines.Add(line);
    }

    public void Confirm()
    {
        if (IsConfirmed)
            throw new InvalidOperationException(
                $"Замовлення {Id} вже підтверджене, повторне підтвердження неможливе");
        if (_lines.Count == 0)
            throw new InvalidOperationException(
                $"Замовлення {Id} порожнє, підтвердити його неможливо");

        IsConfirmed = true;
    }

    
    public OrderDto ToDto() => new(
        Id,
        CustomerId,
        IsConfirmed,
        _lines.Select(l => new OrderLineDto(l.ProductId, l.Name, l.Price, l.Quantity)).ToList());


    public static Order FromDto(OrderDto dto)
    {
        Order order = Create(dto.Id, dto.CustomerId);
        foreach (OrderLineDto line in dto.Lines ?? [])
            order.AddLine(line.ProductId, line.Name, line.Price, line.Quantity);

        if (dto.IsConfirmed)
            order.Confirm();

        return order;
    }

    public override string ToString() =>
        $"{Id} (клієнт {CustomerId}) [{(IsConfirmed ? "підтверджене" : "чернетка")}] — рядків: {_lines.Count}, сума {Total:F2}";
}