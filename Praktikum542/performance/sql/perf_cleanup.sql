-- Видаляє тестові тури та їхні записи в обраному після завершення тестів.
DELETE FROM favorites
WHERE tour_id IN (SELECT tour_id FROM tours WHERE name LIKE 'PERF_TOUR_%');

DELETE FROM tours WHERE name LIKE 'PERF_TOUR_%';
