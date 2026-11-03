using Core.Domain;

namespace Core;

public static class SampleData
{
    public static IReadOnlyList<Order> Orders() =>
    [
        Build("O-001", "C-001", OrderStatus.Confirmed, ("P-001", "Ноутбук ASUS VivoBook 15", 24999.99m, 1), ("P-002", "Миша бездротова Logitech", 499.50m, 1)),
        Build("O-002", "C-001", OrderStatus.Draft, ("P-010", "Килимок для миші", 199.00m, 2)),
        Build("O-003", "C-002", OrderStatus.Confirmed, ("P-004", "Монітор 24\" Full HD", 6499.00m, 2)),
        Build("O-004", "C-002", OrderStatus.Cancelled, ("P-011", "Принтер лазерний", 5999.00m, 1)),
        Build("O-005", "C-003", OrderStatus.Confirmed, ("P-003", "Клавіатура механічна", 1899.00m, 1), ("P-005", "Навушники бездротові", 1299.99m, 1)),
        Build("O-006", "C-003", OrderStatus.Draft, ("P-008", "Кабель USB-C 2м", 149.90m, 3)),
        Build("O-007", "C-004", OrderStatus.Confirmed, ("P-012", "Планшет 10\"", 8999.00m, 1)),
        Build("O-008", "C-004", OrderStatus.Draft, ("P-007", "Зовнішній SSD 1TB", 2199.00m, 1), ("P-009", "Повербанк 20000mAh", 899.99m, 1)),
        Build("O-009", "C-004", OrderStatus.Draft),
        Build("O-010", "C-005", OrderStatus.Confirmed, ("P-006", "Веб-камера HD", 899.00m, 2)),
        Build("O-011", "C-005", OrderStatus.Cancelled),
        Build("O-012", "C-005", OrderStatus.Confirmed, ("P-002", "Миша бездротова Logitech", 499.50m, 4)),
        Build("O-013", "C-006", OrderStatus.Draft, ("P-001", "Ноутбук ASUS VivoBook 15", 24999.99m, 2)),
        Build("O-014", "C-006", OrderStatus.Confirmed, ("P-005", "Навушники бездротові", 1299.99m, 1), ("P-010", "Килимок для миші", 199.00m, 1)),
        Build("O-015", "C-007", OrderStatus.Cancelled, ("P-004", "Монітор 24\" Full HD", 6499.00m, 1)),
        Build("O-016", "C-007", OrderStatus.Confirmed, ("P-009", "Повербанк 20000mAh", 899.99m, 2), ("P-008", "Кабель USB-C 2м", 149.90m, 2))
    ];

    private static Order Build(string id, string customerId, OrderStatus status,
        params (string ProductId, string Name, decimal Price, int Quantity)[] lines)
    {
        Order order = Order.Create(id, customerId);
        foreach (var (productId, name, price, quantity) in lines)
            order.AddLine(productId, name, price, quantity);

        if (status == OrderStatus.Confirmed)
            order.Confirm();
        else if (status == OrderStatus.Cancelled)
            order.Cancel();

        return order;
    }
}