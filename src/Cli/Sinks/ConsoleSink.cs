using Core.Abstractions;
using Core.Events;

namespace Cli.Sinks;

public sealed class ConsoleSink : INotificationSink
{
    public void Notify(OrderChangedEventArgs change) =>
        Console.WriteLine($"[ConsoleSink] {OrderChangeFormat.Line(change)}");
}