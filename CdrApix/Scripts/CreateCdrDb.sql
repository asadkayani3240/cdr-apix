-- CreateCdrDb.sql

-- 1. Create the database (if it doesn’t exist)
IF DB_ID('CdrDb') IS NULL
BEGIN
    CREATE DATABASE [CdrDb];
END
GO

USE [CdrDb];
GO

-- 2. Create the CdrRecords table
IF OBJECT_ID('dbo.CdrRecords', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.CdrRecords
    (
        Reference   NVARCHAR(50)   NOT NULL PRIMARY KEY,
        CallerId    NVARCHAR(20)   NOT NULL,
        Recipient   NVARCHAR(20)   NOT NULL,
        CallDate    DATETIME2      NOT NULL,
        EndTime     TIME           NOT NULL,
        Duration    INT            NOT NULL,
        Cost        DECIMAL(18,3)  NOT NULL,
        Currency    NCHAR(3)       NOT NULL
    );
END
GO
