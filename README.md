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
    sample.json       (товари у JSON: 5 проходять імпорт + 2 пошкоджені; P-107 з назвою з пробілів відхиляє домен)
    mixed.csv         (товари і клієнти в одному файлі: 6 коректних + 3 пошкоджені рядки)
  src/
    Core/
      Core.csproj
      EnvironmentInfo.cs
      SampleData.cs       (16 зразкових замовлень для демо і для тижня 7)
      Abstractions/       (контракти)
        IOrderStore.cs
        INotificationSink.cs   (лаб. 6, дод. завдання 1)
      Events/             (лаб. 6: доменні події)
        OrderChangeKind.cs
        OrderChangedEventArgs.cs
      Storage/            (реалізації сховища)
        InMemoryOrderStore.cs
        FileOrderStore.cs
        CachingOrderStore.cs   (дод. завдання 1)
      Services/           (бізнес-операції)
        OrderService.cs
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
        OrderStatus.cs
        OrderLimitPolicy.cs
      Import/
        ProductCsvImporter.cs
        ProductJsonImporter.cs
        MixedCsvImporter.cs
    Cli/
      Cli.csproj (ProjectReference на Core)
      Program.cs          (composition root + сценарії)
      StoreFactory.cs     (дод. завдання 3)
      OrderChangeFormat.cs (лаб. 6: формат рядка журналу)
      Sinks/              (лаб. 6, дод. завдання 1)
        ConsoleSink.cs
        FileSink.cs
