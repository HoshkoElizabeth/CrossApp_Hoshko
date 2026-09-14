# CrossApp_Hoshko

Наскрізний проєкт з крос-платформного програмування.

Предметна область: **Замовлення**.
Сутності: Customer (клієнт), Product (товар), Order (замовлення), OrderLine (рядок замовлення).
Призначення: оформлення замовлень клієнтів та підрахунок сум.

## Запуск

```
dotnet build
dotnet run --project src/Cli
```

Запуск без dotnet run:
```
.\src\Cli\bin\Release\net10.0\win-x64\publish\Cli.exe
```


## Запуск з JSON-виводом 

```
dotnet run --project src/Cli -- --json
```
## Self-contained публікація 

```
dotnet publish src/Cli -c Release -r win-x64 --self-contained true
dotnet publish src/Cli -c Release -r linux-x64 --self-contained true
```

Порівняння розміру публікації під різні RID:

| RID | Розмір publish, МБ |
|---|---|
| win-x64 | 76.84 |
| linux-x64 | 78.80 |


## Середовище

.NET SDK 10.0.401, Windows 10 x64