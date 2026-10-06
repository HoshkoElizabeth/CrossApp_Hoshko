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

// ---- Лабораторна 3: імпорт товарів (CSV або JSON) ----
string path = args.Length > 0 ? args[0] : Path.Combine("data", "sample.csv");

if (!File.Exists(path))
{
    Console.WriteLine($"Файл не знайдено: {Path.GetFullPath(path)}");
    return 1;
}

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

if (result.Errors.Count > 0)
{
    Console.WriteLine($"Пропущено рядків: {result.Errors.Count}");
    foreach (string e in result.Errors)
        Console.WriteLine($"  ! {e}");
}

return 0;