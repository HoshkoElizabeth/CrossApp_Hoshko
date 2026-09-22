# CrossApp_Hoshko

Наскрізний проєкт з крос-платформного програмування.

Предметна область: **Замовлення**.
Сутності: Customer (клієнт), Product (товар), Order (замовлення), OrderLine (рядок замовлення).
Призначення: оформлення замовлень клієнтів та підрахунок сум.

## Структура solution

```
CrossApp_Hoshko/
  CrossApp_Hoshko.slnx
  README.md
  .gitignore
  src/
    Core/
      Core.csproj
      EnvironmentInfo.cs
    Cli/
      Cli.csproj (ProjectReference на Core)
      Program.cs
```

## Запуск

```
dotnet build
dotnet run --project src/Cli
```

## Запуск з JSON-виводом

```
dotnet run --project src/Cli -- --json
```

## Публікація

```
dotnet publish src/Cli -c Release -r win-x64 --self-contained true
dotnet publish src/Cli -c Release -r win-x64 --self-contained false
```

Запуск без dotnet run:
```
.\src\Cli\bin\Release\net10.0\win-x64\publish\Cli.exe
```

### Порівняння режимів публікації (RID win-x64)

| RID | Режим | Розмір publish | Потрібен runtime |
|---|---|---|---|
| win-x64 | self-contained | 76.85 МБ | ні |
| win-x64 | framework-dependent | 0.19 МБ | так (.NET 10) |

Self-contained публікація включає весь .NET Runtime у складі publish-каталогу,
тому може запускатись на машині без встановленого .NET, але займає значно більше місця.

Framework-dependent публікація містить лише скомпільований код застосунку —
набагато менший розмір, але потребує встановленого .NET 10 Runtime на цільовій машині.

## Середовище

.NET SDK 10.0.401, Windows 10 x64
