CREATE TABLE [dbo].[Products] (
    [Id]           UNIQUEIDENTIFIER DEFAULT (newid()) NOT NULL,
    [SupplierId]   UNIQUEIDENTIFIER NOT NULL,
    [CategoryId]   UNIQUEIDENTIFIER NOT NULL,
    [Name]         NVARCHAR (256)   NOT NULL,
    [Price]        DECIMAL (5, 2)   NOT NULL,
    [CreationDate] DATETIME         NOT NULL,
    [UpdatedDate]  DATETIME         NULL,
    [IsDeleted]    BIT              NOT NULL,
    [DeletedDate]  DATETIME         NULL,
    PRIMARY KEY CLUSTERED ([Id] ASC),
    FOREIGN KEY ([CategoryId]) REFERENCES [dbo].[Categories] ([Id]),
    FOREIGN KEY ([SupplierId]) REFERENCES [dbo].[Suppliers] ([Id])
);

