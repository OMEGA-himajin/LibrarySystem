USE master;
GO

IF DB_ID(N'LibrarySystem') IS NULL
BEGIN
    CREATE DATABASE LibrarySystem;
END
GO

USE LibrarySystem;
GO

IF OBJECT_ID(N'dbo.BookStatus', N'U') IS NOT NULL DROP TABLE dbo.BookStatus;
IF OBJECT_ID(N'dbo.Reservations', N'U') IS NOT NULL DROP TABLE dbo.Reservations;
IF OBJECT_ID(N'dbo.Lendings', N'U') IS NOT NULL DROP TABLE dbo.Lendings;
IF OBJECT_ID(N'dbo.Books', N'U') IS NOT NULL DROP TABLE dbo.Books;
IF OBJECT_ID(N'dbo.Users', N'U') IS NOT NULL DROP TABLE dbo.Users;
IF OBJECT_ID(N'dbo.Librarians', N'U') IS NOT NULL DROP TABLE dbo.Librarians;
GO

CREATE TABLE dbo.Librarians
(
    LibrarianId INT IDENTITY(1,1) NOT NULL,
    LibrarianCode NVARCHAR(50) NOT NULL,
    FullName NVARCHAR(100) NOT NULL,
    PasswordHash NVARCHAR(200) NOT NULL,
    PasswordSalt NVARCHAR(100) NOT NULL,
    IsActive BIT NOT NULL CONSTRAINT DF_Librarians_IsActive DEFAULT (1),
    CONSTRAINT PK_Librarians PRIMARY KEY CLUSTERED (LibrarianId),
    CONSTRAINT UQ_Librarians_LibrarianCode UNIQUE (LibrarianCode)
);
GO

CREATE TABLE dbo.Users
(
    UserId INT IDENTITY(1,1) NOT NULL,
    UserCode NVARCHAR(50) NOT NULL,
    FullName NVARCHAR(100) NOT NULL,
    BirthDate DATE NOT NULL,
    Gender TINYINT NOT NULL CONSTRAINT DF_Users_Gender DEFAULT (0),
    PhoneNumber NVARCHAR(30) NULL,
    Email NVARCHAR(254) NULL,
    IsActive BIT NOT NULL CONSTRAINT DF_Users_IsActive DEFAULT (1),
    CONSTRAINT PK_Users PRIMARY KEY CLUSTERED (UserId),
    CONSTRAINT UQ_Users_UserCode UNIQUE (UserCode),
    CONSTRAINT CK_Users_Gender CHECK (Gender IN (0, 1, 2))
);
GO

CREATE TABLE dbo.Books
(
    BookId BIGINT IDENTITY(1,1) NOT NULL,
    ISBN NVARCHAR(20) NULL,
    Title NVARCHAR(200) NOT NULL,
    Author NVARCHAR(200) NOT NULL,
    Publisher NVARCHAR(200) NULL,
    PublishedYear SMALLINT NULL,
    Genre NVARCHAR(100) NULL,
    Description NVARCHAR(1000) NULL,
    IsDeleted BIT NOT NULL CONSTRAINT DF_Books_IsDeleted DEFAULT (0),
    CONSTRAINT PK_Books PRIMARY KEY CLUSTERED (BookId),
    CONSTRAINT CK_Books_PublishedYear CHECK (PublishedYear IS NULL OR PublishedYear BETWEEN 0 AND 9999)
);
GO

CREATE TABLE dbo.Lendings
(
    LendingId BIGINT IDENTITY(1,1) NOT NULL,
    BookId BIGINT NOT NULL,
    UserId INT NOT NULL,
    LibrarianId INT NOT NULL,
    LentAt DATETIME2(0) NOT NULL CONSTRAINT DF_Lendings_LentAt DEFAULT (SYSDATETIME()),
    DueDate DATETIME2(0) NOT NULL,
    ReturnedAt DATETIME2(0) NULL,
    ReturnLibrarianId INT NULL,
    CONSTRAINT PK_Lendings PRIMARY KEY CLUSTERED (LendingId),
    CONSTRAINT FK_Lendings_Books FOREIGN KEY (BookId) REFERENCES dbo.Books (BookId),
    CONSTRAINT FK_Lendings_Users FOREIGN KEY (UserId) REFERENCES dbo.Users (UserId),
    CONSTRAINT FK_Lendings_Librarians FOREIGN KEY (LibrarianId) REFERENCES dbo.Librarians (LibrarianId),
    CONSTRAINT FK_Lendings_ReturnLibrarians FOREIGN KEY (ReturnLibrarianId) REFERENCES dbo.Librarians (LibrarianId),
    CONSTRAINT CK_Lendings_Dates CHECK (DueDate >= LentAt),
    CONSTRAINT CK_Lendings_ReturnedAt CHECK (ReturnedAt IS NULL OR ReturnedAt >= LentAt)
);
GO

