-- Оптимізація БД для зони "Відгуки" (Pogrebnyak)
CREATE INDEX IX_Reviews_TourId_Rating ON Reviews(TourId, Rating);
CREATE INDEX IX_Reviews_CredentialId ON Reviews(CredentialId);

EXPLAIN SELECT TourId, AVG(Rating) AS AvgRating FROM Reviews WHERE TourId = 1 GROUP BY TourId;