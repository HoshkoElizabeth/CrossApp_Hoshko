using Core.Abstractions;
using Core.Domain;

namespace Core.Storage;

// Декоратор: приймає інший IOrderStore і сам реалізує IOrderStore
// кеш читань: повторні List/GetById не доходять до внутрішнього сховища
// Будь-яка зміна передається далі і скидає кеш
public sealed class CachingOrderStore(IOrderStore inner) : IOrderStore
{
    private readonly IOrderStore _inner = inner ?? throw new ArgumentNullException(nameof(inner));
    private readonly Dictionary<string, Order?> _byId = new(StringComparer.OrdinalIgnoreCase);
    private IReadOnlyList<Order>? _list;

    public IReadOnlyList<Order> List() => _list ??= _inner.List();

    public Order? GetById(string id)
    {
        if (_byId.TryGetValue(id, out Order? cached))
            return cached;

        Order? order = _inner.GetById(id);
        _byId[id] = order;
        return order;
    }

    public void Add(Order item)
    {
        _inner.Add(item);
        Invalidate();
    }

    public void Update(Order item)
    {
        _inner.Update(item);
        Invalidate();
    }

    public bool Remove(string id)
    {
        bool removed = _inner.Remove(id);
        Invalidate();
        return removed;
    }

    private void Invalidate()
    {
        _list = null;
        _byId.Clear();
    }
}