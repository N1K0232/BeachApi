CREATE TABLE [dbo].[Reservations] (
    [Id]              UNIQUEIDENTIFIER DEFAULT (newid()) NOT NULL,
    [UserId]          UNIQUEIDENTIFIER NOT NULL,
    [UmbrellaId]      UNIQUEIDENTIFIER NOT NULL,
    [BeachChairId]    UNIQUEIDENTIFIER NULL,
    [ReservationDate] DATETIME         NOT NULL,
    [From]            DATETIME         NOT NULL,
    [To]              DATETIME         NULL,
    [CreationDate]    DATETIME         NOT NULL,
    [UpdatedDate]     DATETIME         NULL,
    [IsDeleted]       BIT              NOT NULL,
    [DeletedDate]     DATETIME         NULL,
    PRIMARY KEY CLUSTERED ([Id] ASC),
    FOREIGN KEY ([BeachChairId]) REFERENCES [dbo].[BeachChairs] ([Id]),
    FOREIGN KEY ([UmbrellaId]) REFERENCES [dbo].[Umbrellas] ([Id]),
    FOREIGN KEY ([UserId]) REFERENCES [dbo].[AspNetUsers] ([Id])
);

