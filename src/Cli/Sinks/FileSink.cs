using Core.Abstractions;
using Core.Events;

namespace Cli.Sinks;

public sealed class FileSink : INotificationSink
{
    private readonly string _path;

    public FileSink(string path)
    {
        _path = path;
        Directory.CreateDirectory(Path.GetDirectoryName(_path)!);
    }

    public void Notify(OrderChangedEventArgs change)
    {
        try
        {
            File.AppendAllText(_path, OrderChangeFormat.Line(change) + Environment.NewLine);
        }
        catch (IOException ex)       // помилка журналу не має ламати бізнес-операцію
        {
            Console.WriteLine($"  (FileSink: не вдалося записати в журнал: {ex.Message})");
        }
    }
}