# Звіт: Performance Testing (зона Favorite)

## 1. Середовище

| Параметр | Значення |
|---|---|
| ОС | |
| CPU / RAM | |
| .NET | 8.0 |
| MySQL | версія, локально |
| Збірка | Release |
| Rate limit на час тесту | вимкнено (PermitLimit = 1000000) |

## 2. Що тестувалось

- Сервіс: `FavoriteService` (`GetMy`, `Add`, `Delete`).
- Ендпоінти: `GET /api/favorites`, `POST /api/favorites`, `DELETE /api/favorites/{tourId}`.

## 3. BenchmarkDotNet (InMemory)

Вставити таблицю з `BenchmarkDotNet.Artifacts/results/*-report-github.md` та скрін консолі.

| Метод | Обране = 10 | Обране = 100 | Обране = 1000 | Alloc (1000) |
|---|---|---|---|---|
| GetMy | | | | |
| Add + Delete | | | | |
| Add дубліката (AppException) | | | | |

## 4. k6 (реальна БД MySQL)

Вставити `performance/results/k6_report.md` та скріни виводу k6.

## 5. Де швидкість падає

- На якому навантаженні (VU) p95 перевищив 500 мс: ...
- На якому розмірі обраного `GET /api/favorites` став повільним: ...
- Чи з'явились помилки (401/429/5xx), на якому навантаженні: ...
- Який ендпоінт деградує першим: ...

## 6. Висновки та гіпотези для Profiling / Оптимізації БД

Заповнюється за фактичними цифрами. Що варто перевірити за даними коду:

- `GET` повертає весь список без пагінації й читає з відстеженням змін (без `AsNoTracking`).
- `Add` робить 3 звернення до БД: `TourExists`, `GetByCredentialAndTour`, вставка.
- Запити фільтрують за `credential_id`: перевірити через EXPLAIN, чи є індекс.
  (в моделі є лише унікальний індекс `uq_favorite (credential_id, tour_id)`).
