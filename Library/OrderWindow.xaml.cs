using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;

namespace Library
{
    /// <summary>
    /// Логика взаимодействия для OrderWindow.xaml
    /// </summary>
    public partial class OrderWindow
    {
        private readonly User _currentUser; // Текущий пользователь системы

        public OrderWindow(User user)
        {
            InitializeComponent();

            _currentUser = user;

            using (LibraryModelContainer dbContainer = new LibraryModelContainer())
            {
                ReaderNameComboBox.ItemsSource = dbContainer.Readers.ToList();
                BookDataGrid.ItemsSource = dbContainer.Books.Include(c => c.Catalog).Include(c => c.Genre).ToList();
            }
        }

        private void FiltersRadioButtonChecked(object sender, RoutedEventArgs e) // Переключатели фильтров поиска книг по названию, автору, каталогу или жанру
        {

            if (NameFilterRadioButton.IsChecked == true) // Если выбран переключатель фильтра поиска книг по названию
            {
                if (NameFilterRadioButton.IsLoaded)
                {
                    if (AuthorTextBox.IsEnabled)
                    {
                        AuthorTextBox.Text = "";
                        AuthorTextBox.IsEnabled = false;
                    }

                    if (CatalogTextBox.IsEnabled)
                    {
                        CatalogTextBox.Text = "";
                        CatalogTextBox.IsEnabled = false;
                    }

                    if (GenreTextBox.IsEnabled)
                    {
                        GenreTextBox.Text = "";
                        GenreTextBox.IsEnabled = false;
                    }
                   
                    NameTextBox.IsEnabled = true;
                }
            }
            else if (AuthorFilterRadioButton.IsChecked == true) // Если выбран переключатель фильтра поиска книг по автору
            {
                if (NameTextBox.IsEnabled)
                {
                    NameTextBox.Text = "";
                    NameTextBox.IsEnabled = false;
                }

                if (CatalogTextBox.IsEnabled)
                {
                    CatalogTextBox.Text = "";
                    CatalogTextBox.IsEnabled = false;
                }

                if (GenreTextBox.IsEnabled)
                {
                    GenreTextBox.Text = "";
                    GenreTextBox.IsEnabled = false;
                }

                AuthorTextBox.IsEnabled = true;
            }
            else if (CatalogFilterRadioButton.IsChecked == true) // Если выбран переключатель фильтра поиска книг по каталогу
            {
                if (NameTextBox.IsEnabled)
                {
                    NameTextBox.Text = "";
                    NameTextBox.IsEnabled = false;
                }

                if (AuthorTextBox.IsEnabled)
                {
                    AuthorTextBox.Text = "";
                    AuthorTextBox.IsEnabled = false;
                }

                if (GenreTextBox.IsEnabled)
                {
                    GenreTextBox.Text = "";
                    GenreTextBox.IsEnabled = false;
                }

                CatalogTextBox.IsEnabled = true;
            }
            else if (GenreFilterRadioButton.IsChecked == true) // Если выбран переключатель фильтра поиска книг по жанру
            {
                if (NameTextBox.IsEnabled)
                {
                    NameTextBox.Text = "";
                    NameTextBox.IsEnabled = false;
                }

                if (AuthorTextBox.IsEnabled)
                {
                    AuthorTextBox.Text = "";
                    AuthorTextBox.IsEnabled = false;
                }

                if (CatalogTextBox.IsEnabled)
                {
                    CatalogTextBox.Text = "";
                    CatalogTextBox.IsEnabled = false;
                }

                GenreTextBox.IsEnabled = true;
            }
        }

        private void NameFilterTextChanged(object sender, TextChangedEventArgs e) // Изменение текста в фильтре при поиске книг по названию
        {
            using (LibraryModelContainer dbContainer = new LibraryModelContainer())
            {
                if (NameTextBox.Text == "") // Если TextBox не заполнен символами
                {
                    BookDataGrid.ItemsSource = dbContainer.Books.Include(c => c.Catalog).Include(c => c.Genre).ToList();
                }
                else // Если TextBox заполнен символами
                {
                    IQueryable<Book> result = dbContainer.Books.Include(c => c.Catalog).Include(c => c.Genre).Where(c => c.Name.Contains(NameTextBox.Text)); // LINQ. Like SQL запрос в БД по названию книги
                    BookDataGrid.ItemsSource = result.ToList();
                }
            }
        }

