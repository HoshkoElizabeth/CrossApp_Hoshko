using Core.Abstractions;
using Core.Domain;
using Core.Events;

namespace Core.Services;

// sinks — дод. 1 (лаб. 6): необов'язкові одержувачі сповіщень через конструктор (ручний DI)
public sealed class OrderService(IOrderStore store, IEnumerable<INotificationSink>? sinks = null)
{
    private readonly IOrderStore _store = store ?? throw new ArgumentNullException(nameof(store));
    private readonly IReadOnlyList<INotificationSink> _sinks = sinks?.ToList() ?? [];

    // Лаб. 6: сповіщення про успішні зміни; як їх обробити — вирішує той, хто підписався
    public event EventHandler<OrderChangedEventArgs>? Changed;

    private void OnChanged(Order order, OrderChangeKind kind)
    {
        var change = new OrderChangedEventArgs(order.Id, kind, order.Lines.Count, order.Total);
        Changed?.Invoke(this, change);
        foreach (INotificationSink sink in _sinks)      // дод. 1: прямий виклик відомих сервісу одержувачів
            sink.Notify(change);
    }

    public Order CreateOrder(string customerId)
    {
        OrderLimitPolicy.EnsureCanOpen(customerId, _store.List());
        var order = Order.Create(Guid.NewGuid().ToString("N")[..8], customerId);
        _store.Add(order);
        OnChanged(order, OrderChangeKind.OrderCreated);     // після зміни, не до
        return order;
    }

    public void AddLine(string orderId, string productId, string name, decimal price, int quantity)
    {
        var order = _store.GetById(orderId)
            ?? throw new InvalidOperationException($"Немає запису з id={orderId}.");
        order.AddLine(productId, name, price, quantity);    // якщо кине виняток — події не буде
        _store.Update(order);
        OnChanged(order, OrderChangeKind.LineAdded);
    }

    public void Confirm(string orderId)
    {
        var order = _store.GetById(orderId)
            ?? throw new InvalidOperationException($"Немає запису з id={orderId}.");
        order.Confirm();                                    // якщо кине виняток — події не буде
        _store.Update(order);
        OnChanged(order, OrderChangeKind.OrderConfirmed);
    }

    public IReadOnlyList<Order> All() => _store.List();
    public Order? Find(string id) => _store.GetById(id);

    // Дод. 2 (лаб. 5); у лаб. 6 — пошук через Func: потрібне значення у відповідь, тому не подія
    public IReadOnlyList<Order> Search(Func<Order, bool> predicate)
    {
        ArgumentNullException.ThrowIfNull(predicate);
        return _store.List().Where(predicate).ToList();
    }
}