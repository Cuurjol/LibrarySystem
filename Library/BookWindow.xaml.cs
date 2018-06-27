using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;

namespace Library
{
    /// <summary>
    /// Логика взаимодействия для BookWindow.xaml
    /// </summary>
    public partial class BookWindow
    {
        private readonly User _currentUser; // Текущий пользователь системы
        private readonly bool _isAddMode; // Булевское поле для проверки режима операции над записью
        private readonly Book _selectedBook; // Выбранная книга в BookDataGrid для редактирования

        public BookWindow(bool isAddMode, Book book, User user)
        {
            InitializeComponent();
            _isAddMode = isAddMode;
            _selectedBook = book;
            _currentUser = user;

            using (LibraryModelContainer dbContainer = new LibraryModelContainer())
            {
                BookCatalogComboBox.ItemsSource = dbContainer.Catalogs.ToList();
                BookGenreComboBox.ItemsSource = dbContainer.Genres.ToList();

                if (!_isAddMode) // Режим редактирования существующей книги
                {
                    AddUpdateButton.Content = "Редактировать";

                    book = dbContainer.Books.Find(book.Id);

                    if (book != null)
                    {
                        BookNameTextBox.Text = book.Name;
                        BookGenreComboBox.SelectedItem = book.Genre;
                        BookAuthorTextBox.Text = book.Author;
                        BookYearMaskedTextBox.Text = book.Year.ToString();
                        BookCatalogComboBox.SelectedItem = book.Catalog;
                    }
                }
            }
        }

        private void CreateEntityHistory(string fieldName, string oldValue, string newValue, EntityRecord record) // Метод создания историй изменений записи книги для таблицы EntityHistory
        {
            using (LibraryModelContainer dbContainer = new LibraryModelContainer())
            {
                User user = dbContainer.Users.Find(_currentUser.Id);
                EntityRecord findRecord = dbContainer.EntityRecords.Find(record.Id);

                EntityHistory history = new EntityHistory
                {
                    Id = Guid.NewGuid(),
                    FieldName = fieldName,
                    OldValue = oldValue,
                    NewValue = newValue,
                    Date = DateTime.Now,
                    User = user,
                    EntityRecord = findRecord
                };
                dbContainer.EntityHistories.Add(history);
                dbContainer.SaveChanges();
            }
        }

