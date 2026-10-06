using System.Text;
using System.Text.Json;
using Core;
using Core.Dto;
using Core.Import;

Console.OutputEncoding = Encoding.UTF8;

// ---- Лабораторні 1-2: інформація про середовище (--env текстом, --json у JSON) ----
if (args.Contains("--json") || args.Contains("--env"))
{
    EnvironmentReport report = EnvironmentInfo.Collect();

    if (args.Contains("--json"))
    {
        Console.WriteLine(JsonSerializer.Serialize(report));
    }
    else
    {
        Console.WriteLine("CrossApp_Hoshko – інформація про середовище");
        Console.WriteLine(new string('-', 52));
        Console.WriteLine($"ОС              : {report.OsDescription}");
        Console.WriteLine($"Runtime         : {report.FrameworkDescription}");
        Console.WriteLine($"Архітектура     : {report.ProcessArchitecture}");
        Console.WriteLine($"RID (визначено) : {report.DetectedRid}");
        Console.WriteLine($"RID (від .NET)  : {report.ReportedRid}");
        Console.WriteLine($"Каталог         : {report.BaseDirectory}");
        Console.WriteLine(new string('-', 52));
        Console.WriteLine("Предметна область: Замовлення (клієнти, товари, замовлення, рядки замовлення)");
        Console.WriteLine($"Примітка збірки : {report.BuildNote}");
    }
    return 0;
}

// ---- Лабораторна 3: імпорт даних ----
bool mixed = args.Contains("--mixed");
string path = args.FirstOrDefault(a => !a.StartsWith("--"))
              ?? Path.Combine("data", mixed ? "mixed.csv" : "sample.csv");

if (!File.Exists(path))
{
    Console.WriteLine($"Файл не знайдено: {Path.GetFullPath(path)}");
    return 1;
}

// Додаткове завдання 2: різнорідні рядки (товари + клієнти)
if (mixed)
{
    MixedImportResult m = MixedCsvImporter.Load(path);
    Console.WriteLine($"Товарів: {m.Products.Count}, клієнтів: {m.Customers.Count}");
    foreach (ProductDto p in m.Products)
        Console.WriteLine($"  {p.Id,-6} {p.Name,-28} {p.Price,10:F2}");
    foreach (CustomerDto c in m.Customers)
        Console.WriteLine($"  {c.Id,-6} {c.Name,-28} {c.Email ?? "(без email)"}");
    PrintErrors(m.Errors);
    return 0;
}

// Додаткове завдання 1: імпортер за розширенням файлу
ImportResult<ProductDto>? result = Path.GetExtension(path).ToLowerInvariant() switch
{
    ".csv" => ProductCsvImporter.Load(path),
    ".json" => ProductJsonImporter.Load(path),
    _ => null
};

if (result is null)
{
    Console.WriteLine($"Непідтримуваний формат файлу: {Path.GetExtension(path)} (очікую .csv або .json)");
    return 1;
}

Console.WriteLine($"Завантажено записів: {result.Items.Count}");
foreach (ProductDto p in result.Items.Take(5))
    Console.WriteLine($"  {p.Id,-6} {p.Name,-28} {p.Price,10:F2}");

PrintErrors(result.Errors);

return 0;

static void PrintErrors(IReadOnlyList<string> errors)
{
    if (errors.Count == 0)
        return;

    Console.WriteLine($"Пропущено рядків: {errors.Count}");
    foreach (string e in errors)
        Console.WriteLine($"  ! {e}");
}