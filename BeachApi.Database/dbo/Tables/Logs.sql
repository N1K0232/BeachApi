CREATE TABLE [dbo].[Logs] (
    [Id]        INT            IDENTITY (1, 1) NOT NULL,
    [Message]   NVARCHAR (MAX) NULL,
    [Level]     VARCHAR (50)   NULL,
    [TimeStamp] NVARCHAR (MAX) NULL,
    [Exception] NVARCHAR (MAX) NULL,
    PRIMARY KEY CLUSTERED ([Id] ASC)
);

