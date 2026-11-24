using Core.Events;

namespace Core.Abstractions;

// Дод. 1 (лаб. 6): одержувач сповіщень, якого сервіс отримує через конструктор.
public interface INotificationSink
{
    void Notify(OrderChangedEventArgs change);
}