using System.Text;
using System.Text.Json;
using Core;
using Core.Domain;
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

// ---- Лабораторна 4: доменна модель (--domain) ----
if (args.Contains("--domain"))
{
    Console.WriteLine("=== Сценарій 1: успіх ===");
    Order order = Order.Create("O-001", "C-001");
    order.AddLine("P-001", "Ноутбук ASUS VivoBook 15", 24999.99m, 1);
    order.AddLine("P-002", "Миша бездротова Logitech", 499.50m, 2);
    Console.WriteLine(order);
    foreach (OrderLine line in order.Lines)
        Console.WriteLine($"  {line}");
    order.Confirm();
    Console.WriteLine(order);

    OrderDto dto = order.ToDto();
    Order restored = Order.FromDto(dto);
    Console.WriteLine($"ToDto -> FromDto: {restored}");
    Console.WriteLine();

    Console.WriteLine("=== Сценарій 2: порушення інваріантів ===");
    Order draft = Order.Create("O-002", "C-002");
    TryDo("порожній номер замовлення", () => Order.Create("  ", "C-001"));
    TryDo("нульова кількість у рядку", () => draft.AddLine("P-003", "Клавіатура механічна", 1899.00m, 0));
    TryDo("від'ємна ціна в рядку", () => draft.AddLine("P-004", "Монітор 24\" Full HD", -1m, 1));
    TryDo("підтвердження порожнього замовлення", () => draft.Confirm());
    TryDo("рядок до підтвердженого замовлення", () => order.AddLine("P-003", "Клавіатура механічна", 1899.00m, 1));
    TryDo("повторне підтвердження", () => order.Confirm());
    TryDo("товар з порожньою назвою", () => Product.Create("P-005", "   ", 100m));
    TryDo("FromDto: підтверджене без рядків", () => Order.FromDto(new OrderDto("O-003", "C-001", "Confirmed", [])));
    TryDo("FromDto: невідомий статус", () => Order.FromDto(new OrderDto("O-004", "C-001", "Shipped", [])));
    Console.WriteLine("Стан після всіх відмов:");
    Console.WriteLine($"  {order}");
    Console.WriteLine($"  {draft}");
    Console.WriteLine();

    Console.WriteLine("=== Додаткове 1: ImportResult -> сутності ===");
    string importPath = args.FirstOrDefault(a => !a.StartsWith("--")) ?? Path.Combine("data", "sample.json");
    ImportResult<ProductDto>? imported = File.Exists(importPath) ? LoadProducts(importPath) : null;
    if (imported is null)
    {
        Console.WriteLine($"  Файл не знайдено або формат не підтримується: {importPath}");
    }
    else
    {
        ImportResult<Product> products = Product.FromImport(imported);
        Console.WriteLine($"  DTO після імпорту: {imported.Items.Count}, сутностей створено: {products.Items.Count}");
        foreach (Product p in products.Items)
            Console.WriteLine($"  {p}");
        PrintErrors(products.Errors);
    }
    Console.WriteLine();

    Console.WriteLine($"=== Додаткове 2: не більше {OrderLimitPolicy.MaxDraftOrders} незавершених замовлень клієнта ===");
    var customerOrders = new List<Order>();
    for (int i = 1; i <= OrderLimitPolicy.MaxDraftOrders; i++)
    {
        OrderLimitPolicy.EnsureCanOpen("C-010", customerOrders);
        customerOrders.Add(Order.Create($"O-10{i}", "C-010"));
    }
    Console.WriteLine($"  Відкрито: {string.Join(", ", customerOrders.Select(o => o.Id))}");
    TryDo("ще одне замовлення", () => OrderLimitPolicy.EnsureCanOpen("C-010", customerOrders));
    customerOrders[0].AddLine("P-006", "Веб-камера HD", 899.00m, 1);
    customerOrders[0].Confirm();
    OrderLimitPolicy.EnsureCanOpen("C-010", customerOrders);
    customerOrders.Add(Order.Create("O-104", "C-010"));
    Console.WriteLine($"  Після підтвердження {customerOrders[0].Id} відкрито O-104");
    Console.WriteLine();

    Console.WriteLine("=== Додаткове 3: переходи статусів ===");
    Order cancelled = Order.Create("O-201", "C-001");
    cancelled.AddLine("P-006", "Веб-камера HD", 899.00m, 1);
    cancelled.Cancel();
    Console.WriteLine($"  Draft -> Cancelled: {cancelled}");
    TryDo("Cancelled -> Confirmed", () => cancelled.Confirm());
    TryDo("Cancelled -> Cancelled", () => cancelled.Cancel());
    order.Cancel();
    Console.WriteLine($"  Confirmed -> Cancelled: {order}");

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

// Додаткове завдання 3(2): різнорідні рядки (товари + клієнти)
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

// Додаткове завдання 3(1): імпортер за розширенням файлу
// Додаткове завдання 1 (лаб. 3): імпортер за розширенням файлу
ImportResult<ProductDto>? result = LoadProducts(path);
if (result is null)
{
    Console.WriteLine($"Непідтримуваний формат файлу: {Path.GetExtension(path)} (очікую .csv або .json)");
    return 1;
}

Console.WriteLine($"Завантажено записів: {result.Items.Count}");
foreach (ProductDto p in result.Items.Take(5))
    Console.WriteLine($"  {p.Id,-6} {p.Name,-28} {p.Price,10:F2}");

PrintErrors(result.Errors);

// Додаткове завдання 3(3): статистика імпорту одним рядком
int total = result.Items.Count + result.Errors.Count;
double errorPercent = total == 0 ? 0 : 100.0 * result.Errors.Count / total;
Console.WriteLine($"Статистика: усього {total} / прийнято {result.Items.Count} / пропущено {result.Errors.Count} / помилок {errorPercent:F1}%");

return 0;

static ImportResult<ProductDto>? LoadProducts(string path) =>
    Path.GetExtension(path).ToLowerInvariant() switch
    {
        ".csv" => ProductCsvImporter.Load(path),
        ".json" => ProductJsonImporter.Load(path),
        _ => null
    };

static void TryDo(string title, Action action)
{
    try
    {
        action();
        Console.WriteLine($"  {title}: виняток НЕ спрацював — інваріант відсутній!");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"  {title}: {ex.GetType().Name} — {ex.Message}");
    }
}

static void PrintErrors(IReadOnlyList<string> errors)
{
    if (errors.Count == 0)
        return;

    Console.WriteLine($"Пропущено рядків: {errors.Count}");
    foreach (string e in errors)
        Console.WriteLine($"  ! {e}");
}