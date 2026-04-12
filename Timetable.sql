-- PONDĚLÍ (13. 4. 2026)
-- C# II (9:00 - 10:30, 90 min) -> EB213 (RoomId 1)
INSERT INTO Reservation (RoomId, OrganizerId, StartTime, EndTime, Purpose, PersonCount, Status) 
VALUES (1, 1, '2026-04-13 09:00:00', '2026-04-13 10:30:00', 'C# II', 15, 0);

-- SKJ (10:45 - 12:15, 90 min) -> EB213 (RoomId 1)
INSERT INTO Reservation (RoomId, OrganizerId, StartTime, EndTime, Purpose, PersonCount, Status) 
VALUES (1, 1, '2026-04-13 10:45:00', '2026-04-13 12:15:00', 'SKJ', 15, 0);

-- C++ I (14:15 - 16:45, 150 min) -> EB215 (RoomId 3) - Kvůli limitu 150 minut!
INSERT INTO Reservation (RoomId, OrganizerId, StartTime, EndTime, Purpose, PersonCount, Status) 
VALUES (3, 1, '2026-04-13 14:15:00', '2026-04-13 16:45:00', 'C++ I', 15, 0);


-- ÚTERÝ (14. 4. 2026)
-- DS II (9:00 - 10:30, 90 min) -> EB213 (RoomId 1)
INSERT INTO Reservation (RoomId, OrganizerId, StartTime, EndTime, Purpose, PersonCount, Status) 
VALUES (1, 1, '2026-04-14 09:00:00', '2026-04-14 10:30:00', 'DS II', 15, 0);

-- A/IV-FEI (12:30 - 14:00, 90 min) -> EB213 (RoomId 1)
INSERT INTO Reservation (RoomId, OrganizerId, StartTime, EndTime, Purpose, PersonCount, Status) 
VALUES (1, 1, '2026-04-14 12:30:00', '2026-04-14 14:00:00', 'A/IV-FEI', 15, 0);


-- STŘEDA (15. 4. 2026)
-- UTI (9:00 - 11:30, 150 min) -> EB215 (RoomId 3) - Kvůli limitu 150 minut!
INSERT INTO Reservation (RoomId, OrganizerId, StartTime, EndTime, Purpose, PersonCount, Status) 
VALUES (3, 1, '2026-04-15 09:00:00', '2026-04-15 11:30:00', 'UTI', 15, 0);

-- URO (16:00 - 17:30, 90 min) -> EB213 (RoomId 1)
INSERT INTO Reservation (RoomId, OrganizerId, StartTime, EndTime, Purpose, PersonCount, Status) 
VALUES (1, 1, '2026-04-15 16:00:00', '2026-04-15 17:30:00', 'URO', 15, 0);