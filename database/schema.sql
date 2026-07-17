IF DB_ID('MidisSqlAiDb') IS NULL
BEGIN
    CREATE DATABASE MidisSqlAiDb;
END
GO

USE MidisSqlAiDb;
GO

DROP TABLE IF EXISTS TicketComments;
DROP TABLE IF EXISTS Tickets;
DROP TABLE IF EXISTS Services;
DROP TABLE IF EXISTS Employees;
DROP TABLE IF EXISTS Departments;
DROP TABLE IF EXISTS Clients;
GO

CREATE TABLE Clients (
    ClientId INT IDENTITY(1,1) PRIMARY KEY,
    ClientName NVARCHAR(100) NOT NULL,
    Country NVARCHAR(100) NOT NULL,
    Industry NVARCHAR(100) NULL
);

CREATE TABLE Departments (
    DepartmentId INT IDENTITY(1,1) PRIMARY KEY,
    DepartmentName NVARCHAR(100) NOT NULL
);

CREATE TABLE Employees (
    EmployeeId INT IDENTITY(1,1) PRIMARY KEY,
    FullName NVARCHAR(100) NOT NULL,
    Email NVARCHAR(150) NOT NULL,
    DepartmentId INT NOT NULL,

    CONSTRAINT FK_Employees_Departments
        FOREIGN KEY (DepartmentId)
        REFERENCES Departments(DepartmentId)
);

CREATE TABLE Services (
    ServiceId INT IDENTITY(1,1) PRIMARY KEY,
    ServiceName NVARCHAR(100) NOT NULL,
    Description NVARCHAR(255) NULL
);

CREATE TABLE Tickets (
    TicketId INT IDENTITY(1,1) PRIMARY KEY,
    ClientId INT NOT NULL,
    ServiceId INT NOT NULL,
    AssignedEmployeeId INT NULL,
    Title NVARCHAR(150) NOT NULL,
    Status NVARCHAR(50) NOT NULL,
    Priority NVARCHAR(50) NOT NULL,
    CreatedAt DATETIME2 NOT NULL,
    ResolvedAt DATETIME2 NULL,

    CONSTRAINT FK_Tickets_Clients
        FOREIGN KEY (ClientId)
        REFERENCES Clients(ClientId),

    CONSTRAINT FK_Tickets_Services
        FOREIGN KEY (ServiceId)
        REFERENCES Services(ServiceId),

    CONSTRAINT FK_Tickets_Employees
        FOREIGN KEY (AssignedEmployeeId)
        REFERENCES Employees(EmployeeId),

    CONSTRAINT CK_Tickets_Status
        CHECK (Status IN ('Open', 'In Progress', 'Resolved', 'Closed')),

    CONSTRAINT CK_Tickets_Priority
        CHECK (Priority IN ('Low', 'Medium', 'High', 'Critical'))
);

CREATE TABLE TicketComments (
    CommentId INT IDENTITY(1,1) PRIMARY KEY,
    TicketId INT NOT NULL,
    EmployeeId INT NULL,
    CommentText NVARCHAR(500) NOT NULL,
    CreatedAt DATETIME2 NOT NULL,

    CONSTRAINT FK_TicketComments_Tickets
        FOREIGN KEY (TicketId)
        REFERENCES Tickets(TicketId),

    CONSTRAINT FK_TicketComments_Employees
        FOREIGN KEY (EmployeeId)
        REFERENCES Employees(EmployeeId)
);
GO