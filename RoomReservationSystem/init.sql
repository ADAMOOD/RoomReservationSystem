-- 1. TABULKA: User (mapování na IsAdmin jako BIT)
CREATE TABLE [User] (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    Username NVARCHAR(100) NOT NULL,
    PasswordHash NVARCHAR(255) NOT NULL,
    IsAdmin BIT NOT NULL DEFAULT 0 -- V SQL je 'bool' reprezentován jako BIT (0 = false, 1 = true)
);

-- 2. TABULKA: Room (mapování na MaxReservationMinutes)
CREATE TABLE [Room] (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    Name NVARCHAR(50) NOT NULL,
    Capacity INT NOT NULL,
    Equipment NVARCHAR(255),
    MaxReservationMinutes INT NOT NULL -- Přejmenováno přesně podle tvé třídy
);

-- 3. TABULKA: Reservation (mapování na Enum Status jako INT)
CREATE TABLE [Reservation] (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    RoomId INT NOT NULL FOREIGN KEY REFERENCES [Room](Id),
    OrganizerId INT NOT NULL FOREIGN KEY REFERENCES [User](Id),
    StartTime DATETIME2 NOT NULL,
    EndTime DATETIME2 NOT NULL,
    Purpose NVARCHAR(255) NOT NULL,
    PersonCount INT NOT NULL,
    Status INT NOT NULL DEFAULT 0 -- V C# je Active = 0, Cancelled = 1
);

-- 4. TABULKA: ReservationHistory
CREATE TABLE [ReservationHistory] (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    ReservationId INT NOT NULL FOREIGN KEY REFERENCES [Reservation](Id),
    OldStatus INT NULL, -- Povoleno NULL, viz moje poznámka pod kódem!
    NewStatus INT NOT NULL,
    ChangedAt DATETIME2 NOT NULL DEFAULT GETDATE(),
    ChangedByUserId INT NOT NULL FOREIGN KEY REFERENCES [User](Id)
);

-- ==========================================
-- VLOŽENÍ VÝCHOZÍCH DAT (Upravené názvy sloupců)
-- ==========================================

-- Vložení uživatele Adam (IsAdmin místo Role)
INSERT INTO [User] (Username, PasswordHash, IsAdmin) 
VALUES ('Adam', '$2a$11$3owI10rcDBGGdNLowVnsA.PRNfumbirjNlYWO/0aV8.nuDBzz6jkS', 0);

-- Vložení místností (MaxReservationMinutes místo MaxReservationTime)
INSERT INTO [Room] (Name, Capacity, Equipment, MaxReservationMinutes) VALUES 
('EB213', 20, 'Projector, White board', 120),
('EB214', 15, 'Projector', 60),
('EB215', 30, 'PCs, Projector, Smart Board', 180),
('EB310', 8, 'TV, Video Conference System', 60),
('EB311', 12, 'White board, Flipchart', 120),
('Aula', 500, 'Dual Projector, Microphones, Sound System', 240);

-- Vložení testovacích rezervací
INSERT INTO [Reservation] (RoomId, OrganizerId, StartTime, EndTime, Purpose, PersonCount, Status) VALUES 
(1, 1, '2026-04-13 09:00:00', '2026-04-13 10:30:00', 'C# II', 15, 0),
(1, 1, '2026-04-13 10:45:00', '2026-04-13 12:15:00', 'SKJ', 15, 0),
(3, 1, '2026-04-13 14:15:00', '2026-04-13 16:45:00', 'C++ I', 15, 0),
(1, 1, '2026-04-14 09:00:00', '2026-04-14 10:30:00', 'DS II', 15, 0),
(1, 1, '2026-04-14 12:30:00', '2026-04-14 14:00:00', 'A/IV-FEI', 15, 0),
(3, 1, '2026-04-15 09:00:00', '2026-04-15 11:30:00', 'UTI', 15, 0),
(1, 1, '2026-04-15 16:00:00', '2026-04-15 17:30:00', 'URO', 15, 0);

-- Vygenerování prvotní historie
INSERT INTO [ReservationHistory] (ReservationId, OldStatus, NewStatus, ChangedAt, ChangedByUserId)
SELECT Id, NULL, Status, GETDATE(), OrganizerId FROM [Reservation];