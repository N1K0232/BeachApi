CREATE TABLE [dbo].[Tenants]
(
	[Id] UNIQUEIDENTIFIER NOT NULL DEFAULT newid(),
    [ConnectionString] VARCHAR(4000) NOT NULL,
    [StorageConnectionString] VARCHAR(4000) NULL,
    [ContainerName] VARCHAR(256) NULL,

    PRIMARY KEY([Id])
)