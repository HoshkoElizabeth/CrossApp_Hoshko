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
    sample.json       (товари у JSON: 4 коректні + 2 пошкоджені елементи)
    mixed.csv         (товари і клієнти в одному файлі: 6 коректних + 3 пошкоджені рядки)
  src/
    Core/
      Core.csproj
      EnvironmentInfo.cs
      Dto/                (формат даних)
        ProductDto.cs
        CustomerDto.cs
        OrderDto.cs
        OrderLineDto.cs
        ImportResult.cs
        MixedImportResult.cs
      Domain/             (сутності з поведінкою та інваріантами)
        Product.cs
        Order.cs
        OrderLine.cs
      Import/
        ProductCsvImporter.cs
        ProductJsonImporter.cs
        MixedCsvImporter.cs
    Cli/
      Cli.csproj (ProjectReference на Core)
      Program.cs
```

## Збірка

```
dotnet build
```

## Імпорт даних

Запускати з кореня репозиторію. Без аргументів імпортується `data/sample.csv`.
Імпортер товарів обирається за розширенням файлу: `.csv` або `.json`.

```
dotnet run --project src/Cli -f net10.0
dotnet run --project src/Cli -f net10.0 -- data\sample.json
dotnet run --project src/Cli -f net10.0 -- шлях\до\файлу.csv
dotnet run --project src/Cli -f net10.0 -- --mixed
dotnet run --project src/Cli -f net10.0 -- --mixed шлях\до\файлу.csv
```

Після імпорту товарів виводиться рядок статистики, наприклад:

```
Статистика: усього 15 / прийнято 12 / пропущено 3 / помилок 20,0%
```

Код завершення: `0` — імпорт виконано (навіть якщо частину рядків пропущено),
`1` — файл не знайдено або формат не підтримується.

## Доменна модель (лаб. 4)

```
dotnet run --project src/Cli -f net10.0 -- --domain
```

Сутності (`src/Core/Domain`):
- **Order** — замовлення (агрегат): `Create`, `AddLine`, `Confirm`, `ToDto`/`FromDto`;
- **OrderLine** — рядок замовлення, створюється лише через `Order.AddLine`;
- **Product** — товар: `Create`, `ToDto`/`FromDto`.

Публічних конструкторів немає, стан змінюють лише методи, рядки назовні — `IReadOnlyList`.
`FromDto` відновлює сутність через ті самі фабрики й методи, тобто ті самі перевірки.
Records тижня 3 (`Dto/`) лишились форматом даних; зв'язок із сутностями — `ToDto`/`FromDto`.
Domain не залежить ні від `Console`, ні від `File`, ні від проєкту Cli.

### Інваріанти

| Правило | Тип винятку | Де перевіряється |
|---|---|---|
| Номер замовлення не порожній | `ArgumentException` | `Order.Create` |
| Клієнт замовлення не порожній | `ArgumentException` | `Order.Create` |
| Товар у рядку (Id, назва) не порожній | `ArgumentException` | `Order.AddLine` |
| Кількість у рядку > 0 | `ArgumentOutOfRangeException` | `Order.AddLine` |
| Ціна в рядку ≥ 0 | `ArgumentOutOfRangeException` | `Order.AddLine` |
| Не можна додавати рядки до підтвердженого замовлення | `InvalidOperationException` | `Order.AddLine` |
| Порожнє замовлення не можна підтвердити | `InvalidOperationException` | `Order.Confirm` |
| Не можна підтвердити замовлення повторно | `InvalidOperationException` | `Order.Confirm` |
| Id і назва товару не порожні | `ArgumentException` | `Product.Create` |
| Ціна товару ≥ 0 | `ArgumentOutOfRangeException` | `Product.Create` |

## Інформація про середовище (лаб. 1-2)

```
dotnet run --project src/Cli -f net10.0 -- --env
dotnet run --project src/Cli -f net10.0 -- --json
```

## Формат файлів даних

`data/sample.csv`:
- кодування UTF-8;
- роздільник — крапка з комою `;` (кома може траплятися в назвах);
- колонки: `id;name;price`;
- перший рядок може бути заголовком — він пропускається; файл без заголовка теж підтримується;
- порожні рядки та рядки, що починаються з `#`, ігноруються;
- ціна — з десятковою **крапкою** (`12.50`), розбирається з `CultureInfo.InvariantCulture`;
  `12,50` вважається помилкою, щоб не перетворитися мовчки на 1250.

`data/sample.json`:
- кодування UTF-8;
- масив об'єктів `{ "id": ..., "name": ..., "price": ... }`, регістр імен властивостей не важливий;
- елементи з порожнім `id`/`name` або від'ємною ціною пропускаються з повідомленням про помилку.

`data/mixed.csv` (режим `--mixed`):
- кодування UTF-8, роздільник `;`, без заголовка, рядки з `#` — коментарі;
- тип рядка визначає перша колонка:
  - `P;id;name;price` — товар;
  - `C;id;name;email` — клієнт, email необов'язковий (колонку можна не писати або залишити порожньою).

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