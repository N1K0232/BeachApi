CREATE TABLE [dbo].[OrderDetails] (
    [OrderId]         UNIQUEIDENTIFIER NOT NULL,
    [ProductId]       UNIQUEIDENTIFIER NOT NULL,
    [OrderedQuantity] INT              NOT NULL,
    [Price]           DECIMAL (5, 2)   NOT NULL,
    PRIMARY KEY CLUSTERED ([OrderId] ASC, [ProductId] ASC),
    FOREIGN KEY ([OrderId]) REFERENCES [dbo].[Orders] ([Id]),
    FOREIGN KEY ([ProductId]) REFERENCES [dbo].[Products] ([Id])
);

