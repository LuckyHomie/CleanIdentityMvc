-- Szkic tabel niestandardowych. Pełny schemat Identity wygeneruje EF Core migrations.
CREATE TABLE UserActivities (
    Id int IDENTITY(1,1) PRIMARY KEY,
    UserId nvarchar(450) NOT NULL,
    Action nvarchar(100) NOT NULL,
    Details nvarchar(max) NULL,
    IpAddress nvarchar(64) NULL,
    UserAgent nvarchar(512) NULL,
    CreatedAt datetimeoffset NOT NULL
);

CREATE TABLE PasswordHistories (
    Id int IDENTITY(1,1) PRIMARY KEY,
    UserId nvarchar(450) NOT NULL,
    PasswordHash nvarchar(max) NOT NULL,
    CreatedAt datetimeoffset NOT NULL
);

CREATE TABLE LoginAuditLogs (
    Id int IDENTITY(1,1) PRIMARY KEY,
    UserId nvarchar(450) NULL,
    Email nvarchar(256) NULL,
    Success bit NOT NULL,
    LockedOut bit NOT NULL,
    FailureReason nvarchar(max) NULL,
    IpAddress nvarchar(64) NULL,
    UserAgent nvarchar(512) NULL,
    SessionId nvarchar(128) NULL,
    CreatedAt datetimeoffset NOT NULL,
    LogoutAt datetimeoffset NULL
);

CREATE TABLE AllowedIpAddresses (
    Id int IDENTITY(1,1) PRIMARY KEY,
    Value nvarchar(64) NOT NULL UNIQUE,
    Enabled bit NOT NULL,
    Description nvarchar(256) NULL,
    CreatedAt datetimeoffset NOT NULL
);
