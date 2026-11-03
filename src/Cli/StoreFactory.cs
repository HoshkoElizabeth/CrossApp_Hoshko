using Core;
using Core.Abstractions;
using Core.Storage;

namespace Cli;

// Дод. 3(5): вибір реалізації сховища, винесений з Program.cs в окремий клас
public static class StoreFactory
{
    public static IOrderStore Create(string[] args)
    {
        string dataPath = Path.Combine(AppContext.BaseDirectory, "data", "orders.json");

        IOrderStore store = args.Contains("--file")
            ? new FileOrderStore(dataPath)
            : new InMemoryOrderStore(SampleData.Orders());

        return args.Contains("--cache") ? new CachingOrderStore(store) : store;
    }
}