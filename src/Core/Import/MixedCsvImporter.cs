using System.Text;
using Core.Dto;

namespace Core.Import;

// Різнорідні рядки: "P;id;name;price" — товар, "C;id;name[;email]" — клієнт.
public static class MixedCsvImporter
{
    private const char Separator = ';';

    public static MixedImportResult Load(string path)
    {
        var products = new List<ProductDto>();
        var customers = new List<CustomerDto>();
        var errors = new List<string>();

        string[] lines = File.ReadAllLines(path, Encoding.UTF8);
        for (int i = 0; i < lines.Length; i++)
        {
            int number = i + 1;
            string line = lines[i];

            if (string.IsNullOrWhiteSpace(line) || line.StartsWith('#'))
                continue;

            switch (ParseLine(line))
            {
                case ProductRow row:
                    products.Add(row.Value);
                    break;
                case CustomerRow row:
                    customers.Add(row.Value);
                    break;
                case ParseFailed failed:
                    errors.Add($"рядок {number}: {failed.Reason}");
                    break;
            }
        }

        return new MixedImportResult(products, customers, errors);
    }

    private static ParseOutcome ParseLine(string line)
    {
        string[] parts = line.Split(Separator, StringSplitOptions.TrimEntries);

        return parts switch
        {
            // Товари
            ["P", "", _, _] or ["P", _, "", _]
                => new ParseFailed("товар: Id або назва порожні"),
            ["P", _, _, var price] when !ProductCsvImporter.TryParsePrice(price, out decimal p) || p < 0
                => new ParseFailed($"товар: ціна '{price}' не є невід'ємним числом"),
            ["P", var id, var name, var price]
                => new ProductRow(new ProductDto(id, name, ProductCsvImporter.ParsePrice(price))),
            ["P", ..]
                => new ParseFailed($"товар: очікую 4 колонки, отримав {parts.Length}"),

            // Клієнти (email необов'язковий)
            ["C", "", ..] or ["C", _, "", ..]
                => new ParseFailed("клієнт: Id або ім'я порожні"),
            ["C", var id, var name]
                => new CustomerRow(new CustomerDto(id, name)),
            ["C", var id, var name, var email]
                => new CustomerRow(new CustomerDto(id, name, email is "" ? null : email)),
            ["C", ..]
                => new ParseFailed($"клієнт: очікую 3-4 колонки, отримав {parts.Length}"),

            _ => new ParseFailed($"невідомий тип рядка '{parts[0]}'")
        };
    }

    private abstract record ParseOutcome;
    private sealed record ProductRow(ProductDto Value) : ParseOutcome;
    private sealed record CustomerRow(CustomerDto Value) : ParseOutcome;
    private sealed record ParseFailed(string Reason) : ParseOutcome;
}