        private void AddUpdateRecord_Click(object sender, RoutedEventArgs e)
        {
            if (BookNameTextBox.Text != "" && BookNameTextBox.Text.Length <= 50 && BookGenreComboBox.SelectedItem != null 
                && BookAuthorTextBox.Text != "" && BookAuthorTextBox.Text.Length <= 50 && BookYearMaskedTextBox.IsMaskCompleted) // Если в окне заполнены все поля и их длины <= заявленного значения
            {
                using (LibraryModelContainer dbContainer = new LibraryModelContainer())
                {
                    Entity entity = dbContainer.Entities.First(c => c.Name == "Книга");
                    User user = dbContainer.Users.Find(_currentUser.Id);

                    if (_isAddMode) // Режим добавления записи
                    {
                        Catalog catalog = dbContainer.Catalogs.Find(((Catalog)BookCatalogComboBox.SelectedItem)?.Id);
                        Genre genre = dbContainer.Genres.Find(((Genre)BookGenreComboBox.SelectedItem)?.Id);

                        // Создание книги для таблицы Catalogs
                        Book book = new Book
                        {
                            Id = Guid.NewGuid(),
                            Name = BookAuthorTextBox.Text,
                            Author = BookAuthorTextBox.Text,
                            Year = Convert.ToInt16(BookYearMaskedTextBox.Text),
                            Genre = genre,
                            Catalog = catalog
                        };
                        dbContainer.Books.Add(book);

                        // Создание записи для таблицы EntityRecords
                        EntityRecord record = new EntityRecord
                        {
                            Id = Guid.NewGuid(),
                            Entity = entity,
                            Name = book.Name,
                            State = "Добавлен",
                            CreatedBy = user,
                            ModifiedBy = null,
                            Book = book
                        };
                        dbContainer.EntityRecords.Add(record);

                        // Создание коллекции историй изменений записи книги для таблицы EntityHistories
                        IList<EntityHistory> historyRecordList = new List<EntityHistory>
                        {
                            new EntityHistory
                            {
                                Id = Guid.NewGuid(),
                                FieldName = "Название",
                                OldValue = null,
                                NewValue = book.Name,
                                Date = DateTime.Now,
                                User = user,
                                EntityRecord = record
                            },
                            new EntityHistory
                            {
                                Id = Guid.NewGuid(),
                                FieldName = "Жанр",
                                OldValue = null,
                                NewValue = book.Genre.Name,
                                Date = DateTime.Now,
                                User = user,
                                EntityRecord = record
                            },
                            new EntityHistory
                            {
                                Id = Guid.NewGuid(),
                                FieldName = "Автор",
                                OldValue = null,
                                NewValue = book.Author,
                                Date = DateTime.Now,
                                User = user,
                                EntityRecord = record
                            },
                            new EntityHistory
                            {
                                Id = Guid.NewGuid(),
                                FieldName = "Год издания",
                                OldValue = null,
                                NewValue = book.Year.ToString(),
                                Date = DateTime.Now,
                                User = user,
                                EntityRecord = record
                            },
                            new EntityHistory
                            {
                                Id = Guid.NewGuid(),
                                FieldName = "Каталог",
                                OldValue = null,
                                NewValue = book.Catalog?.Name,
                                Date = DateTime.Now,
                                User = user,
                                EntityRecord = record
                            }
                        };
                        dbContainer.EntityHistories.AddRange(historyRecordList);

                        dbContainer.SaveChanges();
                        MessageBox.Show("Новая книга успешно добавлена в систему.");
                    }
                    else // Режим редактирования записи
                    {
                        Book oldBook = dbContainer.Books.Find(_selectedBook.Id);

                        if (oldBook != null)
                        {
                            // Сохраняем старое значения читателя
                            string name = oldBook.Name;
                            string genre = oldBook.Genre?.Name;
                            string author = oldBook.Author;
                            short year = oldBook.Year;
                            string catalog = oldBook.Catalog?.Name;

                            oldBook.Name = BookNameTextBox.Text;
                            oldBook.Genre = dbContainer.Genres.Find(((Genre)BookGenreComboBox.SelectedItem)?.Id);
                            oldBook.Author = BookAuthorTextBox.Text;
                            oldBook.Year = Convert.ToInt16(BookYearMaskedTextBox.Text);
                            oldBook.Catalog = dbContainer.Catalogs.Find(((Catalog)BookCatalogComboBox.SelectedItem)?.Id);

                            // Если пользователь решил не изменять информацию о выбранной книге в таблице BookDataGrid и нажал кнопку "Редактировать"
                            if (name == oldBook.Name && genre == oldBook.Genre?.Name && author == oldBook.Author
                                && year == oldBook.Year && catalog == oldBook.Catalog?.Name)
                            {
                                MessageBox.Show("Информация о выбранной книге не изменилась.");
                            }
                            else
                            {
                                EntityRecord record = dbContainer.EntityRecords.First(c => c.Book.Id == oldBook.Id); // Находим запись нужной книги для редактирования в таблице EntityRecords

                                // Меняем значения у объекта типа EntityRecord
                                record.ModifiedBy = user;
                                record.State = "Изменён";

                                if (name != oldBook.Name) // Если изменилось название у книги
                                {
                                    record.Name = oldBook.Name;

                                    CreateEntityHistory("Название", name, oldBook.Name, record);
                                }

                                if (genre != oldBook.Genre?.Name) // Если изменился жанр у книги
                                {
                                    CreateEntityHistory("Жанр", genre, oldBook.Genre?.Name, record);
                                }

                                if (author != oldBook.Author) // Если изменился автор у книги
                                {
                                    CreateEntityHistory("Автор", author, oldBook.Author, record);
                                }

                                if (year != oldBook.Year) // Если изменился год издания у книги
                                {
                                    CreateEntityHistory("Год издания", year.ToString(), oldBook.Year.ToString(), record);
                                }

                                if (catalog != oldBook.Catalog?.Name) // Если изменился каталог у книги
                                {
                                    CreateEntityHistory("Каталог", catalog, oldBook.Catalog?.Name, record);
                                }

                                dbContainer.SaveChanges();
                                MessageBox.Show("Информация о выбранной книге успешно изменена.");
                            }
                        }
                    }
                }

                Close();
            }
            else if (BookNameTextBox.Text.Length > 50) // Если поле название имеет длину текста > 50 символов
            {
                MessageBox.Show("Поле название. Длина текста не должна превышать больше 50 символов.");
            }
            else if (BookAuthorTextBox.Text.Length > 50) // Если поле автор имеет длину текста > 50 символов
            {
                MessageBox.Show("Поле автор. Длина текста не должна превышать больше 50 символов.");
            }
            else if (BookNameTextBox.Text == "" && BookGenreComboBox.SelectedItem == null && 
                BookAuthorTextBox.Text == "" && !BookYearMaskedTextBox.IsMaskCompleted) // Если в окне все поля не заполнены 
            {
                MessageBox.Show("Заполните поля для книги: Название, Жанр, Автор, Год издания, Каталог.");
            }
            else // Проверяем на пустоту заполнения некоторых полей в окне
            {
                if (BookNameTextBox.Text == "")
                {
                    MessageBox.Show("Заполните поле для книги: Название.");
                }
                else if (BookGenreComboBox.SelectedItem == null)
                {
                    MessageBox.Show("Заполните поле для книги: Жанр.");
                }
                else if (BookAuthorTextBox.Text == "")
                {
                    MessageBox.Show("Заполните поле для книги: Автор.");
                }
                else if (!BookYearMaskedTextBox.IsMaskCompleted)
                {
                    MessageBox.Show("Заполните поле для книги: Год издания.");
                }
                else if (BookCatalogComboBox.SelectedItem == null)
                {
                    MessageBox.Show("Заполните поле для книги: Каталог.");
                }
            }
        }

