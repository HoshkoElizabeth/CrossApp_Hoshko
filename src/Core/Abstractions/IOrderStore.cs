using Core.Domain;

namespace Core.Abstractions;

// Контракт сховища замовлень: лише операції, потрібні сервісу.
// Агрегат Order зберігається як одне ціле: GetById повертає замовлення разом з рядками.
public interface IOrderStore
{
    IReadOnlyList<Order> List();
    Order? GetById(string id);
    void Add(Order item);
    void Update(Order item);
    bool Remove(string id);
}