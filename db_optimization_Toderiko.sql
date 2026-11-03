
USE praktikum;

-- Перевірка наявних індексів
SHOW INDEX FROM bookings;
SHOW INDEX FROM booking_persons;

-- Пошук бронювань користувача
EXPLAIN
SELECT *
FROM bookings
WHERE credential_id = 1;

-- Фільтрація за статусом
EXPLAIN
SELECT *
FROM bookings
WHERE status = 'pending';

-- Отримання туристів бронювання
EXPLAIN
SELECT *
FROM booking_persons
WHERE booking_id = 1;

-- Отримання бронювань із даними туру та користувача
EXPLAIN
SELECT
    b.booking_id,
    b.status,
    b.start_date,
    t.name AS tour_name,
    c.email
FROM bookings b
JOIN tours t ON t.tour_id = b.tour_id
JOIN credentials c ON c.credential_id = b.credential_id
WHERE b.credential_id = 1;
