CREATE TABLE [dbo].[Orders] (
    [Id]           UNIQUEIDENTIFIER DEFAULT (newid()) NOT NULL,
    [UserId]       UNIQUEIDENTIFIER NOT NULL,
    [UmbrellaId]   UNIQUEIDENTIFIER NOT NULL,
    [OrderDate]    DATETIME         NOT NULL,
    [OrderStatus]  NVARCHAR (15)    NOT NULL,
    [CreationDate] DATETIME         NOT NULL,
    [UpdatedDate]  DATETIME         NULL,
    [IsDeleted]    BIT              NOT NULL,
    [DeletedDate]  DATETIME         NULL,
    PRIMARY KEY CLUSTERED ([Id] ASC),
    FOREIGN KEY ([UmbrellaId]) REFERENCES [dbo].[Umbrellas] ([Id]),
    FOREIGN KEY ([UserId]) REFERENCES [dbo].[AspNetUsers] ([Id])
);

