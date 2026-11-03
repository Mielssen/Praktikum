# Performance Testing: зона Favorite (api/favorites)

Що тестуємо: `FavoriteService` (BenchmarkDotNet) і ендпоінти `GET/POST/DELETE /api/favorites` (k6).

## 0. Що розпакувати куди

```
Praktikum542.Benchmarks/   -> замінити файли в існуючому проєкті (csproj, Program.cs, FavoriteServiceBenchmarks.cs)
performance/               -> покласти в корінь рішення, поруч із Praktikum542.sln
```

## 1. Одна зміна в Program.cs (обов'язково для k6)

Глобальний rate limiter пропускає 100 запитів/хв на IP, тож k6 одразу отримає `429`.
Зроби ліміт налаштовуваним (за замовчуванням лишається 100, поведінка не змінюється).

Перед `builder.Services.AddRateLimiter(...)` додай:

```csharp
var permitLimit = builder.Configuration.GetValue<int?>("RateLimiting:PermitLimit") ?? 100;
```

і заміни `PermitLimit = 100,` на `PermitLimit = permitLimit,`.

## 2. BenchmarkDotNet

```powershell
dotnet run -c Release --project Praktikum542.Benchmarks
```

- Обов'язково Release. Запускай без дебагера.
- Бенчмарки: `GetMy`, `Add + Delete`, `Add дубліката (AppException)` при `FavoritesCount` = 10 / 100 / 1000.
- Результати: `BenchmarkDotNet.Artifacts/results/*.md` та `*.csv`.
- Це InMemory-БД: міряється код сервісу, репозиторію й EF без мережі та реального MySQL.
  Реальну БД покриває k6.
- Якщо BenchmarkDotNet не збирає окремий проєкт: додай `[InProcess]` над класом
  `FavoriteServiceBenchmarks` (`using BenchmarkDotNet.Jobs;`/`BenchmarkDotNet.Attributes`).

## 3. Підготовка БД для k6 (локальна БД!)

1. Виконай `performance/sql/perf_seed.sql` у MySQL Workbench. Створить 1300 турів `PERF_TOUR_xxxx`.
2. Запиши `first_tour_id` з останнього SELECT. Це `-FirstTourId`.
3. Потрібен тестовий користувач: зареєструй через Swagger (`POST /api/auth/register`) або візьми існуючого.

## 4. Запуск API без rate limit

```powershell
$env:RateLimiting__PermitLimit = "1000000"
dotnet run -c Release --launch-profile http --project Praktikum542
```

Профіль `http` важливий: порт 5265 без редиректу на HTTPS.
Логування Serilog і `RequestLoggingMiddleware` лишаються ввімкненими, тож вони входять у виміряний час.

## 5. k6

Встанови k6 (`winget install k6`), потім з папки `performance`:

```powershell
cd performance
.\run_k6.ps1 -Email "test@test.com" -Password "ТВІЙ_ПАРОЛЬ" -FirstTourId 101
```

Це 9 запусків (small/medium/large × 10/100/1000 обраних), приблизно 12 хвилин.
Один запуск: `-FavCounts 100 -Loads medium`.

Профілі: small = 5 VU, medium = 50 VU, large = 200 VU (10 с розгін, 40 с утримання, 10 с спад).
Поріг: p95 < 500 мс і помилок < 1%.
Результат: `performance/results/k6_report.md` та окремі `k6_*.json`.

## 6. Прибирання

Після тестів виконай `performance/sql/perf_cleanup.sql`.

## 7. Звіт

Заповни `PERFORMANCE_REPORT_TEMPLATE.md` вставками з `BenchmarkDotNet.Artifacts/results/*.md`
і `performance/results/k6_report.md`, плюс скріни.
