CREATE TABLE [dbo].[Umbrellas] (
    [Id]           UNIQUEIDENTIFIER DEFAULT (newid()) NOT NULL,
    [Coordinates]  NVARCHAR (10)    NOT NULL,
    [PricePerHour] DECIMAL (5, 2)   NOT NULL,
    [CreationDate] DATETIME         NOT NULL,
    [UpdatedDate]  DATETIME         NULL,
    [IsDeleted]    BIT              NOT NULL,
    [DeletedDate]  DATETIME         NULL,
    PRIMARY KEY CLUSTERED ([Id] ASC)
);

