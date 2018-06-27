INSERT INTO Entities VALUES (NEWID(), N'Читатель'), (NEWID(), N'Заказ'), (NEWID(), N'Книга'), (NEWID(), N'Каталог'), (NEWID(), N'Жанр'), (NEWID(), N'Чёрный список'), (NEWID(), N'Пользователь')
INSERT INTO Users VALUES (NEWID(), N'Ильин Кирилл Вадимович', N'Supervisor', N'Supervisor', N'Supervisor', NULL, NULL)


select * from Entities
select * from EntityRecords
select * from EntityHistories
select * from Users
select * from Readers
select * from BlackLists
select * from Orders
select * from Catalogs
select * from Books
select * from Genres


delete from EntityHistories
delete from EntityRecords
delete from Readers
delete from Catalogs
delete from Genres
delete from Books