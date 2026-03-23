CREATE TABLE Room (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    Name NVARCHAR(100) NOT NULL,
    Capacity INT NOT NULL,
    Equipment NVARCHAR(500),
    MaxReservationMinutes INT NOT NULL
);

CREATE TABLE Reservation (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    RoomId INT NOT NULL FOREIGN KEY REFERENCES Room(Id),
    StartTime DATETIME2 NOT NULL,
    EndTime DATETIME2 NOT NULL,
    Purpose NVARCHAR(255) NOT NULL,
    PersonCount INT NOT NULL,
    OrganizerId INT NOT NULL,
    Status INT NOT NULL -- 0 = Active, 1 = Cancelled 
);

CREATE TABLE ReservationHistory (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    ReservationId INT NOT NULL FOREIGN KEY REFERENCES Reservation(Id),
    OldStatus INT NOT NULL,
    NewStatus INT NOT NULL,
    ChangedAt DATETIME2 NOT NULL,
    ChangedByUserId INT NOT NULL
);