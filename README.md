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
  data/
    sample.csv        (товари: 12 коректних + 3 пошкоджені рядки)
  src/
    Core/
      Core.csproj
      EnvironmentInfo.cs
      Dto/
        ProductDto.cs
        CustomerDto.cs
        ImportResult.cs
      Import/
        ProductCsvImporter.cs
    Cli/
      Cli.csproj (ProjectReference на Core)
      Program.cs
```

## Збірка

```
dotnet build
```

## Імпорт товарів

Запускати з кореня репозиторію. Без аргументів імпортується `data/sample.csv`:

```
dotnet run --project src/Cli -f net10.0
dotnet run --project src/Cli -f net10.0 -- шлях\до\файлу.csv
```

Код завершення: `0` — імпорт виконано (навіть якщо частину рядків пропущено),
`1` — файл не знайдено.

## Інформація про середовище (лаб. 1-2)

```
dotnet run --project src/Cli -f net10.0 -- --env
dotnet run --project src/Cli -f net10.0 -- --json
```

## Формат файлу даних

`data/sample.csv`:
- кодування UTF-8;
- роздільник — крапка з комою `;` (кома може траплятися в назвах);
- колонки: `id;name;price`;
- перший рядок може бути заголовком — він пропускається; файл без заголовка теж підтримується;
- порожні рядки та рядки, що починаються з `#`, ігноруються;
- ціна — з десятковою **крапкою** (`12.50`), розбирається з `CultureInfo.InvariantCulture`;
  `12,50` вважається помилкою, щоб не перетворитися мовчки на 1250.

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