```

## Збірка

```
dotnet build
```

## Імпорт даних

Запускати з кореня репозиторію. Шлях до файлу вказується явно (без аргументів запускається сценарій лаб. 5). З `--report` пропущені рядки друкуються під час імпорту (лаб. 6, дод. 2).
Імпортер товарів обирається за розширенням файлу: `.csv` або `.json`.

```
dotnet run --project src/Cli -f net10.0 -- data\sample.csv
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
dotnet run --project src/Cli -f net10.0 -- --domain data\sample.csv
```

Сутності (`src/Core/Domain`):
- **Order** — замовлення (агрегат): `Create`, `AddLine`, `Confirm`, `Cancel`, `ToDto`/`FromDto`;
  статус `OrderStatus` (додаткове завдання 3): `Draft` → `Confirmed`, `Draft` → `Cancelled`, `Confirmed` → `Cancelled`,
  допустимі переходи перевіряються через switch expression;
- **OrderLine** — рядок замовлення, створюється лише через `Order.AddLine`;
- **Product** — товар: `Create`, `ToDto`/`FromDto`, `FromImport` (результат імпорту → сутності + помилки).

`Product.FromImport` (додаткове завдання 1) бере `ImportResult<ProductDto>` і повертає `ImportResult<Product>`:
до помилок імпорту додаються записи, що не пройшли інваріанти домену.
За замовчуванням у режимі `--domain` береться `data/sample.json`.

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
| Рядки додаються лише до чернетки (`Draft`) | `InvalidOperationException` | `Order.AddLine` |
| Порожнє замовлення не можна підтвердити | `InvalidOperationException` | `Order.Confirm` |
| Лише допустимі переходи статусу (не підтвердити повторно, не змінити скасоване) | `InvalidOperationException` | `Order.Confirm` / `Order.Cancel` |
| Статус у DTO — один із `Draft`, `Confirmed`, `Cancelled` | `ArgumentException` | `Order.FromDto` |
| Id і назва товару не порожні | `ArgumentException` | `Product.Create` |
| Ціна товару ≥ 0 | `ArgumentOutOfRangeException` | `Product.Create` |
| У клієнта не більше 3 незавершених (`Draft`) замовлень | `InvalidOperationException` | `OrderLimitPolicy.EnsureCanOpen` |

Останнє правило (додаткове завдання 2) охоплює дві сутності: клієнта і всі його замовлення.
Окреме `Order` бачить лише себе, тому правило винесено з сутності в `OrderLimitPolicy`;
з лаб. 5 його викликає `OrderService.CreateOrder`, отримуючи замовлення клієнта зі сховища.

## Сервісний шар і два сховища (лаб. 5)

```
dotnet run --project src/Cli -f net10.0
dotnet run --project src/Cli -f net10.0 -- --file
dotnet run --project src/Cli -f net10.0 -- --cache
dotnet run --project src/Cli -f net10.0 -- --factory --file --cache
```

- **Інтерфейс:** `IOrderStore` (`src/Core/Abstractions`) — `List`, `GetById`, `Add`, `Update`, `Remove`;
- **Реалізації:** `InMemoryOrderStore` (словник у пам'яті, початкові дані `SampleData.Orders()`)
  і `FileOrderStore` (JSON-файл, кеш у пам'яті, завантаження при першому зверненні, запис після кожної зміни);
- **Сервіс:** `OrderService` (`src/Core/Services`) — `CreateOrder`, `AddLine`, `Confirm`, `All`, `Find`, `Search`;
  залежність `IOrderStore` передається через конструктор.

Composition root — `Program.cs` (і `StoreFactory` з дод. завдання 3, теж у Cli): лише там створюються конкретні сховища і сервіс.
`Microsoft.Extensions.DependencyInjection` не підключено.

```
Cli → OrderService → IOrderStore → InMemoryOrderStore | FileOrderStore
```

Режими:
- без `--file` — `Сховище: InMemoryOrderStore`, після виходу дані зникають;
- з `--file` — `Сховище: FileOrderStore`, створюється `data/orders.json` поруч зі збіркою
  (`src/Cli/bin/Debug/net10.0/data/orders.json`), повторний запуск бачить попередні записи.

Файл зберігає список `OrderDto` разом із рядками (`OrderLineDto`): агрегат зберігається як одне ціле.
Мапінг — `Order.ToDto` / `Order.FromDto`, тож при завантаженні інваріанти перевіряються знову.

Сценарій: створити замовлення, додати рядки, підтвердити, показати список, знайти за id.
Відмова: рядок до неіснуючого id і дубль id — виняток перехоплюється в Cli, друкується зрозуміле повідомлення.

### Додаткові завдання

1. **`CachingOrderStore`** (`src/Core/Storage`) — декоратор: приймає будь-який `IOrderStore` у конструкторі
   і сам реалізує `IOrderStore`. Нова поведінка — кеш читань: повторні `List`/`GetById` не доходять
   до внутрішнього сховища, `Add`/`Update`/`Remove` передаються далі й скидають кеш. Вмикається `--cache`
   (разом з `--file` — кеш поверх файлового сховища).
2. **`OrderService.Search(Func<Order, bool>)`** — умову пошуку передає викликач лямбдою,
   контракт `IOrderStore` не змінюється. У сценарії: підтверджені на суму > 5000 і чернетки клієнта C-004.
3. **`StoreFactory.Create(args)`** (`src/Cli`) — вибір реалізації винесено з `Program.cs`, вмикається `--factory`.
   Порівняння з варіантом «усе в Program.cs»:
   - усе в `Program.cs` — вибір видно одразу в точці входу, але з кожним новим прапорцем (`--file`, `--cache`)
     код розростається й змішується зі сценарієм;
   - `StoreFactory` — рішення «яке сховище створити» в одному іменованому методі, `Program.cs` лише викликає
     `Create(args)`, фабрику можна перевикористати;
   - в обох варіантах конкретні класи лишаються в Cli (composition root), а `OrderService` отримує `IOrderStore`
     і різниці не помічає.

## Доменні події (лаб. 6)

```
dotnet run --project src/Cli -f net10.0 -- --events
dotnet run --project src/Cli -f net10.0 -- --events --file
dotnet run --project src/Cli -f net10.0 -- --sinks
dotnet run --project src/Cli -f net10.0 -- data\sample.csv --report
```

Події домену «Замовлення» (`src/Core/Events`):

| Подія (`OrderChangeKind`) | Коли виникає |
|---|---|
| `OrderCreated` | `OrderService.CreateOrder` — замовлення створено і збережено |
| `LineAdded` | `OrderService.AddLine` — рядок додано і збережено |
| `OrderConfirmed` | `OrderService.Confirm` — замовлення підтверджено і збережено |

Дані події — `OrderChangedEventArgs : EventArgs`: `OrderId`, `Kind`, `LinesCount`, `Total` (нова сума), `At`.

- `OrderService` оголошує `public event EventHandler<OrderChangedEventArgs>? Changed`
  і піднімає її одним приватним методом `OnChanged` через `Changed?.Invoke(...)`;
- `OnChanged` викликається **після** успішної зміни і `_store.Add/Update`: якщо інваріант кинув виняток,
  до `OnChanged` виконання не доходить і сповіщення немає;
- Core не пише ні в консоль, ні у файл журналу (пошук `Console` і `logs` у `src/Core` нічого не знаходить).

Сценарій `--events` у `Program.cs`:
1. перший підписник — локальний метод `OnOrderChanged`: рядок у консоль і в `logs/app.log`
   (`Path.Combine(AppContext.BaseDirectory, "logs", "app.log")`, каталог створює `Directory.CreateDirectory`;
   помилка запису перехоплюється в обробнику, щоб не зламати операцію);
2. другий підписник — лямбда `(_, _) => ...` із замиканням над локальною змінною `changes`;
   обробники викликаються в порядку підписки: спершу рядок журналу, потім лічильник;
3. створення замовлення і два рядки — три події;
4. відмова: рядок з кількістю 0 (інваріант) і рядок до `O-404` — виняток є, рядка події немає, лічильник не змінився;
5. `service.Changed -= OnOrderChanged`, потім `Confirm` — статус змінився, у консолі й журналі рядка немає,
   лічильник +1 (лямбда лишилась підписаною);
6. `service.Search(o => o.Total < 1000m)` — `Func<Order, bool>`: тут потрібне значення у відповідь, тому не подія.

Журнал: `src/Cli/bin/Debug/net10.0/logs/app.log` (дописується з кожним запуском).

Одержувачі сповіщень зараз — консоль і `logs/app.log`; на тижні 10 — логер проєкту Api,
на тижні 12 — компонент Blazor, що оновлює таблицю. Core для цього змінювати не доведеться.

### Додаткові завдання

1. **`INotificationSink`** (`src/Core/Abstractions`) з реалізаціями `ConsoleSink` і `FileSink` (`src/Cli/Sinks`).
   Cli передає їх у `OrderService` через конструктор (`new OrderService(store, sinks)`, ручний DI тижня 5), режим `--sinks`.
   Параметр необов'язковий, тож `new OrderService(store)` з лаб. 5 працює як раніше.
   Порівняння з подією:
   - **подія** — сервіс не знає про одержувачів; підписка й відписка (`+=`/`-=`) у будь-який момент, з будь-якого місця,
     обробником може бути лямбда; але підписку легко забути зняти, а порядок і склад одержувачів видно лише в коді Cli;
   - **sink через конструктор** — одержувачі явно видно в сигнатурі і задаються в composition root разом зі сховищем;
     але список фіксований на час життя сервісу, а для кожного одержувача потрібен окремий клас з інтерфейсом.
2. **`Action<string>? onSkipped`** у `ProductCsvImporter.Load` і `ProductJsonImporter.Load` — викликається для кожного
   пропущеного рядка. З `--report` Cli передає лямбду, яка друкує `[звіт] пропущено ...` одразу під час імпорту;
   без `--report` поведінка лаб. 3 не змінилась.

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