namespace Core.Events;

// Вид зміни замовлення, про яку сповіщає OrderService.Changed
public enum OrderChangeKind
{
    OrderCreated,
    LineAdded,
    OrderConfirmed
}