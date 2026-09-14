using System.Runtime.InteropServices;
using System.Text.Json;

var info = new
{
    Student = "Hoshko Yelyzaveta, group FEI-36",
    OsDescription = RuntimeInformation.OSDescription,
    OsEnvironment = Environment.OSVersion.ToString(),
    ProcessArchitecture = RuntimeInformation.ProcessArchitecture.ToString(),
    DotnetVersion = Environment.Version.ToString(),
    Runtime = RuntimeInformation.FrameworkDescription,
    AppDirectory = AppContext.BaseDirectory,
    CurrentDirectory = Environment.CurrentDirectory,
    Domain = "Orders (customers, products, orders, order lines)"
};

if (args.Contains("--json"))
{
    string json = JsonSerializer.Serialize(info);
    Console.WriteLine(json);
}
else
{
    Console.WriteLine("CrossApp_Hoshko - практикум з крос-платформного програмування");
    Console.WriteLine($"Студент: {info.Student}");
    Console.WriteLine(new string('-', 52));
    Console.WriteLine($"ОС (OSDescription)   : {info.OsDescription}");
    Console.WriteLine($"ОС (Environment)     : {info.OsEnvironment}");
    Console.WriteLine($"Архітектура процесу  : {info.ProcessArchitecture}");
    Console.WriteLine($"Версія .NET (CLR)    : {info.DotnetVersion}");
    Console.WriteLine($"Runtime              : {info.Runtime}");
    Console.WriteLine($"Каталог застосунку   : {info.AppDirectory}");
    Console.WriteLine($"Поточний каталог     : {info.CurrentDirectory}");
    Console.WriteLine(new string('-', 52));
    Console.WriteLine($"Предметна область: {info.Domain}");
}