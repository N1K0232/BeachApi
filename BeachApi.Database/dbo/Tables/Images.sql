CREATE TABLE [dbo].[Images] (
    [Id]           UNIQUEIDENTIFIER DEFAULT (newid()) NOT NULL,
    [FileName]     NVARCHAR (256)   NOT NULL,
    [Path]         NVARCHAR (256)   NOT NULL,
    [Length]       BIGINT           NOT NULL,
    [[Content]]]   VARBINARY (MAX)  NOT NULL,
    [Description]  NVARCHAR (512)   NULL,
    [CreationDate] DATETIME         NOT NULL,
    [UpdatedDate]  DATETIME         NULL,
    PRIMARY KEY CLUSTERED ([Id] ASC)
);

