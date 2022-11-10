CREATE TABLE [dbo].[BeachChairs] (
    [Id]           UNIQUEIDENTIFIER DEFAULT (newid()) NOT NULL,
    [Color]        NVARCHAR (10)    NOT NULL,
    [PricePerHour] DECIMAL (5, 2)   NOT NULL,
    [IsAvailable]  BIT              NOT NULL,
    [CreationDate] DATETIME         NOT NULL,
    [UpdatedDate]  DATETIME         NULL,
    [IsDeleted]    BIT              NOT NULL,
    [DeletedDate]  DATETIME         NULL,
    PRIMARY KEY CLUSTERED ([Id] ASC)
);

