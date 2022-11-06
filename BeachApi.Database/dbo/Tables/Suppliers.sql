CREATE TABLE [dbo].[Suppliers] (
    [Id]           UNIQUEIDENTIFIER NOT NULL,
    [CompanyName]  NVARCHAR (100)   NOT NULL,
    [ContactName]  NVARCHAR (100)   NOT NULL,
    [City]         NVARCHAR (50)    NOT NULL,
    [CreationDate] DATETIME         NOT NULL,
    [UpdatedDate]  DATETIME         NULL,
    PRIMARY KEY CLUSTERED ([Id] ASC)
);

