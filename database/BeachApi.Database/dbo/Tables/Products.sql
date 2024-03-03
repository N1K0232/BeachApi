CREATE TABLE [dbo].[Products]
(
	[Id] UNIQUEIDENTIFIER NOT NULL DEFAULT newid(),
    [CategoryId] UNIQUEIDENTIFIER NOT NULL,
    [Name] NVARCHAR(256) NOT NULL,
    [Description] NVARCHAR(512) NULL,
    [Quantity] INTEGER NULL,
    [Price] DECIMAL(8, 2) NOT NULL,
    [CreationDate] DATETIME NOT NULL DEFAULT getutcdate(),
    [LastModifiedDate] DATETIME NULL,
    [IsDeleted] BIT NOT NULL DEFAULT (0),
    [DeletedDate] DATETIME NULL,
    [TenantId] UNIQUEIDENTIFIER NOT NULL,

    PRIMARY KEY([Id]),
    FOREIGN KEY([CategoryId]) REFERENCES [dbo].[Categories]([Id])
)