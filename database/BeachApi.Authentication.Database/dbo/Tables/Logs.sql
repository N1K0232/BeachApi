CREATE TABLE [dbo].[Logs] (
    [Id]        INT            IDENTITY (1, 1) NOT NULL,
    [Message]   NVARCHAR (MAX) NULL,
    [Level]     NVARCHAR (MAX) NULL,
    [Timestamp] DATETIME       NULL,
    [Exception] NVARCHAR (MAX) NULL,
    [LogEvent]  NVARCHAR (MAX) NULL,
    CONSTRAINT [PK_Logs] PRIMARY KEY CLUSTERED ([Id] ASC)
);