        private void AuthorFilterTextChanged(object sender, TextChangedEventArgs e) // Изменение текста в фильтре при поиске книг по автору
        {
            using (LibraryModelContainer dbContainer = new LibraryModelContainer())
            {
                if (AuthorTextBox.Text == "") // Если TextBox не заполнен символами
                {
                    BookDataGrid.ItemsSource = dbContainer.Books.Include(c => c.Catalog).Include(c => c.Genre).ToList();
                }
                else // Если TextBox заполнен символами
                {
                    IQueryable<Book> result = dbContainer.Books.Include(c => c.Catalog).Include(c => c.Genre).Where(c => c.Author.Contains(AuthorTextBox.Text)); // LINQ. Like SQL запрос в БД по автору книги
                    BookDataGrid.ItemsSource = result.ToList();
                }
            }
        }

        private void CatalogFilterTextChanged(object sender, TextChangedEventArgs e) // Изменение текста в фильтре при поиске книг по каталогу
        {
            using (LibraryModelContainer dbContainer = new LibraryModelContainer())
            {
                if (CatalogTextBox.Text == "") // Если TextBox не заполнен символами
                {
                    BookDataGrid.ItemsSource = dbContainer.Books.Include(c => c.Catalog).Include(c => c.Genre).ToList();
                }
                else // Если TextBox заполнен символами
                {
                    IQueryable<Book> result = dbContainer.Books.Include(c => c.Catalog).Include(c => c.Genre).Where(c => c.Catalog.Name.Contains(CatalogTextBox.Text)); // LINQ. Like SQL запрос в БД по автору книги
                    BookDataGrid.ItemsSource = result.ToList();
                }
            }
        }

        private void GenreFilterTextChanged(object sender, TextChangedEventArgs e) // Изменение текста в фильтре при поиске книг по жанру
        {
            using (LibraryModelContainer dbContainer = new LibraryModelContainer())
            {
                if (GenreTextBox.Text == "") // Если TextBox не заполнен символами
                {
                    BookDataGrid.ItemsSource = dbContainer.Books.Include(c => c.Catalog).Include(c => c.Genre).ToList();
                }
                else // Если TextBox заполнен символами
                {
                    IQueryable<Book> result = dbContainer.Books.Include(c => c.Catalog).Include(c => c.Genre).Where(c => c.Genre.Name.Contains(GenreTextBox.Text)); // LINQ. Like SQL запрос в БД по автору книги
                    BookDataGrid.ItemsSource = result.ToList();
                }
            }
        }

        private void AddBookToTheBasket_Click(object sender, RoutedEventArgs e) // Добавление книг в корзину заказа
        {
            using (LibraryModelContainer dbContainer = new LibraryModelContainer())
            {
                Book book = dbContainer.Books.Include(c => c.Catalog).Include(c => c.Genre).FirstOrDefault(c => c.Id == ((Book)BookDataGrid.SelectedItem).Id);
                if (book != null) // Если в DataGrid книг выбрана строка
                {
                    BasketBookDataGrid.Items.Add(book); // Добавляем выбранную книгу в корзину
                }
                else // Если в DataGrid книг ничего не выбрано
                {
                    MessageBox.Show("В таблице книг не выбрана книга.");
                }
            }
        }

        private void DeleteBooksFromBasket_Click(object sender, RoutedEventArgs e) // Удаление книг из корзины заказа
        {
            if (BasketBookDataGrid.SelectedItems.Count != 0) // Если в DataGrid корзины книг выбраны одна или несколько строк
            {
                for (int i = BasketBookDataGrid.SelectedItems.Count - 1; i >= 0; i--)
                {
                    BasketBookDataGrid.Items.Remove(BasketBookDataGrid.SelectedItem); // Удаляем выбранне книги из корзины
                }
            }
            else // Если в DataGrid корзины книг ничего не выбрано
            {
                MessageBox.Show("В корзине не выбраны книги для удаления.");
            }
        }

