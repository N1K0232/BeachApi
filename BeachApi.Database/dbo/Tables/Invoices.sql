CREATE TABLE [dbo].[Invoices]
(
    [Id]           UNIQUEIDENTIFIER DEFAULT (newid()) NOT NULL,
    [Title]        NVARCHAR (100)   NOT NULL,
    [Description]  NVARCHAR (4000)  NOT NULL,
    [InvoiceDate]  DATETIME         NOT NULL,
    [Price]        DECIMAL (5, 2)   NOT NULL,
    [CreationDate] DATETIME         NOT NULL,
    [UpdatedDate]  DATETIME         NULL,

    PRIMARY KEY CLUSTERED ([Id] ASC)
);