        private void Close_Click(object sender, RoutedEventArgs e) // Закрытие окна
        {
            Close();
        }

        private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e) // Метод перетаскивания окна за любую область
        {
            DragMove();
        }

        private void BookCatalogComboboxTextChanged(object sender, TextChangedEventArgs e) // Живой поиск в ComboBox каталогов
        {
            BookCatalogComboBox.IsDropDownOpen = true;
            var tb = (TextBox)e.OriginalSource; // убрать selection, если dropdown только открылся
            tb.Select(tb.SelectionStart + tb.SelectionLength, 0);
            CollectionView cv = (CollectionView)CollectionViewSource.GetDefaultView(BookCatalogComboBox.ItemsSource);
            cv.Filter = s => ((Catalog)s).Name.IndexOf(BookCatalogComboBox.Text, StringComparison.CurrentCultureIgnoreCase) >= 0;
        }

        private void BookGenreComboboxTextChanged(object sender, TextChangedEventArgs e) // Живой поиск в ComboBox жанров
        {
            BookGenreComboBox.IsDropDownOpen = true;
            var tb = (TextBox)e.OriginalSource; // убрать selection, если dropdown только открылся
            tb.Select(tb.SelectionStart + tb.SelectionLength, 0);
            CollectionView cv = (CollectionView)CollectionViewSource.GetDefaultView(BookGenreComboBox.ItemsSource);
            cv.Filter = s => ((Genre)s).Name.IndexOf(BookGenreComboBox.Text, StringComparison.CurrentCultureIgnoreCase) >= 0;
        }
    }
}
