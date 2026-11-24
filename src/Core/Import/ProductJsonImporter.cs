using System.Text;
using System.Text.Json;
using Core.Dto;

namespace Core.Import;

public static class ProductJsonImporter
{
    private static readonly JsonSerializerOptions Options = new() { PropertyNameCaseInsensitive = true };

    // onSkipped — дод. 2 (лаб. 6): викликається для кожного пропущеного елемента; що з ним робити, вирішує викликач
    public static ImportResult<ProductDto> Load(string path, Action<string>? onSkipped = null)
    {
        string json = File.ReadAllText(path, Encoding.UTF8);

        List<ProductDto?> raw;
        try
        {
            raw = JsonSerializer.Deserialize<List<ProductDto?>>(json, Options) ?? [];
        }
        catch (JsonException ex)
        {
            string error = $"некоректний JSON: {ex.Message}";
            onSkipped?.Invoke(error);
            return new ImportResult<ProductDto>([], [error]);
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
                    onSkipped?.Invoke(errors[^1]);
                    break;
                case { Id: null or "" } or { Name: null or "" }:
                    errors.Add($"{where}: Id або назва порожні");
                    onSkipped?.Invoke(errors[^1]);
                    break;
                case { Price: < 0 } p:
                    errors.Add($"{where}: ціна {p.Price} від'ємна");
                    onSkipped?.Invoke(errors[^1]);
                    break;
                case var p:
                    items.Add(p);
                    break;
            }
        }

        return new ImportResult<ProductDto>(items, errors);
    }
}