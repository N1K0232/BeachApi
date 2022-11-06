CREATE TABLE [dbo].[Reviews] (
    [Id]           UNIQUEIDENTIFIER DEFAULT (newid()) NOT NULL,
    [UserId]       UNIQUEIDENTIFIER NOT NULL,
    [Value]        INT              NOT NULL,
    [Title]        NVARCHAR (100)   NULL,
    [Text]         NVARCHAR (4000)  NULL,
    [CreationDate] DATETIME         NOT NULL,
    [UpdatedDate]  DATETIME         NULL,
    PRIMARY KEY CLUSTERED ([Id] ASC),
    FOREIGN KEY ([UserId]) REFERENCES [dbo].[AspNetUsers] ([Id])
);

