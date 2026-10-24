namespace Core.Domain;

// Додаткове завдання 2: інваріант, що охоплює ДВІ сутності (клієнт і його замовлення).
// Окреме замовлення бачить лише себе і не знає про інші замовлення клієнта

public static class OrderLimitPolicy
{
    public const int MaxDraftOrders = 3;

    public static void EnsureCanOpen(string customerId, IEnumerable<Order> customerOrders)
    {
        if (string.IsNullOrWhiteSpace(customerId))
            throw new ArgumentException("Клієнт обов'язковий", nameof(customerId));

        string id = customerId.Trim();
        int drafts = customerOrders.Count(o => o.CustomerId == id && !o.IsConfirmed);

        if (drafts >= MaxDraftOrders)
            throw new InvalidOperationException(
                $"Клієнт {id} уже має {drafts} незавершених замовлення (ліміт {MaxDraftOrders}), нове відкрити неможливо");
    }
}