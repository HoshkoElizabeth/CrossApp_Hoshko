using Core.Events;

namespace Cli;

// Один формат рядка для обробника події і для sink-ів (лаб. 6)
public static class OrderChangeFormat
{
    public static string Line(OrderChangedEventArgs e) =>
        $"{e.At:HH:mm:ss} {e.Kind,-14} {e.OrderId} рядків: {e.LinesCount,2} → нова сума {e.Total:F2}";
}
