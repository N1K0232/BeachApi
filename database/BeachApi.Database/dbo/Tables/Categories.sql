CREATE TABLE [dbo].[Categories]
(
	[Id] UNIQUEIDENTIFIER NOT NULL DEFAULT newid(),
    [Name] NVARCHAR(256) NOT NULL,
    [Description] NVARCHAR(512) NULL,
    [CreationDate] DATETIME NOT NULL DEFAULT getutcdate(),
    [LastModifiedDate] DATETIME NULL,
    [TenantId] UNIQUEIDENTIFIER,

    PRIMARY KEY([Id])
)