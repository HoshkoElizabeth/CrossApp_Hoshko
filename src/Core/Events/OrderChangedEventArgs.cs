namespace Core.Events;

// Дані події: яке замовлення, що сталося і який стан після зміни
public sealed class OrderChangedEventArgs(
    string orderId, OrderChangeKind kind, int linesCount, decimal total) : EventArgs
{
    public string OrderId { get; } = orderId;
    public OrderChangeKind Kind { get; } = kind;
    public int LinesCount { get; } = linesCount;     // кількість рядків після зміни
    public decimal Total { get; } = total;           // нова сума замовлення
    public DateTimeOffset At { get; } = DateTimeOffset.Now;
}