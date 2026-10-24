namespace Core.Domain;


public sealed class OrderLine
{
    public string ProductId { get; }
    public string Name { get; }
    public decimal Price { get; }
    public int Quantity { get; }
    public decimal Total => Price * Quantity;

    private OrderLine(string productId, string name, decimal price, int quantity)
    {
        ProductId = productId;
        Name = name;
        Price = price;
        Quantity = quantity;
    }

    internal static OrderLine Create(string productId, string name, decimal price, int quantity)
    {
        if (string.IsNullOrWhiteSpace(productId))
            throw new ArgumentException("Ідентифікатор товару в рядку обов'язковий", nameof(productId));
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException($"Назва товару {productId} у рядку не може бути порожньою", nameof(name));
        if (price < 0)
            throw new ArgumentOutOfRangeException(nameof(price), price,
                $"Ціна товару {productId} у рядку не може бути від'ємною");
        if (quantity <= 0)
            throw new ArgumentOutOfRangeException(nameof(quantity), quantity,
                $"Кількість товару {productId} у рядку має бути більшою за нуль");

        return new OrderLine(productId.Trim(), name.Trim(), price, quantity);
    }

    public override string ToString() => $"{ProductId} {Name} — {Quantity} x {Price:F2} = {Total:F2}";
}