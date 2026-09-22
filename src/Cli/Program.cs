using Core;
using System.Text.Json;

Console.OutputEncoding = System.Text.Encoding.UTF8;

EnvironmentReport report = EnvironmentInfo.Collect();

if (args.Contains("--json"))
{
    string json = JsonSerializer.Serialize(report);
    Console.WriteLine(json);
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
}