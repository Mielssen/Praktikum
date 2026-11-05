CREATE INDEX idx_credentials_role_status ON credentials(role, status);
CREATE INDEX idx_credentials_created_at ON credentials(created_at);
CREATE INDEX idx_user_details_name ON user_details(name);
CREATE INDEX idx_bookings_status_booking_date ON bookings(status, booking_date);
CREATE INDEX idx_bookings_start_date ON bookings(start_date);
CREATE INDEX idx_bookings_tour_total ON bookings(tour_id, total_price);

EXPLAIN SELECT * FROM credentials 
WHERE role = 'client' AND status = 'active' 
ORDER BY created_at;

EXPLAIN SELECT tour_id, COUNT(*), SUM(total_price) 
FROM bookings 
WHERE status != 'cancelled' 
GROUP BY tour_id;