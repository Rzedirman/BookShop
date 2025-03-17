IF OBJECT_ID(N'[__EFMigrationsHistory]') IS NULL
BEGIN
    CREATE TABLE [__EFMigrationsHistory] (
        [MigrationId] nvarchar(150) NOT NULL,
        [ProductVersion] nvarchar(32) NOT NULL,
        CONSTRAINT [PK___EFMigrationsHistory] PRIMARY KEY ([MigrationId])
    );
END;
GO

BEGIN TRANSACTION;
GO

IF SCHEMA_ID(N'Favorites') IS NULL EXEC(N'CREATE SCHEMA [Favorites];');
GO

CREATE TABLE [Favorites].[Favorites] (
    [UserID] int NOT NULL,
    [ProductID] int NOT NULL,
    [AddedDate] datetime2 NOT NULL DEFAULT ((getdate())),
    CONSTRAINT [PK_Favorites] PRIMARY KEY ([UserID], [ProductID]),
    CONSTRAINT [FK_Favorites_Products] FOREIGN KEY ([ProductID]) REFERENCES [Products].[Products] ([ProductID]),
    CONSTRAINT [FK_Favorites_Users] FOREIGN KEY ([UserID]) REFERENCES [Users].[Users] ([UserID])
);
GO

CREATE INDEX [IX_Favorites_ProductID] ON [Favorites].[Favorites] ([ProductID]);
GO

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20250316165358_AddFavoritesTable', N'6.0.23');
GO

COMMIT;
GO