CREATE TABLE dbo.Reservations
(
    ReservationId BIGINT IDENTITY(1,1) NOT NULL,
    BookId BIGINT NOT NULL,
    UserId INT NOT NULL,
    LibrarianId INT NOT NULL,
    ReservedAt DATETIME2(0) NOT NULL CONSTRAINT DF_Reservations_ReservedAt DEFAULT (SYSDATETIME()),
    Status TINYINT NOT NULL CONSTRAINT DF_Reservations_Status DEFAULT (0),
    NotifiedAt DATETIME2(0) NULL,
    ExpiresAt DATETIME2(0) NULL,
    CONSTRAINT PK_Reservations PRIMARY KEY CLUSTERED (ReservationId),
    CONSTRAINT FK_Reservations_Books FOREIGN KEY (BookId) REFERENCES dbo.Books (BookId),
    CONSTRAINT FK_Reservations_Users FOREIGN KEY (UserId) REFERENCES dbo.Users (UserId),
    CONSTRAINT FK_Reservations_Librarians FOREIGN KEY (LibrarianId) REFERENCES dbo.Librarians (LibrarianId),
    CONSTRAINT CK_Reservations_Status CHECK (Status IN (0, 1, 2, 3)),
    CONSTRAINT CK_Reservations_ExpiresAt CHECK (ExpiresAt IS NULL OR ExpiresAt >= ReservedAt)
);
GO

CREATE TABLE dbo.BookStatus
(
    BookId BIGINT NOT NULL,
    Status INT NOT NULL CONSTRAINT DF_BookStatus_Status DEFAULT (0),
    CurrentLendingId BIGINT NULL,
    CONSTRAINT PK_BookStatus PRIMARY KEY CLUSTERED (BookId),
    CONSTRAINT FK_BookStatus_Books FOREIGN KEY (BookId) REFERENCES dbo.Books (BookId),
    CONSTRAINT FK_BookStatus_Lendings FOREIGN KEY (CurrentLendingId) REFERENCES dbo.Lendings (LendingId),
    CONSTRAINT CK_BookStatus_Status CHECK (Status IN (0, 1, 2, 3))
);
GO

CREATE INDEX IX_Books_Title ON dbo.Books (Title);
CREATE INDEX IX_Books_Author ON dbo.Books (Author);
CREATE INDEX IX_Books_ISBN ON dbo.Books (ISBN);
CREATE INDEX IX_Lendings_UserId_ReturnedAt ON dbo.Lendings (UserId, ReturnedAt);
CREATE INDEX IX_Lendings_BookId_ReturnedAt_LentAt ON dbo.Lendings (BookId, ReturnedAt, LentAt DESC);
CREATE INDEX IX_Reservations_BookId_Status_ReservedAt ON dbo.Reservations (BookId, Status, ReservedAt);
CREATE INDEX IX_Reservations_UserId_Status ON dbo.Reservations (UserId, Status);
GO

CREATE UNIQUE INDEX UX_Lendings_ActiveBook
ON dbo.Lendings (BookId)
WHERE ReturnedAt IS NULL;
GO

CREATE UNIQUE INDEX UX_Reservations_ActiveBookUser
ON dbo.Reservations (BookId, UserId)
WHERE Status = 0;
GO

INSERT INTO dbo.Librarians
(
    LibrarianCode,
    FullName,
    PasswordHash,
    PasswordSalt,
    IsActive
)
VALUES
(
    N'admin',
    N'管理者',
    N'FgkIcMRVc6urv4mHuikMFZDZxDLqDDITGHsZ/A6IxHA=',
    N'j0VbbXFvRhs/Y75D70XOUg==',
    1
);
GO

INSERT INTO dbo.Users
(
    UserCode,
    FullName,
    BirthDate,
    Gender,
    PhoneNumber,
    Email,
    IsActive
)
VALUES
(
    N'U0001',
    N'山田 太郎',
    '2000-01-01',
    1,
    N'090-0000-0000',
    N'taro@example.com',
    1
);
GO

INSERT INTO dbo.Books
(
    ISBN,
    Title,
    Author,
    Publisher,
    PublishedYear,
    Genre,
    Description,
    IsDeleted
)
VALUES
(
    N'9784000000001',
    N'サンプル蔵書',
    N'テスト著者',
    N'サンプル出版社',
    2024,
    N'総記',
    N'動作確認用の初期データです。',
    0
);
GO

INSERT INTO dbo.BookStatus
(
    BookId,
    Status,
    CurrentLendingId
)
SELECT
    b.BookId,
    0,
    NULL
FROM dbo.Books AS b;
GO

PRINT N'LibrarySystem database setup completed.';
PRINT N'Initial login: librarianCode=admin / password=admin123';
GO