        private void PackageOrder_Click(object sender, RoutedEventArgs e) // Добавить оформленный заказ
        {
            if (ReaderNameComboBox.SelectedItem != null && DeadlineDateOrder.Value != null && BasketBookDataGrid.Items.Count > 0) // Если выбран читатель, указана дата закрытия и таблица "Корзина выбранных книг" непустая
            {
                if (((Reader)ReaderNameComboBox.SelectedItem).Blocked || ((Reader)ReaderNameComboBox.SelectedItem).Status == "Заблокирован")
                {
                    MessageBox.Show($"Читатель: {((Reader)ReaderNameComboBox.SelectedItem).Name}. Выбранный читатель находится в чёрном списке.");
                }
                else if (((Reader)ReaderNameComboBox.SelectedItem).Removed)
                {
                    MessageBox.Show($"Читатель: {((Reader)ReaderNameComboBox.SelectedItem).Name}. Выбранный читатель находится в очереди на удаление из системы.");
                }
                else if (DeadlineDateOrder.Value.Value < DateTime.Now)
                {
                    MessageBox.Show("Значение даты закрытия нового заказа не может быть меньше сегодняшнего дня.");
                }
                else
                {
                    using (LibraryModelContainer dbContainer = new LibraryModelContainer())
                    {
                        Reader reader = dbContainer.Readers.Find(((Reader)ReaderNameComboBox.SelectedItem)?.Id);
                        Entity entity = dbContainer.Entities.First(c => c.Name == "Заказ");
                        User user = dbContainer.Users.Find(_currentUser.Id);

                        ICollection<Book> books = new List<Book>();
                        foreach (Book book in BasketBookDataGrid.Items)
                        {
                            Book findBook = dbContainer.Books.Find(book.Id);
                            books.Add(findBook);
                        }

                        Order order = new Order
                        {
                            Id = Guid.NewGuid(),
                            RegisteredOn = DateTime.Now,
                            DeadlineDate = (DateTime)DeadlineDateOrder.Value,
                            ClosureDate = null,
                            Reader = reader,
                            Books = books
                        };
                        dbContainer.Orders.Add(order);

                        short maxNumber = dbContainer.Orders.Max(c => c.Number);

                        // Создание записи для таблицы EntityRecords
                        EntityRecord record = new EntityRecord
                        {
                            Id = Guid.NewGuid(),
                            Entity = entity,
                            Name = (maxNumber + 1).ToString(),
                            State = "Добавлен",
                            CreatedBy = user,
                            ModifiedBy = null,
                            Order = order
                        };
                        dbContainer.EntityRecords.Add(record);

                        // Создание коллекции историй изменений записи заказа для таблицы EntityHistories
                        IList<EntityHistory> historyRecordList = new List<EntityHistory>
                        {
                            new EntityHistory
                            {
                                Id = Guid.NewGuid(),
                                FieldName = "Номер",
                                OldValue = null,
                                NewValue = order.Number.ToString(),
                                Date = DateTime.Now,
                                User = user,
                                EntityRecord = record
                            },
                            new EntityHistory
                            {
                                Id = Guid.NewGuid(),
                                FieldName = "Дата регистрации",
                                OldValue = null,
                                NewValue = order.RegisteredOn.ToString("F", DeadlineDateOrder.CultureInfo.Parent),
                                Date = DateTime.Now,
                                User = user,
                                EntityRecord = record
                            },
                            new EntityHistory
                            {
                                Id = Guid.NewGuid(),
                                FieldName = "Дата закрытия",
                                OldValue = null,
                                NewValue = order.DeadlineDate.ToString("F", DeadlineDateOrder.CultureInfo.Parent),
                                Date = DateTime.Now,
                                User = user,
                                EntityRecord = record
                            },
                            new EntityHistory
                            {
                                Id = Guid.NewGuid(),
                                FieldName = "Фактическое закрытие",
                                OldValue = null,
                                NewValue = null,
                                Date = DateTime.Now,
                                User = user,
                                EntityRecord = record
                            }
                        };
                        dbContainer.EntityHistories.AddRange(historyRecordList);

                        dbContainer.SaveChanges();
                    }

                    Close();
                }
            }
            else if (ReaderNameComboBox.SelectedItem == null && DeadlineDateOrder.Value == null && BasketBookDataGrid.Items.Count == 0) // Если в окне не заполнена вся информация о новом заказе
            {
                MessageBox.Show("Заполните всю информацию о новом заказе: выберите нужного читателя, укажите дату закрытия и заполните книгами таблицу \"Корзина выбранных книг\".");
            }
            else if (ReaderNameComboBox.SelectedItem == null)
            {
                MessageBox.Show("Не выбран читатель для оформления нового заказа.");
            }
            else if (DeadlineDateOrder.Value == null)
            {
                MessageBox.Show("Не указана дата закрытия нового заказа.");
            }
            else if (BasketBookDataGrid.Items.Count == 0)
            {
                MessageBox.Show("Корзина книг пустая. Выберите нужные книги в таблице \"Таблица книг\" для оформления нового заказа.");
            }
        }

        public void Close_Click(object sender, RoutedEventArgs e) // Закрыть окно добавления заказа
        {
            Close();
        }

        private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e) // Метод перетаскивания окна за любую область
        {
            DragMove();
        }

        private void ReaderNameComboBoxTextChanged(object sender, TextChangedEventArgs e) // Живой поиск в ComboBox читателей
        {
            ReaderNameComboBox.IsDropDownOpen = true;
            var tb = (TextBox)e.OriginalSource; // убрать selection, если dropdown только открылся
            tb.Select(tb.SelectionStart + tb.SelectionLength, 0);
            CollectionView cv = (CollectionView)CollectionViewSource.GetDefaultView(ReaderNameComboBox.ItemsSource);
            cv.Filter = s => ((Reader)s).Name.IndexOf(ReaderNameComboBox.Text, StringComparison.CurrentCultureIgnoreCase) >= 0;
        }
    }
}
