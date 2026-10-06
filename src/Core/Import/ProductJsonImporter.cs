using System.Text;
using System.Text.Json;
using Core.Dto;

namespace Core.Import;

public static class ProductJsonImporter
{
    private static readonly JsonSerializerOptions Options = new() { PropertyNameCaseInsensitive = true };

    public static ImportResult<ProductDto> Load(string path)
    {
        string json = File.ReadAllText(path, Encoding.UTF8);

        List<ProductDto?> raw;
        try
        {
            raw = JsonSerializer.Deserialize<List<ProductDto?>>(json, Options) ?? [];
        }
        catch (JsonException ex)
        {
            return new ImportResult<ProductDto>([], [$"некоректний JSON: {ex.Message}"]);
        }

        var items = new List<ProductDto>();
        var errors = new List<string>();

        for (int i = 0; i < raw.Count; i++)
        {
            string where = $"елемент {i + 1}";
            // Ті самі правила, що й для CSV: JSON не гарантує, що поля заповнені.
            switch (raw[i])
            {
                case null:
                    errors.Add($"{where}: порожній елемент");
                    break;
                case { Id: null or "" } or { Name: null or "" }:
                    errors.Add($"{where}: Id або назва порожні");
                    break;
                case { Price: < 0 } p:
                    errors.Add($"{where}: ціна {p.Price} від'ємна");
                    break;
                case var p:
                    items.Add(p);
                    break;
            }
        }

        return new ImportResult<ProductDto>(items, errors);
    }
}