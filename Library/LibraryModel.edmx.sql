
-- --------------------------------------------------
-- Entity Designer DDL Script for SQL Server 2005, 2008, 2012 and Azure
-- --------------------------------------------------
-- Date Created: 03/29/2018 09:11:04
-- Generated from EDMX file: C:\Users\TopCRM\Desktop\0987654\WPF Library\Current project\Library\Library\LibraryModel.edmx
-- --------------------------------------------------

SET QUOTED_IDENTIFIER OFF;
GO
USE [LibrarySystem];
GO
IF SCHEMA_ID(N'dbo') IS NULL EXECUTE(N'CREATE SCHEMA [dbo]');
GO

-- --------------------------------------------------
-- Dropping existing FOREIGN KEY constraints
-- --------------------------------------------------

IF OBJECT_ID(N'[dbo].[FK_ReaderOrder]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[Orders] DROP CONSTRAINT [FK_ReaderOrder];
GO
IF OBJECT_ID(N'[dbo].[FK_CatalogBook]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[Books] DROP CONSTRAINT [FK_CatalogBook];
GO
IF OBJECT_ID(N'[dbo].[FK_BooksOrders_Book]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[BooksOrders] DROP CONSTRAINT [FK_BooksOrders_Book];
GO
IF OBJECT_ID(N'[dbo].[FK_BooksOrders_Order]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[BooksOrders] DROP CONSTRAINT [FK_BooksOrders_Order];
GO
IF OBJECT_ID(N'[dbo].[FK_GenreBook]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[Books] DROP CONSTRAINT [FK_GenreBook];
GO
IF OBJECT_ID(N'[dbo].[FK_EntityEntityRecord]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[EntityRecords] DROP CONSTRAINT [FK_EntityEntityRecord];
GO
IF OBJECT_ID(N'[dbo].[FK_EntityRecordEntityHistory]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[EntityHistories] DROP CONSTRAINT [FK_EntityRecordEntityHistory];
GO
IF OBJECT_ID(N'[dbo].[FK_UserEntityHistory]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[EntityHistories] DROP CONSTRAINT [FK_UserEntityHistory];
GO
IF OBJECT_ID(N'[dbo].[FK_UserEntityRecord]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[EntityRecords] DROP CONSTRAINT [FK_UserEntityRecord];
GO
IF OBJECT_ID(N'[dbo].[FK_UserEntityRecord1]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[EntityRecords] DROP CONSTRAINT [FK_UserEntityRecord1];
GO
IF OBJECT_ID(N'[dbo].[FK_GenreEntityRecord]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[EntityRecords] DROP CONSTRAINT [FK_GenreEntityRecord];
GO
IF OBJECT_ID(N'[dbo].[FK_CatalogEntityRecord]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[EntityRecords] DROP CONSTRAINT [FK_CatalogEntityRecord];
GO
IF OBJECT_ID(N'[dbo].[FK_BookEntityRecord]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[EntityRecords] DROP CONSTRAINT [FK_BookEntityRecord];
GO
IF OBJECT_ID(N'[dbo].[FK_OrderEntityRecord]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[EntityRecords] DROP CONSTRAINT [FK_OrderEntityRecord];
GO
IF OBJECT_ID(N'[dbo].[FK_ReaderEntityRecord]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[EntityRecords] DROP CONSTRAINT [FK_ReaderEntityRecord];
GO
IF OBJECT_ID(N'[dbo].[FK_BlackListEntityRecord]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[EntityRecords] DROP CONSTRAINT [FK_BlackListEntityRecord];
GO
IF OBJECT_ID(N'[dbo].[FK_ReaderBlackList]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[BlackLists] DROP CONSTRAINT [FK_ReaderBlackList];
GO

-- --------------------------------------------------
-- Dropping existing tables
-- --------------------------------------------------

IF OBJECT_ID(N'[dbo].[Books]', 'U') IS NOT NULL
    DROP TABLE [dbo].[Books];
GO
IF OBJECT_ID(N'[dbo].[Catalogs]', 'U') IS NOT NULL
    DROP TABLE [dbo].[Catalogs];
GO
IF OBJECT_ID(N'[dbo].[Orders]', 'U') IS NOT NULL
    DROP TABLE [dbo].[Orders];
GO
IF OBJECT_ID(N'[dbo].[Readers]', 'U') IS NOT NULL
    DROP TABLE [dbo].[Readers];
GO
IF OBJECT_ID(N'[dbo].[Users]', 'U') IS NOT NULL
    DROP TABLE [dbo].[Users];
GO
IF OBJECT_ID(N'[dbo].[Genres]', 'U') IS NOT NULL
    DROP TABLE [dbo].[Genres];
GO
IF OBJECT_ID(N'[dbo].[BlackLists]', 'U') IS NOT NULL
    DROP TABLE [dbo].[BlackLists];
GO
IF OBJECT_ID(N'[dbo].[Entities]', 'U') IS NOT NULL
    DROP TABLE [dbo].[Entities];
GO
IF OBJECT_ID(N'[dbo].[EntityRecords]', 'U') IS NOT NULL
    DROP TABLE [dbo].[EntityRecords];
GO
IF OBJECT_ID(N'[dbo].[EntityHistories]', 'U') IS NOT NULL
    DROP TABLE [dbo].[EntityHistories];
GO
IF OBJECT_ID(N'[dbo].[BooksOrders]', 'U') IS NOT NULL
    DROP TABLE [dbo].[BooksOrders];
GO

-- --------------------------------------------------
-- Creating all tables
-- --------------------------------------------------

-- Creating table 'Books'
CREATE TABLE [dbo].[Books] (
    [Id] uniqueidentifier  NOT NULL,
    [Name] nvarchar(50)  NOT NULL,
    [Author] nvarchar(50)  NOT NULL,
    [Year] smallint  NOT NULL,
    [Catalog_Id] uniqueidentifier  NULL,
    [Genre_Id] uniqueidentifier  NOT NULL
);
GO

-- Creating table 'Catalogs'
CREATE TABLE [dbo].[Catalogs] (
    [Id] uniqueidentifier  NOT NULL,
    [Name] nvarchar(50)  NOT NULL,
    [Description] nvarchar(150)  NOT NULL
);
GO

-- Creating table 'Orders'
CREATE TABLE [dbo].[Orders] (
    [Id] uniqueidentifier  NOT NULL,
    [Number] smallint IDENTITY(1,1) NOT NULL,
    [RegisteredOn] datetime  NOT NULL,
    [DeadlineDate] datetime  NOT NULL,
    [ClosureDate] datetime  NULL,
    [Reader_Id] uniqueidentifier  NOT NULL
);
GO

-- Creating table 'Readers'
CREATE TABLE [dbo].[Readers] (
    [Id] uniqueidentifier  NOT NULL,
    [Name] nvarchar(50)  NOT NULL,
    [Status] nvarchar(20)  NOT NULL,
    [Blocked] bit  NOT NULL,
    [Removed] bit  NOT NULL
);
GO

-- Creating table 'Users'
CREATE TABLE [dbo].[Users] (
    [Id] uniqueidentifier  NOT NULL,
    [Name] nvarchar(50)  NOT NULL,
    [UserRole] nvarchar(30)  NOT NULL,
    [Login] nvarchar(30)  NOT NULL,
    [Password] nvarchar(30)  NOT NULL,
    [TimeInSystem] datetime  NULL,
    [TimeOutSystem] datetime  NULL
);
GO

-- Creating table 'Genres'
CREATE TABLE [dbo].[Genres] (
    [Id] uniqueidentifier  NOT NULL,
    [Name] nvarchar(50)  NOT NULL
);
GO

-- Creating table 'BlackLists'
CREATE TABLE [dbo].[BlackLists] (
    [Id] uniqueidentifier  NOT NULL,
    [Name] nvarchar(50)  NOT NULL,
    [Date] datetime  NOT NULL,
    [Reader_Id] uniqueidentifier  NOT NULL
);
GO

-- Creating table 'Entities'
CREATE TABLE [dbo].[Entities] (
    [Id] uniqueidentifier  NOT NULL,
    [Name] nvarchar(50)  NOT NULL
);
GO

-- Creating table 'EntityRecords'
CREATE TABLE [dbo].[EntityRecords] (
    [Id] uniqueidentifier  NOT NULL,
    [Name] nvarchar(50)  NOT NULL,
    [State] nvarchar(20)  NOT NULL,
    [Entity_Id] uniqueidentifier  NOT NULL,
    [CreatedBy_Id] uniqueidentifier  NULL,
    [ModifiedBy_Id] uniqueidentifier  NULL,
    [Genre_Id] uniqueidentifier  NULL,
    [Catalog_Id] uniqueidentifier  NULL,
    [Book_Id] uniqueidentifier  NULL,
    [Order_Id] uniqueidentifier  NULL,
    [Reader_Id] uniqueidentifier  NULL,
    [BlackList_Id] uniqueidentifier  NULL
);
GO

-- Creating table 'EntityHistories'
CREATE TABLE [dbo].[EntityHistories] (
    [Id] uniqueidentifier  NOT NULL,
    [FieldName] nvarchar(50)  NOT NULL,
    [OldValue] nvarchar(150)  NULL,
    [NewValue] nvarchar(150)  NULL,
    [Date] datetime  NOT NULL,
    [EntityRecord_Id] uniqueidentifier  NOT NULL,
    [User_Id] uniqueidentifier  NOT NULL
);
GO

-- Creating table 'BooksOrders'
CREATE TABLE [dbo].[BooksOrders] (
    [Books_Id] uniqueidentifier  NOT NULL,
    [Orders_Id] uniqueidentifier  NOT NULL
);
GO

-- --------------------------------------------------
-- Creating all PRIMARY KEY constraints
-- --------------------------------------------------

-- Creating primary key on [Id] in table 'Books'
ALTER TABLE [dbo].[Books]
ADD CONSTRAINT [PK_Books]
    PRIMARY KEY CLUSTERED ([Id] ASC);
GO

-- Creating primary key on [Id] in table 'Catalogs'
ALTER TABLE [dbo].[Catalogs]
ADD CONSTRAINT [PK_Catalogs]
    PRIMARY KEY CLUSTERED ([Id] ASC);
GO

-- Creating primary key on [Id] in table 'Orders'
ALTER TABLE [dbo].[Orders]
ADD CONSTRAINT [PK_Orders]
    PRIMARY KEY CLUSTERED ([Id] ASC);
GO

-- Creating primary key on [Id] in table 'Readers'
ALTER TABLE [dbo].[Readers]
ADD CONSTRAINT [PK_Readers]
    PRIMARY KEY CLUSTERED ([Id] ASC);
GO

-- Creating primary key on [Id] in table 'Users'
ALTER TABLE [dbo].[Users]
ADD CONSTRAINT [PK_Users]
    PRIMARY KEY CLUSTERED ([Id] ASC);
GO

-- Creating primary key on [Id] in table 'Genres'
ALTER TABLE [dbo].[Genres]
ADD CONSTRAINT [PK_Genres]
    PRIMARY KEY CLUSTERED ([Id] ASC);
GO

-- Creating primary key on [Id] in table 'BlackLists'
ALTER TABLE [dbo].[BlackLists]
ADD CONSTRAINT [PK_BlackLists]
    PRIMARY KEY CLUSTERED ([Id] ASC);
GO

-- Creating primary key on [Id] in table 'Entities'
ALTER TABLE [dbo].[Entities]
ADD CONSTRAINT [PK_Entities]
    PRIMARY KEY CLUSTERED ([Id] ASC);
GO

-- Creating primary key on [Id] in table 'EntityRecords'
ALTER TABLE [dbo].[EntityRecords]
ADD CONSTRAINT [PK_EntityRecords]
    PRIMARY KEY CLUSTERED ([Id] ASC);
GO

-- Creating primary key on [Id] in table 'EntityHistories'
ALTER TABLE [dbo].[EntityHistories]
ADD CONSTRAINT [PK_EntityHistories]
    PRIMARY KEY CLUSTERED ([Id] ASC);
GO

-- Creating primary key on [Books_Id], [Orders_Id] in table 'BooksOrders'
ALTER TABLE [dbo].[BooksOrders]
ADD CONSTRAINT [PK_BooksOrders]
    PRIMARY KEY CLUSTERED ([Books_Id], [Orders_Id] ASC);
GO

-- --------------------------------------------------
-- Creating all FOREIGN KEY constraints
-- --------------------------------------------------

-- Creating foreign key on [Reader_Id] in table 'Orders'
ALTER TABLE [dbo].[Orders]
ADD CONSTRAINT [FK_ReaderOrder]
    FOREIGN KEY ([Reader_Id])
    REFERENCES [dbo].[Readers]
        ([Id])
    ON DELETE CASCADE ON UPDATE NO ACTION;
GO

-- Creating non-clustered index for FOREIGN KEY 'FK_ReaderOrder'
CREATE INDEX [IX_FK_ReaderOrder]
ON [dbo].[Orders]
    ([Reader_Id]);
GO

-- Creating foreign key on [Catalog_Id] in table 'Books'
ALTER TABLE [dbo].[Books]
ADD CONSTRAINT [FK_CatalogBook]
    FOREIGN KEY ([Catalog_Id])
    REFERENCES [dbo].[Catalogs]
        ([Id])
    ON DELETE NO ACTION ON UPDATE NO ACTION;
GO

-- Creating non-clustered index for FOREIGN KEY 'FK_CatalogBook'
CREATE INDEX [IX_FK_CatalogBook]
ON [dbo].[Books]
    ([Catalog_Id]);
GO

-- Creating foreign key on [Books_Id] in table 'BooksOrders'
ALTER TABLE [dbo].[BooksOrders]
ADD CONSTRAINT [FK_BooksOrders_Book]
    FOREIGN KEY ([Books_Id])
    REFERENCES [dbo].[Books]
        ([Id])
    ON DELETE NO ACTION ON UPDATE NO ACTION;
GO

-- Creating foreign key on [Orders_Id] in table 'BooksOrders'
ALTER TABLE [dbo].[BooksOrders]
ADD CONSTRAINT [FK_BooksOrders_Order]
    FOREIGN KEY ([Orders_Id])
    REFERENCES [dbo].[Orders]
        ([Id])
    ON DELETE NO ACTION ON UPDATE NO ACTION;
GO

-- Creating non-clustered index for FOREIGN KEY 'FK_BooksOrders_Order'
CREATE INDEX [IX_FK_BooksOrders_Order]
ON [dbo].[BooksOrders]
    ([Orders_Id]);
GO

-- Creating foreign key on [Genre_Id] in table 'Books'
ALTER TABLE [dbo].[Books]
ADD CONSTRAINT [FK_GenreBook]
    FOREIGN KEY ([Genre_Id])
    REFERENCES [dbo].[Genres]
        ([Id])
    ON DELETE CASCADE ON UPDATE NO ACTION;
GO

-- Creating non-clustered index for FOREIGN KEY 'FK_GenreBook'
CREATE INDEX [IX_FK_GenreBook]
ON [dbo].[Books]
    ([Genre_Id]);
GO

-- Creating foreign key on [Entity_Id] in table 'EntityRecords'
ALTER TABLE [dbo].[EntityRecords]
ADD CONSTRAINT [FK_EntityEntityRecord]
    FOREIGN KEY ([Entity_Id])
    REFERENCES [dbo].[Entities]
        ([Id])
    ON DELETE NO ACTION ON UPDATE NO ACTION;
GO

-- Creating non-clustered index for FOREIGN KEY 'FK_EntityEntityRecord'
CREATE INDEX [IX_FK_EntityEntityRecord]
ON [dbo].[EntityRecords]
    ([Entity_Id]);
GO

-- Creating foreign key on [EntityRecord_Id] in table 'EntityHistories'
ALTER TABLE [dbo].[EntityHistories]
ADD CONSTRAINT [FK_EntityRecordEntityHistory]
    FOREIGN KEY ([EntityRecord_Id])
    REFERENCES [dbo].[EntityRecords]
        ([Id])
    ON DELETE CASCADE ON UPDATE NO ACTION;
GO

-- Creating non-clustered index for FOREIGN KEY 'FK_EntityRecordEntityHistory'
CREATE INDEX [IX_FK_EntityRecordEntityHistory]
ON [dbo].[EntityHistories]
    ([EntityRecord_Id]);
GO

-- Creating foreign key on [User_Id] in table 'EntityHistories'
ALTER TABLE [dbo].[EntityHistories]
ADD CONSTRAINT [FK_UserEntityHistory]
    FOREIGN KEY ([User_Id])
    REFERENCES [dbo].[Users]
        ([Id])
    ON DELETE NO ACTION ON UPDATE NO ACTION;
GO

-- Creating non-clustered index for FOREIGN KEY 'FK_UserEntityHistory'
CREATE INDEX [IX_FK_UserEntityHistory]
ON [dbo].[EntityHistories]
    ([User_Id]);
GO

-- Creating foreign key on [CreatedBy_Id] in table 'EntityRecords'
ALTER TABLE [dbo].[EntityRecords]
ADD CONSTRAINT [FK_UserEntityRecord]
    FOREIGN KEY ([CreatedBy_Id])
    REFERENCES [dbo].[Users]
        ([Id])
    ON DELETE NO ACTION ON UPDATE NO ACTION;
GO

-- Creating non-clustered index for FOREIGN KEY 'FK_UserEntityRecord'
CREATE INDEX [IX_FK_UserEntityRecord]
ON [dbo].[EntityRecords]
    ([CreatedBy_Id]);
GO

-- Creating foreign key on [ModifiedBy_Id] in table 'EntityRecords'
ALTER TABLE [dbo].[EntityRecords]
ADD CONSTRAINT [FK_UserEntityRecord1]
    FOREIGN KEY ([ModifiedBy_Id])
    REFERENCES [dbo].[Users]
        ([Id])
    ON DELETE NO ACTION ON UPDATE NO ACTION;
GO

-- Creating non-clustered index for FOREIGN KEY 'FK_UserEntityRecord1'
CREATE INDEX [IX_FK_UserEntityRecord1]
ON [dbo].[EntityRecords]
    ([ModifiedBy_Id]);
GO

-- Creating foreign key on [Genre_Id] in table 'EntityRecords'
ALTER TABLE [dbo].[EntityRecords]
ADD CONSTRAINT [FK_GenreEntityRecord]
    FOREIGN KEY ([Genre_Id])
    REFERENCES [dbo].[Genres]
        ([Id])
    ON DELETE NO ACTION ON UPDATE NO ACTION;
GO

-- Creating non-clustered index for FOREIGN KEY 'FK_GenreEntityRecord'
CREATE INDEX [IX_FK_GenreEntityRecord]
ON [dbo].[EntityRecords]
    ([Genre_Id]);
GO

-- Creating foreign key on [Catalog_Id] in table 'EntityRecords'
ALTER TABLE [dbo].[EntityRecords]
ADD CONSTRAINT [FK_CatalogEntityRecord]
    FOREIGN KEY ([Catalog_Id])
    REFERENCES [dbo].[Catalogs]
        ([Id])
    ON DELETE NO ACTION ON UPDATE NO ACTION;
GO

-- Creating non-clustered index for FOREIGN KEY 'FK_CatalogEntityRecord'
CREATE INDEX [IX_FK_CatalogEntityRecord]
ON [dbo].[EntityRecords]
    ([Catalog_Id]);
GO

-- Creating foreign key on [Book_Id] in table 'EntityRecords'
ALTER TABLE [dbo].[EntityRecords]
ADD CONSTRAINT [FK_BookEntityRecord]
    FOREIGN KEY ([Book_Id])
    REFERENCES [dbo].[Books]
        ([Id])
    ON DELETE NO ACTION ON UPDATE NO ACTION;
GO

-- Creating non-clustered index for FOREIGN KEY 'FK_BookEntityRecord'
CREATE INDEX [IX_FK_BookEntityRecord]
ON [dbo].[EntityRecords]
    ([Book_Id]);
GO

-- Creating foreign key on [Order_Id] in table 'EntityRecords'
ALTER TABLE [dbo].[EntityRecords]
ADD CONSTRAINT [FK_OrderEntityRecord]
    FOREIGN KEY ([Order_Id])
    REFERENCES [dbo].[Orders]
        ([Id])
    ON DELETE NO ACTION ON UPDATE NO ACTION;
GO

-- Creating non-clustered index for FOREIGN KEY 'FK_OrderEntityRecord'
CREATE INDEX [IX_FK_OrderEntityRecord]
ON [dbo].[EntityRecords]
    ([Order_Id]);
GO

-- Creating foreign key on [Reader_Id] in table 'EntityRecords'
ALTER TABLE [dbo].[EntityRecords]
ADD CONSTRAINT [FK_ReaderEntityRecord]
    FOREIGN KEY ([Reader_Id])
    REFERENCES [dbo].[Readers]
        ([Id])
    ON DELETE NO ACTION ON UPDATE NO ACTION;
GO

-- Creating non-clustered index for FOREIGN KEY 'FK_ReaderEntityRecord'
CREATE INDEX [IX_FK_ReaderEntityRecord]
ON [dbo].[EntityRecords]
    ([Reader_Id]);
GO

-- Creating foreign key on [BlackList_Id] in table 'EntityRecords'
ALTER TABLE [dbo].[EntityRecords]
ADD CONSTRAINT [FK_BlackListEntityRecord]
    FOREIGN KEY ([BlackList_Id])
    REFERENCES [dbo].[BlackLists]
        ([Id])
    ON DELETE NO ACTION ON UPDATE NO ACTION;
GO

-- Creating non-clustered index for FOREIGN KEY 'FK_BlackListEntityRecord'
CREATE INDEX [IX_FK_BlackListEntityRecord]
ON [dbo].[EntityRecords]
    ([BlackList_Id]);
GO

-- Creating foreign key on [Reader_Id] in table 'BlackLists'
ALTER TABLE [dbo].[BlackLists]
ADD CONSTRAINT [FK_ReaderBlackList]
    FOREIGN KEY ([Reader_Id])
    REFERENCES [dbo].[Readers]
        ([Id])
    ON DELETE CASCADE ON UPDATE NO ACTION;
GO

-- Creating non-clustered index for FOREIGN KEY 'FK_ReaderBlackList'
CREATE INDEX [IX_FK_ReaderBlackList]
ON [dbo].[BlackLists]
    ([Reader_Id]);
GO

-- --------------------------------------------------
-- Script has ended
-- --------------------------------------------------