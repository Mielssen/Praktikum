-- Запускати ТІЛЬКИ на ЛОКАЛЬНІЙ БД (MySQL 8).
-- Створює 1300 тестових турів PERF_TOUR_xxxx для k6 (1000 під обране + 200 VU + запас).
-- Потрібен хоча б один запис у tour_types.

DROP PROCEDURE IF EXISTS perf_seed_tours;

DELIMITER //
CREATE PROCEDURE perf_seed_tours(IN total INT)
BEGIN
  DECLARE i INT DEFAULT 1;
  DECLARE t INT;
  SELECT MIN(type_id) INTO t FROM tour_types;

  WHILE i <= total DO
    INSERT INTO tours (name, description, price, duration_days, type_id, available_from, available_to)
    VALUES (
      CONCAT('PERF_TOUR_', LPAD(i, 4, '0')),
      'Performance test tour',
      1000 + i,
      3 + (i % 10),
      t,
      CURDATE(),
      DATE_ADD(CURDATE(), INTERVAL 365 DAY)
    );
    SET i = i + 1;
  END WHILE;
END //
DELIMITER ;

CALL perf_seed_tours(1300);
DROP PROCEDURE perf_seed_tours;

-- Перевірка: total має бути 1300 і last_tour_id - first_tour_id + 1 = 1300 (id підряд).
-- first_tour_id передай у k6 як FIRST_TOUR_ID.
SELECT MIN(tour_id) AS first_tour_id, MAX(tour_id) AS last_tour_id, COUNT(*) AS total
FROM tours WHERE name LIKE 'PERF_TOUR_%';
