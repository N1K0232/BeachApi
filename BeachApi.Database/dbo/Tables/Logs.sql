CREATE TABLE [dbo].[Logs]
(
    [Id]        INT            IDENTITY (1, 1) NOT NULL,
    [Message]   NVARCHAR (4000) NULL,
    [Level]     VARCHAR (50)   NULL,
    [TimeStamp] NVARCHAR (4000) NULL,
    [Exception] NVARCHAR (4000) NULL,

    PRIMARY KEY CLUSTERED ([Id] ASC)
);