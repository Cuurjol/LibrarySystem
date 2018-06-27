using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Library
{
    public partial class MainWindow
    {
        private readonly User _currentUser; // Текущий авторизованный пользователь системы

        public MainWindow()
        {
            InitializeComponent();

            // https://marlongrech.wordpress.com/2008/05/28/wpf-dialogs-and-dialogresult/
            // https://professorweb.ru/my/WPF/UI_WPF/level23/23_4.php
            // https://msdn.microsoft.com/ru-ru/library/system.windows.window.dialogresult(v=vs.110).aspx

            AuthorizationWindow window = new AuthorizationWindow(); // Создание объекта окна авторизации пользователя в системе
            window.ShowDialog();

            if (window.DialogResult.HasValue && window.DialogResult.Value) // Если пользователь авторизовался в системе
            {
                InitializeComponent();

                using (LibraryModelContainer dbContainer = new LibraryModelContainer())
                {
                    _currentUser = dbContainer.Users.Find(window.CurrentUser.Id);

                    if (_currentUser != null && _currentUser.UserRole == "Supervisor") // Если пользователь с ролью "Supervisor"
                    {
                        _currentUser.TimeInSystem = DateTime.Now;
                        dbContainer.SaveChanges();

                        ReaderDataGrid.ItemsSource = dbContainer.Readers.ToList();
                        OrderDataGrid.ItemsSource = dbContainer.Orders.ToList();
                        CatalogDataGrid.ItemsSource = dbContainer.Catalogs.ToList();
                        BookDataGrid.ItemsSource = dbContainer.Books.ToList();
                        GenreBookComboBox.ItemsSource = dbContainer.Genres.ToList();
                        GenreDirectoryListBox.ItemsSource = dbContainer.Genres.ToList();

                        ReaderForAdminDataGrid.ItemsSource = dbContainer.Readers.ToList();
                        BlackListDataGrid.ItemsSource = dbContainer.BlackLists.ToList();
                        UserDataGrid.ItemsSource = dbContainer.Users.ToList();
                        EntityRecordDataGrid.ItemsSource = dbContainer.EntityRecords.Include(c => c.Entity).ToList();
                    }
                    if (_currentUser != null && _currentUser.UserRole == "Администратор") // Если пользователь с ролью "Администратор"
                    {
                        _currentUser.TimeInSystem = DateTime.Now;
                        dbContainer.SaveChanges();

                        ReadersTabItem.Visibility = Visibility.Collapsed;
                        CatalogsTabItem.Visibility = Visibility.Collapsed;
                        GenreDirectoryTabItem.Visibility = Visibility.Collapsed;

                        ReaderForAdminDataGrid.ItemsSource = dbContainer.Readers.ToList();
                        BlackListDataGrid.ItemsSource = dbContainer.BlackLists.ToList();

                        User currentUser = dbContainer.Users.Find(_currentUser.Id); // Текущий авторизованный пользователь
                        IQueryable<User> result = dbContainer.Users.Where(c =>
                            c.UserRole == "Библиотекарь" || c.Id == currentUser.Id); // Для пользователей с ролью "Администратор" ищем всех пользовталей с ролью "Библиотекарь" и самого себя

                        UserDataGrid.ItemsSource = result.ToList();
                        EntityRecordDataGrid.ItemsSource = dbContainer.EntityRecords.Include(c => c.Entity).Include(c => c.CreatedBy).Include(c => c.ModifiedBy).ToList();
                    }
                    else if (_currentUser != null && _currentUser.UserRole == "Библиотекарь") // Если пользователь с ролью "Библиотекарь"
                    {
                        _currentUser.TimeInSystem = DateTime.Now;
                        dbContainer.SaveChanges();

                        BlackListReadersTabItem.Visibility = Visibility.Collapsed;
                        UsersTabItem.Visibility = Visibility.Collapsed;
                        ReadersTabItem.IsSelected = true;

                        ReaderDataGrid.ItemsSource = dbContainer.Readers.ToList();
                        OrderDataGrid.ItemsSource = dbContainer.Orders.ToList();
                        CatalogDataGrid.ItemsSource = dbContainer.Catalogs.ToList();
                        BookDataGrid.ItemsSource = dbContainer.Books.ToList();
                        GenreBookComboBox.ItemsSource = dbContainer.Genres.ToList();
                        GenreDirectoryListBox.ItemsSource = dbContainer.Genres.ToList();
                    }
                }
            }
            else // Если пользователь вышел из программы
            {
                Close();
            }
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            using (LibraryModelContainer dbContainer = new LibraryModelContainer())
            {
                User currentUser = dbContainer.Users.Find(_currentUser.Id);

                if (currentUser != null)
                {
                    currentUser.TimeOutSystem = DateTime.Now;
                    dbContainer.SaveChanges();
                }
            }
        }

        private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e) // Метод перетаскивания окна за любую область
        {
            DragMove();
        }

        // Метод создания историй изменений записей для таблицы EntityHistory
        private void CreateEntityHistory(string fieldName, string oldValue, string newValue, EntityRecord record)
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

        // Фильтры для таблицы "Читатели"
        #region ReaderDataGrid filters

        private void ReaderRadioButtonChecked(object sender, RoutedEventArgs e) // Фильтрация читателей. Метод нажатия кнопок у компонента RadioButton
        {
            if (ReaderTurnOffFilter.IsChecked == true) // Если нажата кнопка "Выключить фильтрацию"
            {
                // Во время процесса инициализации окна, графические элементы могут быть не загружены физически. Поскольку графический элемент — это объект какого-либо класса,
                // следовательно, если графический элемент физически не загружен, то значение объекта этого элемента = null, а значит, к такому объекту нельзя обращаться через
                // его свойства и нельзя для этого объекта через свойства указывать какие-либо значения. Поэтому для текущего RadioButton делаем проверку IsLoaded.
                if (ReaderTurnOffFilter.IsLoaded)
                {
                    // GroupBox фильтрации заказов делаем доступным
                    OrderFilterGroupBox.IsEnabled = true;

                    // Скрываем фильтры для таблицы читателей
                    NameReaderFilterWrapPanel.Visibility = Visibility.Collapsed;
                    if (NameReaderTextBox.Text != "")
                    {
                        NameReaderTextBox.Text = "";
                    }

                    StatusReaderFilterWrapPanel.Visibility = Visibility.Collapsed;
                    if (StatusReaderComboBox.SelectedItem != null)
                    {
                        StatusReaderComboBox.SelectedItem = null;
                    }

                    SelectedReaderTurnOnFilterWrapPanel.Visibility = Visibility.Collapsed;
                    if (SelectedReaderCheckBox.IsChecked == true)
                    {
                        SelectedReaderCheckBox.IsChecked = false;
                        OrderFilterGroupBox.IsEnabled = true;
                    }
                }
            }
            else if (ReaderTurnOnFilter.IsChecked == true) // Если нажата кнопка "Включить фильтрацию"
            {
                // GroupBox фильтрации заказов делаем недоступным
                OrderFilterGroupBox.IsEnabled = false;

                // Показываем фильтры для таблицы читателей
                NameReaderFilterWrapPanel.Visibility = Visibility.Visible;
                StatusReaderFilterWrapPanel.Visibility = Visibility.Visible;
                SelectedReaderTurnOnFilterWrapPanel.Visibility = Visibility.Visible;
            }
        }

        private void OrderSearchTutnOnFilterForSelectedReader(object sender, RoutedEventArgs e) // Фильтрация поиска заказов для выбранного читателя
        {
            switch (SelectedReaderCheckBox.IsChecked)
            {
                // Включена фильтрация поиска заказов для выбранного читателя
                case true:
                {
                    ReaderDataGrid.IsEnabled = false;
                    CreateReaderButton.IsEnabled = false;
                    UpdateReaderButton.IsEnabled = false;
                    NameReaderFilterWrapPanel.IsEnabled = false;
                    StatusReaderFilterWrapPanel.IsEnabled = false;

                    OrderFilterGroupBox.IsEnabled = true;
                    break;
                }
                // Выключена фильтрация поиска заказов для выбранного читателя
                case false:
                {
                    ReaderDataGrid.IsEnabled = true;
                    CreateReaderButton.IsEnabled = true;
                    UpdateReaderButton.IsEnabled = true;
                    NameReaderFilterWrapPanel.IsEnabled = true;
                    StatusReaderFilterWrapPanel.IsEnabled = true;

                    OrderFilterGroupBox.IsEnabled = false;
                    break;
                }
            }
        }

        #region NameFilter

        private void NameReaderSqlLikeSearch(object sender, TextChangedEventArgs e) // Поиск читателя по ФИО
        {
            using (LibraryModelContainer dbContainer = new LibraryModelContainer())
            {
                if (NameReaderTextBox.Text == "") // Если TextBox ФИО читателя не заполнен символами
                {
                    StatusReaderFilterWrapPanel.IsEnabled = true;

                    ReaderDataGrid.ItemsSource = dbContainer.Readers.ToList();
                    OrderDataGrid.ItemsSource = dbContainer.Orders.ToList();
                }
                else // Если TextBox ФИО читателя заполнен символами
                {
                    IQueryable<Reader> result = dbContainer.Readers.Where(c => c.Name.Contains(NameReaderTextBox.Text)); // LINQ. Like SQL запрос в БД по ФИО читателя
                    ReaderDataGrid.ItemsSource = result.ToList();

                    StatusReaderFilterWrapPanel.IsEnabled = false;
                }
            }
        }

        #endregion

        #region StatusFilter

        private void StatusReaderComboBoxSelection(object sender, SelectionChangedEventArgs e) // Выборка статуса читателя из ComboBox для поиска
        {
            if (StatusReaderComboBox.SelectedItem == null) // Если в ComboBox выбрано пустое значение
            {
                NameReaderTextBox.IsEnabled = true;
                StatusReaderSearchButton.IsEnabled = false;
                StatusReaderCancelSearchButton.IsEnabled = false;
            }
            else // Если в ComboBox выбрано какое-либо значение
            {
                NameReaderTextBox.IsEnabled = false;
                StatusReaderSearchButton.IsEnabled = true;
                StatusReaderCancelSearchButton.IsEnabled = true;
            }
        }

        private void StatusReaderSearchButton_Click(object sender, RoutedEventArgs e) // Поиск читателя по статусу
        {
            using (LibraryModelContainer dbContainer = new LibraryModelContainer())
            {
                IQueryable<Reader> result = dbContainer.Readers.Where(c => c.Status == StatusReaderComboBox.Text); // LINQ. SQL запрос в БД по статусу читателя
                ReaderDataGrid.ItemsSource = result.ToList();
            }
        }

        private void StatusReaderCancelSearchButton_Click(object sender, RoutedEventArgs e) // Отмена поиска читателя по статусу
        {
            using (LibraryModelContainer dbContainer = new LibraryModelContainer())
            {
                StatusReaderComboBox.SelectedItem = null;
                ReaderDataGrid.ItemsSource = dbContainer.Readers.ToList();
            }
        }

        #endregion

        #endregion

        // Основные операции по работе с таблицей "Читатели"
        #region ReaderDataGrid functions

        private void CreateReaderButton_Click(object sender, RoutedEventArgs e) // Создание нового читателя в системе
        {
            using (LibraryModelContainer dbContainer = new LibraryModelContainer())
            {
                ReaderWindow window = new ReaderWindow(true, null, _currentUser);

                window.ShowDialog();

                ReaderDataGrid.ItemsSource = dbContainer.Readers.ToList();

                User currentUser = dbContainer.Users.Find(_currentUser.Id);
                if (currentUser != null && currentUser.UserRole == "Supervisor")
                {
                    ReaderForAdminDataGrid.ItemsSource = dbContainer.Readers.ToList();
                    EntityRecordDataGrid.ItemsSource = dbContainer.EntityRecords.Include(c => c.Entity).Include(c => c.CreatedBy).Include(c => c.ModifiedBy).ToList();
                }
            }
        }

        private void UpdateReaderButton_Click(object sender, RoutedEventArgs e) // Редактирование существующего читателя в системе
        {
            using (LibraryModelContainer dbContainer = new LibraryModelContainer())
            {
                Reader reader = dbContainer.Readers.Find(((Reader) ReaderDataGrid.SelectedItem)?.Id);

                if (reader != null)
                {
                    ReaderWindow window = new ReaderWindow(false, reader, _currentUser);

                    window.ShowDialog();
                
                    dbContainer.Entry(reader).Reload();
                    ReaderDataGrid.ItemsSource = dbContainer.Readers.ToList();

                    User currentUser = dbContainer.Users.Find(_currentUser.Id);
                    if (currentUser != null && currentUser.UserRole == "Supervisor")
                    {
                        ReaderForAdminDataGrid.ItemsSource = dbContainer.Readers.ToList();
                        EntityRecordDataGrid.ItemsSource = dbContainer.EntityRecords.Include(c => c.Entity).Include(c => c.CreatedBy).Include(c => c.ModifiedBy).ToList();
                    }
                }
            }
        }

        private void ReaderDataGridSelectionChanged(object sender, SelectionChangedEventArgs e) // Поиск всех заказов в системе по выбранному читателю в таблице ReaderDataGrid
        {
            using (LibraryModelContainer dbContainer = new LibraryModelContainer())
            {
                Reader reader = dbContainer.Readers.Find(((Reader)ReaderDataGrid.SelectedItem)?.Id);
                if (reader != null)
                {
                    UpdateReaderButton.IsEnabled = true;
                    SelectedReaderCheckBox.IsEnabled = true;

                    IQueryable<Order> result = dbContainer.Orders.Where(c => c.Reader.Id == reader.Id);
                    OrderDataGrid.ItemsSource = result.ToList();
                }
                else
                {
                    UpdateReaderButton.IsEnabled = false;
                    SelectedReaderCheckBox.IsEnabled = false;

                    OrderDataGrid.ItemsSource = dbContainer.Orders.ToList();
                }
            }
        }

        private void ReaderDataGridCancelSelection_DoubleClick(object sender, MouseButtonEventArgs e)
        {
            ReaderDataGrid.SelectedItem = null;
        }

        #endregion

        // Фильтры для таблицы "Заказы"
        #region OrderDataGrid filters

        private void OrderRadioButtonChecked(object sender, RoutedEventArgs e) // Фильтрация заказов. Метод нажатия кнопок у компонента RadioButton
        {
            if (OrderTurnOffFilter.IsChecked == true) // Если нажата кнопка "Выключить фильтрацию"
            {
                // Во время процесса инициализации окна, графические элементы могут быть не загружены физически. Поскольку графический элемент — это объект какого-либо класса,
                // следовательно, если графический элемент физически не загружен, то значение объекта этого элемента = null, а значит, к такому объекту нельзя обращаться через
                // его свойства и нельзя для этого объекта через свойства указывать какие-либо значения. Поэтому для текущего RadioButton делаем проверку IsLoaded.
                if (OrderTurnOffFilter.IsLoaded)
                {
                    // GroupBox фильтрации читателей делаем доступным
                    ReaderFilterGroupBox.IsEnabled = true;

                    // Скрываем фильтры для таблицы читателей
                    NumberOrderFilterWrapPanel.Visibility = Visibility.Collapsed;
                    if (NumberOrderFilterWrapPanel.IsEnabled)
                    {
                        NumberOrderTextBox.Text = "";
                    }

                    DateOrderFilterWrapPanel.Visibility = Visibility.Collapsed;
                    if (DateOrderFilterWrapPanel.IsEnabled)
                    {
                        RegisteredOnOrderCheckBox.IsChecked = false;
                        DeadlineDateOrderCheckBox.IsChecked = false;
                        ClosureDateOrderCheckBox.IsChecked = false;
                        DateTimeFromOrder.Value = null;
                        DateTimeToOrder.Value = null;
                        OrderDateTimeConditionFiltersComboBox.SelectedIndex = 0;
                    }
                }
            }
            else if (OrderTurnOnFilter.IsChecked == true) // Если нажата кнопка "Включить фильтрацию"
            {
                // GroupBox фильтрации заказов делаем недоступным
                ReaderFilterGroupBox.IsEnabled = false;

                // Показываем фильтры для таблицы заказаов
                NumberOrderFilterWrapPanel.Visibility = Visibility.Visible;
                DateOrderFilterWrapPanel.Visibility = Visibility.Visible;
            }
        }

        #region NumberFilter

        private void NumberOrderSqlLikeSearch(object sender, TextChangedEventArgs e) // Поиск заказа по номеру
        {
            using (LibraryModelContainer dbContainer = new LibraryModelContainer())
            {
                if (NumberOrderTextBox.Text == "") // Если TextBox поиска заказа по номеру пустой
                {
                    DateOrderFilterWrapPanel.IsEnabled = true;

                    switch (SelectedReaderCheckBox.IsChecked)
                    {
                        // Вывод в таблицу всех существующих заказов, которые принадлежат выбранному читателю
                        case true:
                        {
                            Reader selectedReader = dbContainer.Readers.Find(((Reader)ReaderDataGrid.SelectedItem)?.Id);

                            IQueryable<Order> result = dbContainer.Orders.Where(c => c.Reader.Id == selectedReader.Id);

                            OrderDataGrid.ItemsSource = result.ToList();
                            break;
                        }
                        // Вывод в таблицу всех существующих заказов в базе данных
                        case false:
                        {
                            OrderDataGrid.ItemsSource = dbContainer.Orders.ToList();
                            break;
                        }
                    }
                }
                else // Если TextBox поиска заказа по номеру имеет текст
                {
                    switch (SelectedReaderCheckBox.IsChecked)
                    {
                        // Поиск заказов по номеру для выбранного читателя
                        case true:
                        {
                            short tempNumber = Convert.ToInt16(NumberOrderTextBox.Text); 
                            Reader selectedReader = dbContainer.Readers.Find(((Reader)ReaderDataGrid.SelectedItem)?.Id);

                            IQueryable<Order> result = dbContainer.Orders.Where(c => 
                                c.Number == tempNumber && 
                                c.Reader.Id == selectedReader.Id);

                            OrderDataGrid.ItemsSource = result.ToList();
                            break;
                        }

                        // Поиск заказов по номеру в базе данных
                        case false:
                        {
                            short tempNumber = Convert.ToInt16(NumberOrderTextBox.Text); 

                            IQueryable<Order> result = dbContainer.Orders.Where(c => c.Number == tempNumber);

                            OrderDataGrid.ItemsSource = result.ToList();
                            break;
                        }
                    }
                }
            }
        }

        #endregion

        #region DateTimeFilters

        private void DateTimeOrderFiltersChecked(object sender, RoutedEventArgs e) // Выбор режима фильтрации заказа по дате
        {
            if (RegisteredOnOrderCheckBox.IsChecked == true) // Если выбран фильтр "Дата регистрации"
            {
                DeadlineDateOrderCheckBox.IsEnabled = false;
                ClosureDateOrderCheckBox.IsEnabled = false;

                DateTimeFromOrder.IsEnabled = true;
                DateTimeToOrder.IsEnabled = true;
            }
            else if (DeadlineDateOrderCheckBox.IsChecked == true) // Если выбран фильтр "Дата закрытия"
            {
                RegisteredOnOrderCheckBox.IsEnabled = false;
                ClosureDateOrderCheckBox.IsEnabled = false;

                DateTimeFromOrder.IsEnabled = true;
                DateTimeToOrder.IsEnabled = true;
            }
            else if (ClosureDateOrderCheckBox.IsChecked == true) // Если выбран фильтр "Фактическое закрытие"
            {
                RegisteredOnOrderCheckBox.IsEnabled = false;
                DeadlineDateOrderCheckBox.IsEnabled = false;

                DateTimeFromOrder.IsEnabled = true;
                DateTimeToOrder.IsEnabled = true;
            }
            else // Если не выбран ни один из фильтров
            {
                RegisteredOnOrderCheckBox.IsEnabled = true;
                DeadlineDateOrderCheckBox.IsEnabled = true;
                ClosureDateOrderCheckBox.IsEnabled = true;

                DateTimeFromOrder.IsEnabled = false;
                DateTimeToOrder.IsEnabled = false;
            }
        }

        private void DateTimeOrderValueChanged(object sender, RoutedPropertyChangedEventArgs<object> e) // Изменение значений времён "Даты от" и "Даты до"
        {
            if (DateTimeFromOrder.Value != null && DateTimeToOrder.Value != null) // Если указаны значения времён "Даты от" и "Даты до"
            {
                OrderDateTimeConditionFiltersComboBox.IsEnabled = false;
                DateTimeSearchOrderButton.IsEnabled = true;
            }
            else if (DateTimeFromOrder.Value != null && OrderDateTimeConditionFiltersComboBox.SelectedItem != null &&
                     ((ContentControl)OrderDateTimeConditionFiltersComboBox.SelectedItem)?.Content.ToString() != "") // Если указаны значение времени "Дата от" и в ComboBox выбрано условие фильтрации
            {
                DateTimeToOrder.IsEnabled = false;
                DateTimeSearchOrderButton.IsEnabled = true;
            }
            else
            {
                DateTimeToOrder.IsEnabled = true;
                OrderDateTimeConditionFiltersComboBox.IsEnabled = true;
                DateTimeSearchOrderButton.IsEnabled = false;
            }
        }

        private void DateTimeOrderConditionFiltersChanged(object sender, SelectionChangedEventArgs e) // Выбор из ComboBox условий поиска заказа по дате
        {
            // Если указаны значение времени "Дата от" и в ComboBox выбрано условие фильтрации
            if (DateTimeFromOrder.Value != null && OrderDateTimeConditionFiltersComboBox.SelectedItem != null && 
                ((ContentControl)OrderDateTimeConditionFiltersComboBox.SelectedItem)?.Content.ToString() != "")
            {
                DateTimeToOrder.IsEnabled = false;
                DateTimeSearchOrderButton.IsEnabled = true;
            }
            else
            {
                DateTimeToOrder.IsEnabled = true;
                DateTimeSearchOrderButton.IsEnabled = false;
            }
        }

        private void DateTimeOrderSearchButton_Click(object sender, RoutedEventArgs e) // Кнопка поиска заказа по дате
        {
            using (LibraryModelContainer dbContainer = new LibraryModelContainer())
            {
                if (RegisteredOnOrderCheckBox.IsChecked == true) // Поиск заказов по дате регистрации
                {
                    // Если указаны значение времени "Дата от" и в ComboBox выбрано условие фильтрации
                    if (DateTimeFromOrder.Value != null && OrderDateTimeConditionFiltersComboBox.SelectedItem != null && 
                        ((ContentControl)OrderDateTimeConditionFiltersComboBox.SelectedItem)?.Content.ToString() != "")
                    {
                        DateTime? tempDateTimeFrom = DateTimeFromOrder.Value.Value.AddSeconds(1);

                        switch (SelectedReaderCheckBox.IsChecked)
                        {
                            // Поиск заказов по дате регистрации для выбранного читателя
                            case true:
                            {
                                Reader selectedReader = dbContainer.Readers.Find(((Reader)ReaderDataGrid.SelectedItem)?.Id);

                                switch (OrderDateTimeConditionFiltersComboBox.SelectedIndex)
                                {
                                    case 1: // Если в ComboBox выбрано значение =
                                    {
                                        IQueryable<Order> result = dbContainer.Orders.Where(c =>
                                            c.RegisteredOn >= DateTimeFromOrder.Value &&
                                            c.RegisteredOn < tempDateTimeFrom.Value &&
                                            c.Reader.Id == selectedReader.Id);

                                        OrderDataGrid.ItemsSource = result.ToList();
                                        break;
                                    }
                                    case 2: // Если в ComboBox выбрано значение ≠
                                    {
                                        IQueryable<Order> result = dbContainer.Orders.Where(c =>
                                            (c.RegisteredOn < DateTimeFromOrder.Value ||
                                            c.RegisteredOn >= tempDateTimeFrom.Value) &&
                                            c.Reader.Id == selectedReader.Id);

                                        OrderDataGrid.ItemsSource = result.ToList();
                                        break;
                                    }
                                    case 3: // Если в ComboBox выбрано значение >
                                    {
                                        IQueryable<Order> result = dbContainer.Orders.Where(c =>
                                            c.RegisteredOn >= tempDateTimeFrom.Value &&
                                            c.Reader.Id == selectedReader.Id);

                                        OrderDataGrid.ItemsSource = result.ToList();
                                        break;
                                    }
                                    case 4: // Если в ComboBox выбрано значение <
                                    {
                                        IQueryable<Order> result = dbContainer.Orders.Where(c =>
                                            c.RegisteredOn < DateTimeFromOrder.Value &&
                                            c.Reader.Id == selectedReader.Id);

                                        OrderDataGrid.ItemsSource = result.ToList();
                                        break;
                                    }
                                    case 5: // Если в ComboBox выбрано значение >=
                                    {
                                        IQueryable<Order> result = dbContainer.Orders.Where(c =>
                                            c.RegisteredOn >= DateTimeFromOrder.Value &&
                                            c.Reader.Id == selectedReader.Id);

                                        OrderDataGrid.ItemsSource = result.ToList();
                                        break;
                                    }
                                    case 6: // Если в ComboBox выбрано значение <=
                                    {
                                        IQueryable<Order> result = dbContainer.Orders.Where(c =>
                                            c.RegisteredOn < tempDateTimeFrom.Value &&
                                            c.Reader.Id == selectedReader.Id);

                                        OrderDataGrid.ItemsSource = result.ToList();
                                        break;
                                    }
                                }

                                break;
                            }
                            // Поиск заказов по дате регистрации в базе данных
                            case false:
                            {
                                switch (OrderDateTimeConditionFiltersComboBox.SelectedIndex)
                                {
                                    case 1: // Если в ComboBox выбрано значение =
                                    {
                                        IQueryable<Order> result = dbContainer.Orders.Where(c =>
                                            c.RegisteredOn >= DateTimeFromOrder.Value &&
                                            c.RegisteredOn < tempDateTimeFrom.Value);

                                        OrderDataGrid.ItemsSource = result.ToList();
                                        break;
                                    }
                                    case 2: // Если в ComboBox выбрано значение ≠
                                    {
                                        IQueryable<Order> result = dbContainer.Orders.Where(c =>
                                            c.RegisteredOn < DateTimeFromOrder.Value ||
                                            c.RegisteredOn >= tempDateTimeFrom.Value);

                                        OrderDataGrid.ItemsSource = result.ToList();
                                        break;
                                    }
                                    case 3: // Если в ComboBox выбрано значение >
                                    {
                                        IQueryable<Order> result = dbContainer.Orders.Where(c =>
                                            c.RegisteredOn >= tempDateTimeFrom.Value);

                                        OrderDataGrid.ItemsSource = result.ToList();
                                        break;
                                    }
                                    case 4: // Если в ComboBox выбрано значение <
                                    {
                                        IQueryable<Order> result = dbContainer.Orders.Where(c =>
                                            c.RegisteredOn < DateTimeFromOrder.Value);

                                        OrderDataGrid.ItemsSource = result.ToList();
                                        break;
                                    }
                                    case 5: // Если в ComboBox выбрано значение >=
                                    {
                                        IQueryable<Order> result = dbContainer.Orders.Where(c =>
                                            c.RegisteredOn >= DateTimeFromOrder.Value);

                                        OrderDataGrid.ItemsSource = result.ToList();
                                        break;
                                    }
                                    case 6: // Если в ComboBox выбрано значение <=
                                    {
                                        IQueryable<Order> result = dbContainer.Orders.Where(c =>
                                            c.RegisteredOn < tempDateTimeFrom.Value);

                                        OrderDataGrid.ItemsSource = result.ToList();
                                        break;
                                    }
                                }

                                break;
                            }
                        }
                    }
                    else if (DateTimeFromOrder.Value != null && DateTimeToOrder.Value != null) // Если указаны значения времён "Даты от" и "Даты до"
                    {
                        DateTime? tempDateTimeTo = DateTimeToOrder.Value.Value.AddSeconds(1);

                        switch (SelectedReaderCheckBox.IsChecked)
                        {
                            // Поиск заказов по дате регистрации для выбранного читателя
                            case true:
                            {
                                Reader selectedReader = dbContainer.Readers.Find(((Reader)ReaderDataGrid.SelectedItem)?.Id);

                                IQueryable<Order> result = dbContainer.Orders.Where(c => 
                                    c.RegisteredOn >= DateTimeFromOrder.Value && 
                                    c.RegisteredOn < tempDateTimeTo.Value &&
                                    c.Reader.Id == selectedReader.Id);

                                OrderDataGrid.ItemsSource = result.ToList();
                                break;
                            }
                            // Поиск заказов по дате регистрации в базе данных
                            case false:
                            {
                                IQueryable<Order> result = dbContainer.Orders.Where(c => 
                                    c.RegisteredOn >= DateTimeFromOrder.Value && 
                                    c.RegisteredOn < tempDateTimeTo.Value);

                                OrderDataGrid.ItemsSource = result.ToList();
                                break;
                            }
                        }
                    }
                }
                else if (DeadlineDateOrderCheckBox.IsChecked == true) // Поиск заказов по дате закрытия
                {
                    // Если указаны значение времени "Дата от" и в ComboBox выбрано условие фильтрации
                    if (DateTimeFromOrder.Value != null && OrderDateTimeConditionFiltersComboBox.SelectedItem != null && 
                        ((ContentControl)OrderDateTimeConditionFiltersComboBox.SelectedItem)?.Content.ToString() != "")
                    {
                        DateTime? tempDateTimeFrom = DateTimeFromOrder.Value.Value.AddSeconds(1);

                        switch (SelectedReaderCheckBox.IsChecked)
                        {
                            // Поиск заказов по дате закрытия для выбранного читателя
                            case true:
                            {
                                Reader selectedReader = dbContainer.Readers.Find(((Reader)ReaderDataGrid.SelectedItem)?.Id);

                                switch (OrderDateTimeConditionFiltersComboBox.SelectedIndex)
                                {
                                    case 1: // Если в ComboBox выбрано значение =
                                    {
                                        IQueryable<Order> result = dbContainer.Orders.Where(c =>
                                            c.DeadlineDate >= DateTimeFromOrder.Value &&
                                            c.DeadlineDate < tempDateTimeFrom.Value &&
                                            c.Reader.Id == selectedReader.Id);

                                        OrderDataGrid.ItemsSource = result.ToList();
                                        break;
                                    }
                                    case 2: // Если в ComboBox выбрано значение ≠
                                    {
                                        IQueryable<Order> result = dbContainer.Orders.Where(c =>
                                            (c.DeadlineDate < DateTimeFromOrder.Value ||
                                            c.DeadlineDate >= tempDateTimeFrom.Value) &&
                                            c.Reader.Id == selectedReader.Id);

                                        OrderDataGrid.ItemsSource = result.ToList();
                                        break;
                                    }
                                    case 3: // Если в ComboBox выбрано значение >
                                    {
                                        IQueryable<Order> result = dbContainer.Orders.Where(c =>
                                            c.DeadlineDate >= tempDateTimeFrom.Value &&
                                            c.Reader.Id == selectedReader.Id);

                                        OrderDataGrid.ItemsSource = result.ToList();
                                        break;
                                    }
                                    case 4: // Если в ComboBox выбрано значение <
                                    {
                                        IQueryable<Order> result = dbContainer.Orders.Where(c =>
                                            c.DeadlineDate < DateTimeFromOrder.Value &&
                                            c.Reader.Id == selectedReader.Id);

                                        OrderDataGrid.ItemsSource = result.ToList();
                                        break;
                                    }
                                    case 5: // Если в ComboBox выбрано значение >=
                                    {
                                        IQueryable<Order> result = dbContainer.Orders.Where(c =>
                                            c.DeadlineDate >= DateTimeFromOrder.Value &&
                                            c.Reader.Id == selectedReader.Id);

                                        OrderDataGrid.ItemsSource = result.ToList();
                                        break;
                                    }
                                    case 6: // Если в ComboBox выбрано значение <=
                                    {
                                        IQueryable<Order> result = dbContainer.Orders.Where(c =>
                                            c.DeadlineDate < tempDateTimeFrom.Value &&
                                            c.Reader.Id == selectedReader.Id);

                                        OrderDataGrid.ItemsSource = result.ToList();
                                        break;
                                    }
                                }

                                break;
                            }
                            // Поиск заказов по дате закрытия в базе данных
                            case false:
                            {
                                switch (OrderDateTimeConditionFiltersComboBox.SelectedIndex)
                                {
                                    case 1: // Если в ComboBox выбрано значение =
                                    {
                                        IQueryable<Order> result = dbContainer.Orders.Where(c =>
                                            c.DeadlineDate >= DateTimeFromOrder.Value &&
                                            c.DeadlineDate < tempDateTimeFrom.Value);

                                        OrderDataGrid.ItemsSource = result.ToList();
                                        break;
                                    }
                                    case 2: // Если в ComboBox выбрано значение ≠
                                    {
                                        IQueryable<Order> result = dbContainer.Orders.Where(c =>
                                            c.DeadlineDate < DateTimeFromOrder.Value ||
                                            c.DeadlineDate >= tempDateTimeFrom.Value);

                                        OrderDataGrid.ItemsSource = result.ToList();
                                        break;
                                    }
                                    case 3: // Если в ComboBox выбрано значение >
                                    {
                                        IQueryable<Order> result = dbContainer.Orders.Where(c =>
                                            c.DeadlineDate >= tempDateTimeFrom.Value);

                                        OrderDataGrid.ItemsSource = result.ToList();
                                        break;
                                    }
                                    case 4: // Если в ComboBox выбрано значение <
                                    {
                                        IQueryable<Order> result = dbContainer.Orders.Where(c =>
                                            c.DeadlineDate < DateTimeFromOrder.Value);

                                        OrderDataGrid.ItemsSource = result.ToList();
                                        break;
                                    }
                                    case 5: // Если в ComboBox выбрано значение >=
                                    {
                                        IQueryable<Order> result = dbContainer.Orders.Where(c =>
                                            c.DeadlineDate >= DateTimeFromOrder.Value);

                                        OrderDataGrid.ItemsSource = result.ToList();
                                        break;
                                    }
                                    case 6: // Если в ComboBox выбрано значение <=
                                    {
                                        IQueryable<Order> result = dbContainer.Orders.Where(c =>
                                            c.DeadlineDate < tempDateTimeFrom.Value);

                                        OrderDataGrid.ItemsSource = result.ToList();
                                        break;
                                    }
                                }

                                break;
                            }
                        }
                    }
                    else if (DateTimeFromOrder.Value != null && DateTimeToOrder.Value != null) // Если указаны значения времён "Даты от" и "Даты до"
                    {
                        DateTime? tempDateTimeTo = DateTimeToOrder.Value.Value.AddSeconds(1);

                        switch (SelectedReaderCheckBox.IsChecked)
                        {
                            // Поиск заказов по дате закрытия для выбранного читателя
                            case true:
                            {
                                Reader selectedReader = dbContainer.Readers.Find(((Reader)ReaderDataGrid.SelectedItem)?.Id);

                                IQueryable<Order> result = dbContainer.Orders.Where(c => 
                                    c.DeadlineDate >= DateTimeFromOrder.Value && 
                                    c.DeadlineDate < tempDateTimeTo.Value &&
                                    c.Reader.Id == selectedReader.Id);

                                OrderDataGrid.ItemsSource = result.ToList();
                                break;
                            }
                            // Поиск заказов по дате закрытия в базе данных
                            case false:
                            {
                                IQueryable<Order> result = dbContainer.Orders.Where(c => 
                                    c.DeadlineDate >= DateTimeFromOrder.Value && 
                                    c.DeadlineDate < tempDateTimeTo.Value);

                                OrderDataGrid.ItemsSource = result.ToList();
                                break;
                            }
                        }

                    }
                }
                else if (ClosureDateOrderCheckBox.IsChecked == true) // Поиск заказов по дате фактического закрытия
                {
                    // Если указаны значение времени "Дата от" и в ComboBox выбрано условие фильтрации
                    if (DateTimeFromOrder.Value != null && OrderDateTimeConditionFiltersComboBox.SelectedItem != null && 
                        ((ContentControl)OrderDateTimeConditionFiltersComboBox.SelectedItem)?.Content.ToString() != "")
                    {
                        DateTime? tempDateTimeFrom = DateTimeFromOrder.Value.Value.AddSeconds(1);

                        switch (SelectedReaderCheckBox.IsChecked)
                        {
                            // Поиск заказов по дате фактического закрытия для выбранного читателя
                            case true:
                            {
                                Reader selectedReader = dbContainer.Readers.Find(((Reader)ReaderDataGrid.SelectedItem)?.Id);

                                switch (OrderDateTimeConditionFiltersComboBox.SelectedIndex)
                                {
                                    case 1: // Если в ComboBox выбрано значение =
                                    {
                                        IQueryable<Order> result = dbContainer.Orders.Where(c =>
                                            c.ClosureDate >= DateTimeFromOrder.Value &&
                                            c.ClosureDate < tempDateTimeFrom.Value &&
                                            c.Reader.Id == selectedReader.Id);

                                        OrderDataGrid.ItemsSource = result.ToList();
                                        break;
                                    }
                                    case 2: // Если в ComboBox выбрано значение ≠
                                    {
                                        IQueryable<Order> result = dbContainer.Orders.Where(c =>
                                            (c.ClosureDate < DateTimeFromOrder.Value ||
                                            c.ClosureDate >= tempDateTimeFrom.Value) &&
                                            c.Reader.Id == selectedReader.Id);

                                        OrderDataGrid.ItemsSource = result.ToList();
                                        break;
                                    }
                                    case 3: // Если в ComboBox выбрано значение >
                                    {
                                        IQueryable<Order> result = dbContainer.Orders.Where(c =>
                                            c.ClosureDate >= tempDateTimeFrom.Value &&
                                            c.Reader.Id == selectedReader.Id);

                                        OrderDataGrid.ItemsSource = result.ToList();
                                        break;
                                    }
                                    case 4: // Если в ComboBox выбрано значение <
                                    {
                                        IQueryable<Order> result = dbContainer.Orders.Where(c =>
                                            c.ClosureDate < DateTimeFromOrder.Value &&
                                            c.Reader.Id == selectedReader.Id);

                                        OrderDataGrid.ItemsSource = result.ToList();
                                        break;
                                    }
                                    case 5: // Если в ComboBox выбрано значение >=
                                    {
                                        IQueryable<Order> result = dbContainer.Orders.Where(c =>
                                            c.ClosureDate >= DateTimeFromOrder.Value &&
                                            c.Reader.Id == selectedReader.Id);

                                        OrderDataGrid.ItemsSource = result.ToList();
                                        break;
                                    }
                                    case 6: // Если в ComboBox выбрано значение <=
                                    {
                                        IQueryable<Order> result = dbContainer.Orders.Where(c =>
                                            c.ClosureDate < tempDateTimeFrom.Value &&
                                            c.Reader.Id == selectedReader.Id);

                                        OrderDataGrid.ItemsSource = result.ToList();
                                        break;
                                    }
                                }

                                break;
                            }
                            // Поиск заказов по дате фактического закрытия в базе данных
                            case false:
                            {
                                switch (OrderDateTimeConditionFiltersComboBox.SelectedIndex)
                                {
                                    case 1: // Если в ComboBox выбрано значение =
                                    {
                                        IQueryable<Order> result = dbContainer.Orders.Where(c =>
                                            c.ClosureDate >= DateTimeFromOrder.Value &&
                                            c.ClosureDate < tempDateTimeFrom.Value);

                                        OrderDataGrid.ItemsSource = result.ToList();
                                        break;
                                    }
                                    case 2: // Если в ComboBox выбрано значение ≠
                                    {
                                        IQueryable<Order> result = dbContainer.Orders.Where(c =>
                                            c.ClosureDate < DateTimeFromOrder.Value ||
                                            c.ClosureDate >= tempDateTimeFrom.Value);

                                        OrderDataGrid.ItemsSource = result.ToList();
                                        break;
                                    }
                                    case 3: // Если в ComboBox выбрано значение >
                                    {
                                        IQueryable<Order> result = dbContainer.Orders.Where(c =>
                                            c.ClosureDate >= tempDateTimeFrom.Value);

                                        OrderDataGrid.ItemsSource = result.ToList();
                                        break;
                                    }
                                    case 4: // Если в ComboBox выбрано значение <
                                    {
                                        IQueryable<Order> result = dbContainer.Orders.Where(c =>
                                            c.ClosureDate < DateTimeFromOrder.Value);

                                        OrderDataGrid.ItemsSource = result.ToList();
                                        break;
                                    }
                                    case 5: // Если в ComboBox выбрано значение >=
                                    {
                                        IQueryable<Order> result = dbContainer.Orders.Where(c =>
                                            c.ClosureDate >= DateTimeFromOrder.Value);

                                        OrderDataGrid.ItemsSource = result.ToList();
                                        break;
                                    }
                                    case 6: // Если в ComboBox выбрано значение <=
                                    {
                                        IQueryable<Order> result = dbContainer.Orders.Where(c =>
                                            c.ClosureDate < tempDateTimeFrom.Value);

                                        OrderDataGrid.ItemsSource = result.ToList();
                                        break;
                                    }
                                }

                                break;
                            }
                        }

                        
                    }
                    else if (DateTimeFromOrder.Value != null && DateTimeToOrder.Value != null) // Если указаны значения времён "Даты от" и "Даты до"
                    {
                        DateTime? tempDateTimeTo = DateTimeToOrder.Value.Value.AddSeconds(1);

                        switch (SelectedReaderCheckBox.IsChecked)
                        {
                            // Поиск заказов по дате фактического закрытия для выбранного читателя
                            case true:
                            {
                                Reader selectedReader = dbContainer.Readers.Find(((Reader)ReaderDataGrid.SelectedItem)?.Id);

                                IQueryable<Order> result = dbContainer.Orders.Where(c => 
                                    c.ClosureDate >= DateTimeFromOrder.Value && 
                                    c.ClosureDate < tempDateTimeTo.Value &&
                                    c.Reader.Id == selectedReader.Id);

                                OrderDataGrid.ItemsSource = result.ToList();

                                break;
                            }
                            // Поиск заказов по дате фактического закрытия в базе данных
                            case false:
                            {
                                IQueryable<Order> result = dbContainer.Orders.Where(c => 
                                    c.ClosureDate >= DateTimeFromOrder.Value && 
                                    c.ClosureDate < tempDateTimeTo.Value);

                                OrderDataGrid.ItemsSource = result.ToList();
                                break;
                            }
                        }
                    }
                }
            }
        }

        private void DateTimeOrderCancelSearchButton_Click(object sender, RoutedEventArgs e) // Отмена поиска заказа по дате
        {
            using (LibraryModelContainer dbContainer = new LibraryModelContainer())
            {
                DateTimeToOrder.Value = null;
                DateTimeFromOrder.Value = null;
                OrderDateTimeConditionFiltersComboBox.SelectedIndex = 0;

                switch (SelectedReaderCheckBox.IsChecked)
                {
                    // Вывод в таблицу всех существующих заказов, которые принадлежат выбранному читателю
                    case true:
                    {
                        Reader selectedReader = dbContainer.Readers.Find(((Reader)ReaderDataGrid.SelectedItem)?.Id);

                        IQueryable<Order> result = dbContainer.Orders.Where(c => c.Reader.Id == selectedReader.Id);

                        OrderDataGrid.ItemsSource = result.ToList();
                        break;
                    }
                    // Вывод в таблицу всех существующих заказов в базе данных
                    case false:
                    {
                        OrderDataGrid.ItemsSource = dbContainer.Orders.ToList();
                        break;
                    }
                }
            }
        }

        #endregion

        #endregion

        // Основные операции по работе с таблицей "Заказы"
        #region OrderDataGrid functions

        private void CreateOrderButton_Click(object sender, RoutedEventArgs e) // Регистрация заказа в системе
        {
            using (LibraryModelContainer dbContainer = new LibraryModelContainer())
            {
                OrderWindow window = new OrderWindow(_currentUser);
                window.ShowDialog();

                OrderDataGrid.ItemsSource = dbContainer.Orders.ToList();
                EntityRecordDataGrid.ItemsSource = dbContainer.EntityRecords.Include(c => c.Entity).Include(c => c.CreatedBy).Include(c => c.ModifiedBy).ToList();
            }
        }

        private void ViewOrderDetailButton_Click(object sender, RoutedEventArgs e)
        {
            using (LibraryModelContainer dbContainer = new LibraryModelContainer())
            {
                Order order = dbContainer.Orders.Find(((Order) OrderDataGrid.SelectedItem)?.Id);

                if (order != null)
                {
                    OrderDetailWindow window = new OrderDetailWindow(order, _currentUser)
                    {
                        ReaderNameTextBox = {Text = order.Reader.Name},
                        BasketBookDataGrid = {ItemsSource = order.Books}
                    };

                    window.ShowDialog();

                    if (SelectedReaderCheckBox.IsChecked == true)
                    {
                        Reader selectedReader = order.Reader;
                        ReaderDataGrid.ItemsSource = dbContainer.Readers.ToList();
                        ReaderDataGrid.SelectedValue = selectedReader;
                    }
                    else
                    {
                        ReaderDataGrid.ItemsSource = dbContainer.Readers.ToList();
                    }
                }
            }
        }

        private void CloseOrderButton_Click(object sender, RoutedEventArgs e) // Закрытие существующего заказа в системе
        {
            using (LibraryModelContainer dbContainer = new LibraryModelContainer())
            {
                Order order = dbContainer.Orders.Find(((Order)OrderDataGrid.SelectedItem)?.Id);
                User user = dbContainer.Users.Find(_currentUser.Id);

                if (order != null)
                {
                    if (order.ClosureDate == null)
                    {
                        order.ClosureDate = DateTime.Now;

                        EntityRecord record = dbContainer.EntityRecords.First(c => c.Order.Id == order.Id); // Находим запись нужного заказа для редактирования в таблице EntityRecords

                        // Меняем значения у объекта типа EntityRecord
                        record.ModifiedBy = user;
                        record.State = "Изменён";

                        CreateEntityHistory("Фактическое закрытие", null, order.ClosureDate?.ToString("F", new CultureInfo("ru-RU")), record);

                        dbContainer.SaveChanges();

                        OrderDataGrid.ItemsSource = dbContainer.Orders.ToList();
                        EntityRecordDataGrid.ItemsSource = dbContainer.EntityRecords.Include(c => c.Entity).Include(c => c.CreatedBy).Include(c => c.ModifiedBy).ToList();

                        MessageBox.Show($"Выбранный заказ успешно закрыт. Дата фактического закрытия: {order.ClosureDate?.ToString("F", new CultureInfo("ru-RU"))}.");
                    }
                    else
                    {
                        MessageBox.Show("Выбранный заказ в таблице уже закрыт. Нельзя повторно перезакрывать заказ.");
                    }
                }
            }
        }

        private void OrderDataGridSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            using (LibraryModelContainer dbContainer = new LibraryModelContainer())
            {
                Order order = dbContainer.Orders.Find(((Order) OrderDataGrid.SelectedItem)?.Id);
                ViewOrderаButton.IsEnabled = order != null;
                CloseOrderButton.IsEnabled = order != null;
            }
        }

        private void OrderDataGridCancelSelection_DoubleClick(object sender, MouseButtonEventArgs e)
        {
            OrderDataGrid.SelectedItem = null;
        }

        #endregion

        // Фильтры для таблицы "Каталоги"
        #region CatalogDataGrid filters

        private void CatalogRadioButtonChecked(object sender, RoutedEventArgs e) // Фильтрация каталогов. Метод нажатия кнопок у компонента RadioButton
        {
            if (CatalogTurnOffFilter.IsChecked == true) // Если нажата кнопка "Выключить фильтрацию"
            {
                // Во время процесса инициализации окна, графические элементы могут быть не загружены физически. Поскольку графический элемент — это объект какого-либо класса,
                // следовательно, если графический элемент физически не загружен, то значение объекта этого элемента = null, а значит, к такому объекту нельзя обращаться через
                // его свойства и нельзя для этого объекта через свойства указывать какие-либо значения. Поэтому для текущего RadioButton делаем проверку IsLoaded.
                if (CatalogTurnOffFilter.IsLoaded)
                {
                    // GroupBox фильтрации книг делаем доступным
                    BookFilterGroupBox.IsEnabled = true;

                    // Скрываем фильтры для таблицы каталогов
                    NameCatalogFilterWrapPanel.Visibility = Visibility.Collapsed;
                    if (NameCatalogTextBox.Text != "")
                    {
                        NameCatalogTextBox.Text = "";
                    }

                    SelectedCatalogTurnOnFilterWrapPanel.Visibility = Visibility.Collapsed;
                    if (SelectedCatalogCheckBox.IsChecked == true)
                    {
                        SelectedCatalogCheckBox.IsChecked = false;
                        BookFilterGroupBox.IsEnabled = true;
                    }
                }
            }
            else if (CatalogTurnOnFilter.IsChecked == true) // Если нажата кнопка "Включить фильтрацию"
            {
                // GroupBox фильтрации книг делаем недоступным
                BookFilterGroupBox.IsEnabled = false;

                // Показываем фильтры для таблицы каталогов
                NameCatalogFilterWrapPanel.Visibility = Visibility.Visible;
                SelectedCatalogTurnOnFilterWrapPanel.Visibility = Visibility.Visible;
            }
        }

        private void BookSearchTutnOnFilterForSelectedCatalog(object sender, RoutedEventArgs e) // Фильтрация поиска книг для выбранного каталога
        {
            switch (SelectedCatalogCheckBox.IsChecked)
            {
                // Включена фильтрация поиска книг для выбранного каталога
                case true:
                {
                    CatalogDataGrid.IsEnabled = false;
                    CreateCatalogButton.IsEnabled = false;
                    DeleteCatalogButton.IsEnabled = false;
                    UpdateCatalogButton.IsEnabled = false;
                    NameCatalogFilterWrapPanel.IsEnabled = false;

                    BookFilterGroupBox.IsEnabled = true;
                    break;
                }
                // Выключена фильтрация поиска книг для выбранного каталога
                case false:
                {
                    CatalogDataGrid.IsEnabled = true;
                    CreateCatalogButton.IsEnabled = true;
                    DeleteCatalogButton.IsEnabled = true;
                    UpdateCatalogButton.IsEnabled = true;
                    NameCatalogFilterWrapPanel.IsEnabled = true;

                    BookFilterGroupBox.IsEnabled = false;
                    break;
                }   
            }
        }

        private void NameCatalogSqlLikeSearch(object sender, TextChangedEventArgs e) // Поиск каталога по названию
        {
            using (LibraryModelContainer dbContainer = new LibraryModelContainer())
            {
                if (NameCatalogTextBox.Text == "") // Если TextBox названия каталога не заполнен символами
                {
                    CatalogDataGrid.ItemsSource = dbContainer.Catalogs.ToList();
                    BookDataGrid.ItemsSource = dbContainer.Books.ToList();
                }
                else // Если TextBox названия каталога заполнен символами
                {
                    IQueryable<Catalog> result = dbContainer.Catalogs.Where(c => c.Name.Contains(NameCatalogTextBox.Text)); // LINQ. Like SQL запрос в БД по названию каталога
                    CatalogDataGrid.ItemsSource = result.ToList();
                }
            }
        }

        #endregion

        // Основные операции по работе с таблицей "Каталоги"
        #region CatalogDataGrid functions

        private void CreateCatalogButton_Click(object sender, RoutedEventArgs e) // Создание каталога в системе
        {
            using (LibraryModelContainer dbContainer = new LibraryModelContainer())
            {
                CatalogWindow window = new CatalogWindow(true, null, _currentUser);

                window.ShowDialog();

                CatalogDataGrid.ItemsSource = dbContainer.Catalogs.ToList();
                EntityRecordDataGrid.ItemsSource = dbContainer.EntityRecords.Include(c => c.Entity).Include(c => c.CreatedBy).Include(c => c.ModifiedBy).ToList();
            }
        }

        private void UpdateCatalogButton_Click(object sender, RoutedEventArgs e) // Редактирование существующего каталога в системе
        {
            using (LibraryModelContainer dbContainer = new LibraryModelContainer())
            {
                Catalog catalog = dbContainer.Catalogs.Find(((Catalog)CatalogDataGrid.SelectedItem)?.Id);
                
                if (catalog != null)
                {
                    CatalogWindow window = new CatalogWindow(false, catalog, _currentUser);

                    window.ShowDialog();
                }

                dbContainer.Entry(catalog).Reload();
                CatalogDataGrid.ItemsSource = dbContainer.Catalogs.ToList();
                BookDataGrid.ItemsSource = dbContainer.Books.Include(c => c.Genre).Include(c => c.Catalog).ToList();
                EntityRecordDataGrid.ItemsSource = dbContainer.EntityRecords.Include(c => c.Entity).Include(c => c.CreatedBy).Include(c => c.ModifiedBy).ToList();
            }
        }

        private void DeleteCatalogButton_Click(object sender, RoutedEventArgs e)
        {
            using (LibraryModelContainer dbContainer = new LibraryModelContainer())
            {
                Catalog selectedCatalog = dbContainer.Catalogs.Find(((Catalog) CatalogDataGrid.SelectedItem)?.Id);
                EntityRecord record = dbContainer.EntityRecords.First(c => c.Catalog.Id == selectedCatalog.Id); // Находим запись нужного каталога для редактирования в таблице EntityRecords
                User user = dbContainer.Users.Find(_currentUser.Id);

                if (selectedCatalog != null)
                {
                    record.ModifiedBy = user;
                    record.State = "Удалён";

                    // Создание коллекции историй изменений записи каталога для таблицы EntityHistories
                    IList<EntityHistory> historyRecordList = new List<EntityHistory>
                    {
                        new EntityHistory
                        {
                            Id = Guid.NewGuid(),
                            FieldName = "Название",
                            OldValue = selectedCatalog.Name,
                            NewValue = null,
                            Date = DateTime.Now,
                            User = user,
                            EntityRecord = record
                        },
                        new EntityHistory
                        {
                            Id = Guid.NewGuid(),
                            FieldName = "Описание",
                            OldValue = selectedCatalog.Description,
                            NewValue = null,
                            Date = DateTime.Now,
                            User = user,
                            EntityRecord = record
                        }
                    };
                    dbContainer.EntityHistories.AddRange(historyRecordList);

                    foreach (Book itemBook in selectedCatalog.Books) // Фиксируем изменения информации в тех кнингах, где был удалён выбранный каталог
                    {
                        // Находим запись нужной книги удалённого каталога для редактирования в таблице EntityRecords
                        EntityRecord recordBook = dbContainer.EntityRecords.First(c => c.Book.Id == itemBook.Id);

                        recordBook.ModifiedBy = user;
                        recordBook.State = "Изменён";

                        CreateEntityHistory("Каталог", itemBook.Catalog.Name, null, recordBook);
                    }
                    dbContainer.Catalogs.Remove(selectedCatalog);

                    dbContainer.SaveChanges();

                    BookDataGrid.ItemsSource = dbContainer.Books.ToList(); // Обновляем выводимые данные книг в BookDataGrid после удаления каталога из таблицы CatalogDataGrid
                    CatalogDataGrid.ItemsSource = dbContainer.Catalogs.ToList();
                    EntityRecordDataGrid.ItemsSource = dbContainer.EntityRecords.Include(c => c.Entity).Include(c => c.CreatedBy).Include(c => c.ModifiedBy).ToList();
                    MessageBox.Show($"Каталог: {selectedCatalog.Name}. Выбранный каталог успешно удалён из системы.");
                }
            }
        }

        private void CatalogDataGridSelectionChanged(object sender, SelectionChangedEventArgs e) // Смена выборки каталогов в таблице CatalogDataGrid
        {
            using (LibraryModelContainer dbContainer = new LibraryModelContainer())
            {
                Catalog catalog = dbContainer.Catalogs.Find(((Catalog)CatalogDataGrid.SelectedItem)?.Id);
                if (catalog != null)
                {
                    UpdateCatalogButton.IsEnabled = true;
                    DeleteCatalogButton.IsEnabled = true;
                    SelectedCatalogCheckBox.IsEnabled = true;

                    //https://stackoverflow.com/questions/36432769/an-exception-of-type-system-notsupportedexception-thrown-in-asp-net-program
                    // c.Catalog == catalog нельзя писать, поскольку в LINQ нельзя сравнивать объекты друг с другом внутри EF-запроса,
                    // нужно делать сравнение через свойства этих объектов внутри EF-запроса. Подсказка указана выше по ссылке
                    IQueryable<Book> result = dbContainer.Books.Where(c => c.Catalog.Id == catalog.Id); // Поиск всех книг из БД по выбранному каталогу в таблице CatalogDataGrid

                    // Для того, чтобы корректно загружались названия жанров к книгам в DataGrid, нужно через Include обращаться к классу Genre для загрузки всех книг и связанные с ними жанры
                    BookDataGrid.ItemsSource = result.Include(c => c.Genre).ToList();
                }
                else
                {
                    UpdateCatalogButton.IsEnabled = false;
                    DeleteCatalogButton.IsEnabled = false;
                    SelectedCatalogCheckBox.IsEnabled = false;

                    BookDataGrid.ItemsSource = dbContainer.Books.Include(c => c.Genre).Include(c => c.Catalog).ToList();
                }
            }
        }

        private void CatalogDataGridCancelSelection_DoubleClick(object sender, MouseButtonEventArgs e) // Снимает выборку каталога в таблице CatalogDataGrid
        {
            CatalogDataGrid.SelectedItem = null;
        }

        #endregion

        // Фильтры для таблицы "Книги"
        #region BookDataGrid filters

        private void BookRadioButtonChecked(object sender, RoutedEventArgs e) // Фильтрация книг. Метод нажатия кнопок у компонента RadioButton
        {
            if (BookTurnOffFilter.IsChecked == true) // Если нажата кнопка "Выключить фильтрацию"
            {
                // Во время процесса инициализации окна, графические элементы могут быть не загружены физически. Поскольку графический элемент — это объект какого-либо класса,
                // следовательно, если графический элемент физически не загружен, то значение объекта этого элемента = null, а значит, к такому объекту нельзя обращаться через
                // его свойства и нельзя для этого объекта через свойства указывать какие-либо значения. Поэтому для текущего RadioButton делаем проверку IsLoaded.
                if (BookTurnOffFilter.IsLoaded)
                {
                    // GroupBox фильтрации каталогов делаем доступным
                    CatalogFilterGroupBox.IsEnabled = true;

                    // Скрываем фильтры для таблицы книг
                    NameBookFilterWrapPanel.Visibility = Visibility.Collapsed;
                    if (NameBookFilterWrapPanel.IsEnabled)
                    {
                        NameBookTextBox.Text = "";
                    }

                    GenreBookFilterWrapPanel.Visibility = Visibility.Collapsed;
                    if (GenreBookFilterWrapPanel.IsEnabled)
                    {
                        GenreBookComboBox.SelectedItem = null;
                    }

                    AuthorBookFilterWrapPanel.Visibility = Visibility.Collapsed;
                    if (AuthorBookFilterWrapPanel.IsEnabled)
                    {
                        AuthorBookTextBox.Text = "";
                    }

                    YearBookFilterWrapPanel.Visibility = Visibility.Collapsed;
                    if (YearBookFilterWrapPanel.IsEnabled)
                    {
                        YearBookConditionFilterComboBox.SelectedItem = null;
                        YearBookMaskedTextBox.Text = "";
                    }
                }
            }
            else if (BookTurnOnFilter.IsChecked == true) // Если нажата кнопка "Включить фильтрацию"
            {
                // GroupBox фильтрации каталогов делаем недоступным
                CatalogFilterGroupBox.IsEnabled = false;

                // Показываем фильтры для таблицы книг
                NameBookFilterWrapPanel.Visibility = Visibility.Visible;
                GenreBookFilterWrapPanel.Visibility = Visibility.Visible;
                AuthorBookFilterWrapPanel.Visibility = Visibility.Visible;
                YearBookFilterWrapPanel.Visibility = Visibility.Visible;
            }
        }

        #region NameFilter

        private void NameBookSqlLikeSearch(object sender, TextChangedEventArgs e) // Поиск книги по названию
        {
            using (LibraryModelContainer dbContainer = new LibraryModelContainer())
            {
                if (NameBookTextBox.Text == "") // Если TextBox поиска книг по названию пустой
                {
                    GenreBookFilterWrapPanel.IsEnabled = true;
                    AuthorBookFilterWrapPanel.IsEnabled = true;
                    YearBookFilterWrapPanel.IsEnabled = true;

                    switch (SelectedCatalogCheckBox.IsChecked)
                    {
                        // Вывод в таблицу всех существующих книг, которые принадлежат выбранному каталогу
                        case true:
                        {
                            Catalog selectedCatalog = dbContainer.Catalogs.Find(((Catalog)CatalogDataGrid.SelectedItem)?.Id);

                            IQueryable<Book> result = dbContainer.Books.Where(c => c.Catalog.Id == selectedCatalog.Id);

                            BookDataGrid.ItemsSource = result.Include(c => c.Genre).Include(c => c.Catalog).ToList();
                            break;
                        }
                        // Вывод в таблицу всех существующих книг в базе данных
                        case false:
                        {
                            BookDataGrid.ItemsSource = dbContainer.Books.Include(c => c.Genre).Include(c => c.Catalog).ToList();
                            break;
                        }
                    }
                }
                else // Если TextBox поиска книг по названию имеет текст
                {
                    switch (SelectedCatalogCheckBox.IsChecked)
                    {
                        // Поиск книг по названию для выбранного каталога
                        case true:
                        {
                            Catalog selectedCatalog = dbContainer.Catalogs.Find(((Catalog)CatalogDataGrid.SelectedItem)?.Id);

                            IQueryable<Book> result = dbContainer.Books.Where(c => 
                                c.Name.Contains(NameBookTextBox.Text) && 
                                c.Catalog.Id == selectedCatalog.Id);

                            BookDataGrid.ItemsSource = result.Include(c => c.Genre).Include(c => c.Catalog).ToList();
                            break;
                        }
                        // Поиск книг по названию в базе данных
                        case false:
                        {
                            IQueryable<Book> result = dbContainer.Books.Where(c => c.Name.Contains(NameBookTextBox.Text));

                            BookDataGrid.ItemsSource = result.Include(c => c.Genre).Include(c => c.Catalog).ToList();
                            break;
                        }
                    }

                    GenreBookFilterWrapPanel.IsEnabled = false;
                    AuthorBookFilterWrapPanel.IsEnabled = false;
                    YearBookFilterWrapPanel.IsEnabled = false;
                }
            }
        }

        #endregion

        #region GenreFilter

        private void GenreBookComboBoxSelection(object sender, SelectionChangedEventArgs e) // Выборка жанра книги из ComboBox для поиска
        {
            if (GenreBookComboBox.SelectedItem == null) // Если не выбран жанр в ComboBox для поиска книг
            {
                NameReaderTextBox.IsEnabled = true;
                StatusReaderSearchButton.IsEnabled = false;
                StatusReaderCancelSearchButton.IsEnabled = false;
            }
            else // Если выбран какой-либо жанр в ComboBox для поиска книг
            {
                NameReaderTextBox.IsEnabled = false;
                StatusReaderSearchButton.IsEnabled = true;
                StatusReaderCancelSearchButton.IsEnabled = true;
            }
        }

        private void GenreBookSearchButton_Click(object sender, RoutedEventArgs e) // Поиск книги по жанру
        {
            using (LibraryModelContainer dbContainer = new LibraryModelContainer())
            {
                switch (SelectedCatalogCheckBox.IsChecked)
                {
                    // Поиск книг по выбранному жанру для выбранного каталога
                    case true:
                    {
                        Catalog selectedCatalog = dbContainer.Catalogs.Find(((Catalog)CatalogDataGrid.SelectedItem)?.Id);
                        
                        IQueryable<Book> result = dbContainer.Books.Where(c => 
                            c.Genre.Name == GenreBookComboBox.Text &&
                            c.Catalog.Id == selectedCatalog.Id);

                        BookDataGrid.ItemsSource = result.Include(c => c.Genre).Include(c => c.Catalog).ToList();
                        break;
                    }
                    // Поиск книг по выбранному жанру в базе данных
                    case false:
                    {
                        IQueryable<Book> result = dbContainer.Books.Where(c => c.Genre.Name == GenreBookComboBox.Text);

                        BookDataGrid.ItemsSource = result.Include(c => c.Genre).Include(c => c.Catalog).ToList();
                        break;
                    }
                }
            }
        }

        private void GenreBookCancelSearchButton_Click(object sender, RoutedEventArgs e) // Отмена поиска книги по жанру
        {
            using (LibraryModelContainer dbContainer = new LibraryModelContainer())
            {
                GenreBookComboBox.SelectedItem = null;

                switch (SelectedCatalogCheckBox.IsChecked)
                {
                    // Вывод в таблицу всех существующих книг, которые принадлежат выбранному каталогу
                    case true:
                    {
                        Catalog selectedCatalog = dbContainer.Catalogs.Find(((Catalog)CatalogDataGrid.SelectedItem)?.Id);
                        
                        IQueryable<Book> result = dbContainer.Books.Where(c => c.Catalog.Id == selectedCatalog.Id);

                        BookDataGrid.ItemsSource = result.Include(c => c.Genre).Include(c => c.Catalog).ToList();                        
                        break;
                    }
                    // Вывод в таблицу всех существующих книг в базе данных
                    case false:
                    {
                        BookDataGrid.ItemsSource = dbContainer.Books.Include(c => c.Genre).Include(c => c.Catalog).ToList();
                        break;
                    }
                }
            }
        }

        #endregion

        #region AuthorFilter

        private void AuthorBookSqlLikeSearch(object sender, TextChangedEventArgs e) // Поиск книги по автору
        {
            using (LibraryModelContainer dbContainer = new LibraryModelContainer())
            {
                if (AuthorBookTextBox.Text == "") // Если TextBox поиска книг по автору пустой
                {
                    NameBookFilterWrapPanel.IsEnabled = true;
                    GenreBookFilterWrapPanel.IsEnabled = true;
                    YearBookFilterWrapPanel.IsEnabled = true;

                    switch (SelectedCatalogCheckBox.IsChecked)
                    {
                        // Вывод в таблицу всех существующих книг, которые принадлежат выбранному каталогу
                        case true:
                        {
                            Catalog selectedCatalog = dbContainer.Catalogs.Find(((Catalog)CatalogDataGrid.SelectedItem)?.Id);
                        
                            IQueryable<Book> result = dbContainer.Books.Where(c => c.Catalog.Id == selectedCatalog.Id);

                            BookDataGrid.ItemsSource = result.Include(c => c.Genre).Include(c => c.Catalog).ToList();  
                            break;
                        }
                        // Вывод в таблицу всех существующих книг в базе данных
                        case false:
                        {
                            BookDataGrid.ItemsSource = dbContainer.Books.Include(c => c.Genre).Include(c => c.Catalog).ToList();
                            break;
                        }
                    } 
                }
                else // Если TextBox поиска книг по автору имеет текст
                {
                    switch (SelectedCatalogCheckBox.IsChecked)
                    {
                        // Поиск книг по автору для выбранного каталога
                        case true:
                        {
                            Catalog selectedCatalog = dbContainer.Catalogs.Find(((Catalog)CatalogDataGrid.SelectedItem)?.Id);
                        
                            IQueryable<Book> result = dbContainer.Books.Where(c => 
                                c.Author.Contains(AuthorBookTextBox.Text) && 
                                c.Catalog.Id == selectedCatalog.Id);

                            BookDataGrid.ItemsSource = result.Include(c => c.Genre).Include(c => c.Catalog).ToList();
                            break;
                        }
                        // Поиск книг по автору в базе данных
                        case false:
                        {
                            IQueryable<Book> result = dbContainer.Books.Where(c => c.Author.Contains(AuthorBookTextBox.Text));

                            BookDataGrid.ItemsSource = result.Include(c => c.Genre).Include(c => c.Catalog).ToList();
                            break;
                        }
                    }

                    NameBookFilterWrapPanel.IsEnabled = false;
                    GenreBookFilterWrapPanel.IsEnabled = false;
                    YearBookFilterWrapPanel.IsEnabled = false;
                }
            }
        }

        #endregion

        #region YearFilter

        private void YearBookTextChanged(object sender, TextChangedEventArgs textChangedEventArgs) // Поиск книги по году издания
        {
            // Во время процесса инициализации окна, графические элементы могут быть не загружены физически, а значит объекты таких элементов нужно проверить на null
            if (BookDataGrid != null)
            {
                using (LibraryModelContainer dbContainer = new LibraryModelContainer())
                {
                    // Если введены все символы в маске и в ComboBox выбрано условие фильтрации
                    if (YearBookMaskedTextBox.IsMaskCompleted && YearBookConditionFilterComboBox.SelectedItem != null)
                    {
                        NameBookFilterWrapPanel.IsEnabled = false;
                        GenreBookFilterWrapPanel.IsEnabled = false;
                        AuthorBookFilterWrapPanel.IsEnabled = false;

                        switch (SelectedCatalogCheckBox.IsChecked)
                        {
                            // Поиск книг по году издания для выбранного каталога
                            case true:
                            {
                                Catalog selectedCatalog = dbContainer.Catalogs.Find(((Catalog)CatalogDataGrid.SelectedItem)?.Id);

                                switch (YearBookConditionFilterComboBox.SelectedIndex)
                                {
                                    case 0: // Если в ComboBox выбрано значение =
                                    {
                                        short tempYear = Convert.ToInt16(YearBookMaskedTextBox.Text);                                        

                                        IQueryable<Book> result = dbContainer.Books.Where(c => 
                                            c.Year == tempYear && 
                                            c.Catalog.Id == selectedCatalog.Id);

                                        BookDataGrid.ItemsSource = result.Include(c => c.Genre).Include(c => c.Catalog).ToList();
                                        break;
                                    }
                                    case 1: // Если в ComboBox выбрано значение ≠
                                    {
                                        short tempYear = Convert.ToInt16(YearBookMaskedTextBox.Text);

                                        IQueryable<Book> result = dbContainer.Books.Where(c => 
                                            c.Year != tempYear && 
                                            c.Catalog.Id == selectedCatalog.Id);

                                        BookDataGrid.ItemsSource = result.Include(c => c.Genre).Include(c => c.Catalog).ToList();
                                        break;
                                    }
                                    case 2: // Если в ComboBox выбрано значение >
                                    {
                                        short tempYear = Convert.ToInt16(YearBookMaskedTextBox.Text);

                                        IQueryable<Book> result = dbContainer.Books.Where(c => 
                                            c.Year > tempYear && 
                                            c.Catalog.Id == selectedCatalog.Id);

                                        BookDataGrid.ItemsSource = result.Include(c => c.Genre).Include(c => c.Catalog).ToList();
                                        break;
                                    }
                                    case 3: // Если в ComboBox выбрано значение <
                                    {
                                        short tempYear = Convert.ToInt16(YearBookMaskedTextBox.Text);

                                        IQueryable<Book> result = dbContainer.Books.Where(c => 
                                            c.Year < tempYear && 
                                            c.Catalog.Id == selectedCatalog.Id);

                                        BookDataGrid.ItemsSource = result.Include(c => c.Genre).Include(c => c.Catalog).ToList();
                                        break;
                                    }
                                    case 4: // Если в ComboBox выбрано значение >=
                                    {
                                        short tempYear = Convert.ToInt16(YearBookMaskedTextBox.Text);

                                        IQueryable<Book> result = dbContainer.Books.Where(c => 
                                            c.Year >= tempYear && 
                                            c.Catalog.Id == selectedCatalog.Id);

                                        BookDataGrid.ItemsSource = result.Include(c => c.Genre).Include(c => c.Catalog).ToList();
                                        break;
                                    }
                                    case 5: // Если в ComboBox выбрано значение <=
                                    {
                                        short tempYear = Convert.ToInt16(YearBookMaskedTextBox.Text);

                                        IQueryable<Book> result = dbContainer.Books.Where(c => 
                                            c.Year <= tempYear && 
                                            c.Catalog.Id == selectedCatalog.Id);

                                        BookDataGrid.ItemsSource = result.Include(c => c.Genre).Include(c => c.Catalog).ToList();
                                        break;
                                    }
                                }

                                break;
                            }
                            // Поиск книг по году издания в базе данных
                            case false:
                            {
                                switch (YearBookConditionFilterComboBox.SelectedIndex)
                                {
                                    case 0: // Если в ComboBox выбрано значение =
                                    {
                                        short tempYear = Convert.ToInt16(YearBookMaskedTextBox.Text);
                                        IQueryable<Book> result = dbContainer.Books.Where(c => c.Year == tempYear);
                                        BookDataGrid.ItemsSource = result.Include(c => c.Genre).Include(c => c.Catalog).ToList();
                                        break;
                                    }
                                    case 1: // Если в ComboBox выбрано значение ≠
                                    {
                                        short tempYear = Convert.ToInt16(YearBookMaskedTextBox.Text);
                                        IQueryable<Book> result = dbContainer.Books.Where(c => c.Year != tempYear);
                                        BookDataGrid.ItemsSource = result.Include(c => c.Genre).Include(c => c.Catalog).ToList();
                                        break;
                                    }
                                    case 2: // Если в ComboBox выбрано значение >
                                    {
                                        short tempYear = Convert.ToInt16(YearBookMaskedTextBox.Text);
                                        IQueryable<Book> result = dbContainer.Books.Where(c => c.Year > tempYear);
                                        BookDataGrid.ItemsSource = result.Include(c => c.Genre).Include(c => c.Catalog).ToList();
                                        break;
                                    }
                                    case 3: // Если в ComboBox выбрано значение <
                                    {
                                        short tempYear = Convert.ToInt16(YearBookMaskedTextBox.Text);
                                        IQueryable<Book> result = dbContainer.Books.Where(c => c.Year < tempYear);
                                        BookDataGrid.ItemsSource = result.Include(c => c.Genre).Include(c => c.Catalog).ToList();
                                        break;
                                    }
                                    case 4: // Если в ComboBox выбрано значение >=
                                    {
                                        short tempYear = Convert.ToInt16(YearBookMaskedTextBox.Text);
                                        IQueryable<Book> result = dbContainer.Books.Where(c => c.Year >= tempYear);
                                        BookDataGrid.ItemsSource = result.Include(c => c.Genre).Include(c => c.Catalog).ToList();
                                        break;
                                    }
                                    case 5: // Если в ComboBox выбрано значение <=
                                    {
                                        short tempYear = Convert.ToInt16(YearBookMaskedTextBox.Text);
                                        IQueryable<Book> result = dbContainer.Books.Where(c => c.Year <= tempYear);
                                        BookDataGrid.ItemsSource = result.Include(c => c.Genre).Include(c => c.Catalog).ToList();
                                        break;
                                    }
                                }

                                break;
                            }
                        }
                    }
                    else // Если маска ввода полностью не заполнена
                    {
                        NameBookFilterWrapPanel.IsEnabled = true;
                        GenreBookFilterWrapPanel.IsEnabled = true;
                        AuthorBookFilterWrapPanel.IsEnabled = true;

                        switch (SelectedCatalogCheckBox.IsChecked)
                        {
                            case true:
                            {
                                Catalog selectedCatalog = dbContainer.Catalogs.Find(((Catalog)CatalogDataGrid.SelectedItem)?.Id);

                                IQueryable<Book> result = dbContainer.Books.Where(c => c.Catalog.Id == selectedCatalog.Id);

                                BookDataGrid.ItemsSource = result.Include(c => c.Genre).Include(c => c.Catalog).ToList(); 
                                break;
                            }
                            case false:
                            {
                                BookDataGrid.ItemsSource = dbContainer.Books.Include(c => c.Genre).Include(c => c.Catalog).ToList();
                                break;
                            }
                        }
                    }
                }
            }
        }

        private void YearBookConditionFiltersChanged(object sender, SelectionChangedEventArgs e)
        {
            using (LibraryModelContainer dbContainer = new LibraryModelContainer())
            {
                if (YearBookMaskedTextBox.IsMaskCompleted) // Если введены все символы в маске
                {
                    switch (SelectedCatalogCheckBox.IsChecked)
                    {
                        // Поиск книг по году издания для выбранного каталога
                        case true:
                        {
                            Catalog selectedCatalog = dbContainer.Catalogs.Find(((Catalog)CatalogDataGrid.SelectedItem)?.Id);

                            switch (YearBookConditionFilterComboBox.SelectedIndex)
                            {
                                case 0: // Если в ComboBox выбрано значение =
                                {
                                    short tempYear = Convert.ToInt16(YearBookMaskedTextBox.Text);

                                    IQueryable<Book> result = dbContainer.Books.Where(c => 
                                        c.Year == tempYear && 
                                        c.Catalog.Id == selectedCatalog.Id);

                                    BookDataGrid.ItemsSource = result.Include(c => c.Genre).Include(c => c.Catalog).ToList();
                                    break;
                                }
                                case 1: // Если в ComboBox выбрано значение ≠
                                {
                                    short tempYear = Convert.ToInt16(YearBookMaskedTextBox.Text);

                                    IQueryable<Book> result = dbContainer.Books.Where(c => 
                                        c.Year != tempYear && 
                                        c.Catalog.Id == selectedCatalog.Id);

                                    BookDataGrid.ItemsSource = result.Include(c => c.Genre).Include(c => c.Catalog).ToList();
                                    break;
                                }
                                case 2: // Если в ComboBox выбрано значение >
                                {
                                    short tempYear = Convert.ToInt16(YearBookMaskedTextBox.Text);

                                    IQueryable<Book> result = dbContainer.Books.Where(c => 
                                        c.Year > tempYear && 
                                        c.Catalog.Id == selectedCatalog.Id);

                                    BookDataGrid.ItemsSource = result.Include(c => c.Genre).Include(c => c.Catalog).ToList();
                                    break;
                                }
                                case 3: // Если в ComboBox выбрано значение <
                                {
                                    short tempYear = Convert.ToInt16(YearBookMaskedTextBox.Text);

                                    IQueryable<Book> result = dbContainer.Books.Where(c => 
                                        c.Year < tempYear && 
                                        c.Catalog.Id == selectedCatalog.Id);

                                    BookDataGrid.ItemsSource = result.Include(c => c.Genre).Include(c => c.Catalog).ToList();
                                    break;
                                }
                                case 4: // Если в ComboBox выбрано значение >=
                                {
                                    short tempYear = Convert.ToInt16(YearBookMaskedTextBox.Text);

                                    IQueryable<Book> result = dbContainer.Books.Where(c => 
                                        c.Year >= tempYear && 
                                        c.Catalog.Id == selectedCatalog.Id);

                                    BookDataGrid.ItemsSource = result.Include(c => c.Genre).Include(c => c.Catalog).ToList();
                                    break;
                                }
                                case 5: // Если в ComboBox выбрано значение <=
                                {
                                    short tempYear = Convert.ToInt16(YearBookMaskedTextBox.Text);

                                    IQueryable<Book> result = dbContainer.Books.Where(c => 
                                        c.Year <= tempYear && 
                                        c.Catalog.Id == selectedCatalog.Id);

                                    BookDataGrid.ItemsSource = result.Include(c => c.Genre).Include(c => c.Catalog).ToList();
                                    break;
                                }
                            }

                            break;
                        }
                        // Поиск книг по автору в базе данных
                        case false:
                        {
                            switch (YearBookConditionFilterComboBox.SelectedIndex)
                            {
                                case 0: // Если в ComboBox выбрано значение =
                                {
                                    short tempYear = Convert.ToInt16(YearBookMaskedTextBox.Text);
                                    IQueryable<Book> result = dbContainer.Books.Where(c => c.Year == tempYear);
                                    BookDataGrid.ItemsSource = result.Include(c => c.Genre).Include(c => c.Catalog).ToList();
                                    break;
                                }
                                case 1: // Если в ComboBox выбрано значение ≠
                                {
                                    short tempYear = Convert.ToInt16(YearBookMaskedTextBox.Text);
                                    IQueryable<Book> result = dbContainer.Books.Where(c => c.Year != tempYear);
                                    BookDataGrid.ItemsSource = result.Include(c => c.Genre).Include(c => c.Catalog).ToList();
                                    break;
                                }
                                case 2: // Если в ComboBox выбрано значение >
                                {
                                    short tempYear = Convert.ToInt16(YearBookMaskedTextBox.Text);
                                    IQueryable<Book> result = dbContainer.Books.Where(c => c.Year > tempYear);
                                    BookDataGrid.ItemsSource = result.Include(c => c.Genre).Include(c => c.Catalog).ToList();
                                    break;
                                }
                                case 3: // Если в ComboBox выбрано значение <
                                {
                                    short tempYear = Convert.ToInt16(YearBookMaskedTextBox.Text);
                                    IQueryable<Book> result = dbContainer.Books.Where(c => c.Year < tempYear);
                                    BookDataGrid.ItemsSource = result.Include(c => c.Genre).Include(c => c.Catalog).ToList();
                                    break;
                                }
                                case 4: // Если в ComboBox выбрано значение >=
                                {
                                    short tempYear = Convert.ToInt16(YearBookMaskedTextBox.Text);
                                    IQueryable<Book> result = dbContainer.Books.Where(c => c.Year >= tempYear);
                                    BookDataGrid.ItemsSource = result.Include(c => c.Genre).Include(c => c.Catalog).ToList();
                                    break;
                                }
                                case 5: // Если в ComboBox выбрано значение <=
                                {
                                    short tempYear = Convert.ToInt16(YearBookMaskedTextBox.Text);
                                    IQueryable<Book> result = dbContainer.Books.Where(c => c.Year <= tempYear);
                                    BookDataGrid.ItemsSource = result.Include(c => c.Genre).Include(c => c.Catalog).ToList();
                                    break;
                                }
                            }

                            break;
                        }
                    }
                }
            }
        }

        private void YearBookCancelSearchButton_Click(object sender, RoutedEventArgs e) // Отмена поиска книги по году издания
        {
            YearBookConditionFilterComboBox.SelectedItem = null;
            YearBookMaskedTextBox.Text = "";
        }

        #endregion

        #endregion

        // Основные операции по работе с таблицей "Книги"
        #region BookDataGrid functions

        private void CreateBookButton_Click(object sender, RoutedEventArgs e) // Создание новой книги в системе
        {
            using (LibraryModelContainer dbContainer = new LibraryModelContainer())
            {
                BookWindow window = new BookWindow(true, null, _currentUser);

                window.ShowDialog();

                BookDataGrid.ItemsSource = dbContainer.Books.Include(c => c.Genre).Include(c => c.Catalog).ToList();
                EntityRecordDataGrid.ItemsSource = dbContainer.EntityRecords.Include(c => c.Entity).Include(c => c.CreatedBy).Include(c => c.ModifiedBy).ToList();
            }
        }

        private void UpdateBookButton_Click(object sender, RoutedEventArgs e) // Редактирование существующей книги в системе
        {
            using (LibraryModelContainer dbContainer = new LibraryModelContainer())
            {
                Book book = dbContainer.Books.Include(c => c.Genre).Include(c => c.Catalog).FirstOrDefault(c => c.Id == ((Book)BookDataGrid.SelectedItem).Id);
                if (book != null)
                {
                    BookWindow window = new BookWindow(false, book, _currentUser);
                    window.ShowDialog();
                }

                dbContainer.Entry(book).Reload();
                BookDataGrid.ItemsSource = dbContainer.Books.Include(c => c.Genre).Include(c => c.Catalog).ToList();
                EntityRecordDataGrid.ItemsSource = dbContainer.EntityRecords.Include(c => c.Entity).Include(c => c.CreatedBy).Include(c => c.ModifiedBy).ToList();
            }
        }

        private void DeleteBookButton_Click(object sender, RoutedEventArgs e) // Удаление существующей книги в системе
        {
            using (LibraryModelContainer dbContainer = new LibraryModelContainer())
            {
                Book selectedBook = dbContainer.Books.Find(((Book)BookDataGrid.SelectedItem)?.Id);
                EntityRecord record = dbContainer.EntityRecords.First(c => c.Book.Id == selectedBook.Id); // Находим запись нужной книги для редактирования в таблице EntityRecords
                User user = dbContainer.Users.Find(_currentUser.Id);

                if (selectedBook != null)
                {
                    record.ModifiedBy = user;
                    record.State = "Удалён";

                    // Создание коллекции историй изменений записи книги для таблицы EntityHistories
                    IList<EntityHistory> historyRecordList = new List<EntityHistory>
                    {
                        new EntityHistory
                        {
                            Id = Guid.NewGuid(),
                            FieldName = "Название",
                            OldValue = selectedBook.Name,
                            NewValue = null,
                            Date = DateTime.Now,
                            User = user,
                            EntityRecord = record
                        },
                        new EntityHistory
                        {
                            Id = Guid.NewGuid(),
                            FieldName = "Жанр",
                            OldValue = selectedBook.Genre.Name,
                            NewValue = null,
                            Date = DateTime.Now,
                            User = user,
                            EntityRecord = record
                        },
                        new EntityHistory
                        {
                            Id = Guid.NewGuid(),
                            FieldName = "Автор",
                            OldValue = selectedBook.Author,
                            NewValue = null,
                            Date = DateTime.Now,
                            User = user,
                            EntityRecord = record
                        },
                        new EntityHistory
                        {
                            Id = Guid.NewGuid(),
                            FieldName = "Год издания",
                            OldValue = selectedBook.Year.ToString(),
                            NewValue = null,
                            Date = DateTime.Now,
                            User = user,
                            EntityRecord = record
                        },
                        new EntityHistory
                        {
                            Id = Guid.NewGuid(),
                            FieldName = "Каталог",
                            OldValue = selectedBook.Catalog.Name,
                            NewValue = null,
                            Date = DateTime.Now,
                            User = user,
                            EntityRecord = record
                        }
                    };
                    dbContainer.EntityHistories.AddRange(historyRecordList);

                    dbContainer.Books.Remove(selectedBook);

                    dbContainer.SaveChanges();

                    BookDataGrid.ItemsSource = dbContainer.Books.Include(c => c.Genre).Include(c => c.Catalog).ToList();
                    EntityRecordDataGrid.ItemsSource = dbContainer.EntityRecords.Include(c => c.Entity).Include(c => c.CreatedBy).Include(c => c.ModifiedBy).ToList();
                    MessageBox.Show($"Книга: {selectedBook.Name}. Выбранная книга успешно удалена из системы.");
                }
            }
        }

        private void BookDataGridSelectionChanged(object sender, SelectionChangedEventArgs e) // Смена выборки книг в таблице CatalogDataGrid
        {
            using (LibraryModelContainer dbContainer = new LibraryModelContainer())
            {
                Book book = dbContainer.Books.Find(((Book) BookDataGrid.SelectedItem)?.Id);
                if (book != null)
                {
                    UpdateBookButton.IsEnabled = true;
                    DeleteBookButton.IsEnabled = true;
                }
                else
                {
                    UpdateBookButton.IsEnabled = false;
                    DeleteBookButton.IsEnabled = false;
                }
            }
        }

        private void BookDataGridCancelSelection_DoubleClick(object sender, MouseButtonEventArgs e) // Снимает выборку книги в таблице BookDataGrid
        {
            BookDataGrid.SelectedItem = null;
        }

        #endregion

        // Фильтры для таблиц "Чёрный список читателей" и "Таблица читателей" (вспомогательная)
        #region BlackList and Reader datagrids filters

        private void ReaderBlackListDataGridSearchFilter(object sender, RoutedEventArgs e) // Включение фильтров поиска для таблиц "Читатели" и "Чёрный список читателей"
        {
            if (ReaderForAdminDataGridFilterCheckBox.IsChecked == true) // Если включён фильтр поиска "Таблица читателей"
            {
                NameReaderBlackListFilterWrapPanel.IsEnabled = true;
                AddRemoveDeleteBlackListReaderFilterWrapPanel.IsEnabled = true;
                BlackListDataGridFilterCheckBox.IsEnabled = false;
            }
            else if (BlackListDataGridFilterCheckBox.IsChecked == true) // Если включён фильтр поиска "Чёрный список читателей"
            {
                NameReaderBlackListFilterWrapPanel.IsEnabled = true;
                ReaderForAdminDataGridFilterCheckBox.IsEnabled = false;
            }
            else
            {
                NameReaderBlackListFilterWrapPanel.IsEnabled = false;
                AddRemoveDeleteBlackListReaderFilterWrapPanel.IsEnabled = false;
                ReaderForAdminDataGridFilterCheckBox.IsEnabled = true;
                BlackListDataGridFilterCheckBox.IsEnabled = true;
                NameReaderBlackListTextBox.Text = "";
            }
        }

        private void NameReaderBlackListSqlLikeSearch(object sender, TextChangedEventArgs e) // Поиск читателя по ФИО в таблицах "Читатели" и "Чёрный список читателей"
        {
            using (LibraryModelContainer dbContainer = new LibraryModelContainer())
            {
                if (ReaderForAdminDataGridFilterCheckBox.IsChecked == true) // Если включён фильтр поиска "Таблица читателей"
                {
                    IQueryable<Reader> result = dbContainer.Readers.Where(c => c.Name.Contains(NameReaderBlackListTextBox.Text)); // LINQ. Like SQL запрос в БД по ФИО читателя
                    ReaderForAdminDataGrid.ItemsSource = result.ToList();
                }
                else if (BlackListDataGridFilterCheckBox.IsChecked == true) // Если включён фильтр поиска "Чёрный список читателей"
                {
                    IQueryable<BlackList> result = dbContainer.BlackLists.Where(c => c.Name.Contains(NameReaderBlackListTextBox.Text)); // LINQ. Like SQL запрос в БД по ФИО читателя, оказавшимся в чёрном списке
                    BlackListDataGrid.ItemsSource = result.ToList();
                }
                else // Если ни один из фильтров поиска не был включён
                {
                    ReaderForAdminDataGrid.ItemsSource = dbContainer.Readers.ToList();
                    BlackListDataGrid.ItemsSource = dbContainer.BlackLists.ToList();
                }
            }
        }

        private void AddRemoveDeleteReaderForAdminDataGridFilter(object sender, RoutedEventArgs e)
        {
            using (LibraryModelContainer dbContainer = new LibraryModelContainer())
            {
                if (AddReaderInBlackListFilter.IsChecked == true) // Если включён фильтр поиска "Добавить читателя в чёрный список"
                {
                    RemoveReaderFromBlackListFilter.IsEnabled = false;
                    DeleteReaderFromSystemFilter.IsEnabled = false;

                    IQueryable<Reader> result = dbContainer.Readers.Where(c => c.Status == "Активен" && c.Blocked);
                    ReaderForAdminDataGrid.ItemsSource = result.ToList();
                }
                else if (RemoveReaderFromBlackListFilter.IsChecked == true) // Если включён фильтр поиска "Убрать читателя из чёрного списка"
                {
                    AddReaderInBlackListFilter.IsEnabled = false;
                    DeleteReaderFromSystemFilter.IsEnabled = false;

                    IQueryable<Reader> result = dbContainer.Readers.Where(c => c.Status == "Заблокирован" && c.Blocked == false);
                    ReaderForAdminDataGrid.ItemsSource = result.ToList();
                }
                else if (DeleteReaderFromSystemFilter.IsChecked == true) // Если включён фильтр поиска "Удалить читателя из системы"
                {
                    AddReaderInBlackListFilter.IsEnabled = false;
                    RemoveReaderFromBlackListFilter.IsEnabled = false;

                    IQueryable<Reader> result = dbContainer.Readers.Where(c => c.Removed);
                    ReaderForAdminDataGrid.ItemsSource = result.ToList();
                }
                else // Если ни один из фильтров поиска не был включён
                {
                    AddReaderInBlackListFilter.IsEnabled = true;
                    RemoveReaderFromBlackListFilter.IsEnabled = true;
                    DeleteReaderFromSystemFilter.IsEnabled = true;

                    ReaderForAdminDataGrid.ItemsSource = dbContainer.Readers.ToList();
                }
            }
        }

        #endregion

        // Основные операции по работе с таблицами "Чёрный список читателей" и "Таблица читателей" (вспомогательная)
        #region BlackList and Reader datagrids functions

        // Таблица читателей
        #region ReaderForAdminDataGrid

        private void ReaderForAdminDataGridSelectionChanged(object sender, SelectionChangedEventArgs e) // Выбор читателей в таблице читателей во вкладке "Чёрный список читателей"
        {
            using (LibraryModelContainer dbContainer = new LibraryModelContainer())
            {
                IList<Reader> readerList = ReaderForAdminDataGrid.SelectedItems.Cast<Reader>().ToList(); // Коллекция выбранных читателей

                if (readerList.Count > 0) // Если количество выбранных читателей в коллекции > 0
                {
                    if (readerList.Count == 1) // Если количество выбранных читателей в коллекции == 1
                    {
                        Reader reader = dbContainer.Readers.Find(readerList[0].Id);
                        IQueryable<BlackList> result = dbContainer.BlackLists.Where(c => c.Reader.Id == reader.Id);
                        BlackListDataGrid.ItemsSource = result.ToList();
                    }
                    else // Если количество выбранных читателей в коллекции != 1
                    {
                        BlackListDataGrid.ItemsSource = dbContainer.BlackLists.ToList();
                    }

                    foreach (Reader reader in readerList)
                    {
                        if (reader.Status == "Активен" && reader.Blocked) // Если у читателя статус "Активен" и стоит признак "Заблокирован"
                        {
                            AddReaderInBlackListButton.IsEnabled = true; // Кнопка "Добавить читателя в чёрный список" доступна для нажатия
                        }
                        else if (reader.Removed)
                        {
                            DeleteReaderFromSystemButton.IsEnabled = true; // Кнопка "Удалить читателя из системы" доступна для нажатия
                        }
                        else // Если у читателя другой статус и не стоит признак "Заблокирован"
                        {
                            AddReaderInBlackListButton.IsEnabled = false; // Кнопка "Добавить читателя в чёрный список" не доступна для нажатия
                            DeleteReaderFromSystemButton.IsEnabled = false; // Кнопка "Удалить читателя из системы" не доступна для нажатия

                            break;
                        }
                    }
                }
                else // Если количество выбранных читателей в коллекции == 0
                {
                    DeleteReaderFromSystemButton.IsEnabled = false;
                    AddReaderInBlackListButton.IsEnabled = false;

                    BlackListDataGrid.ItemsSource = dbContainer.BlackLists.ToList();
                }
            }
        }

        private void ReaderForAdminDataGridCancelSelection_DoubleClick(object sender, MouseButtonEventArgs e) // Снимает выборку читателя в ReaderForAdminDataGrid
        {
            ReaderForAdminDataGrid.SelectedItem = null;
        }

        private void DeleteReaderFromSystemButton_Click(object sender, RoutedEventArgs e) // Удаление читателя из системы
        {
            using (LibraryModelContainer dbContainer = new LibraryModelContainer())
            {
                User user = dbContainer.Users.Find(_currentUser.Id); // Ищем текущего авторизованного пользователя системы

                IList<Reader> readerList = new List<Reader>();
                foreach (var item in ReaderForAdminDataGrid.SelectedItems) // Формирование коллекции выбранных читателей
                {
                    Reader reader = dbContainer.Readers.Find(((Reader)item)?.Id);
                    readerList.Add(reader);
                }

                IList<Reader> oldeReaderList = readerList;

                dbContainer.Readers.RemoveRange(readerList);

                foreach (var itemReader in oldeReaderList)
                {
                    // Находим запись нужного читателя для редактирования в таблице EntityRecords
                    EntityRecord recordReader = dbContainer.EntityRecords.First(c => c.Reader.Id == itemReader.Id);
                    recordReader.ModifiedBy = user;
                    recordReader.State = "Удалён";

                    // Создание коллекции историй изменений записи читателя для таблицы EntityHistories
                    IList<EntityHistory> historyRecordList = new List<EntityHistory>
                    {
                        new EntityHistory
                        {
                            Id = Guid.NewGuid(),
                            FieldName = "ФИО",
                            OldValue = itemReader.Name,
                            NewValue = null,
                            Date = DateTime.Now,
                            User = user,
                            EntityRecord = recordReader
                        },
                        new EntityHistory
                        {
                            Id = Guid.NewGuid(),
                            FieldName = "Статус",
                            OldValue = itemReader.Status,
                            NewValue = null,
                            Date = DateTime.Now,
                            User = user,
                            EntityRecord = recordReader
                        },
                        new EntityHistory
                        {
                            Id = Guid.NewGuid(),
                            FieldName = "Заблокирован",
                            OldValue = itemReader.Blocked ? "Да" : "Нет",
                            NewValue = null,
                            Date = DateTime.Now,
                            User = user,
                            EntityRecord = recordReader
                        },
                        new EntityHistory
                        {
                            Id = Guid.NewGuid(),
                            FieldName = "Удалён",
                            OldValue = itemReader.Removed ? "Да" : "Нет",
                            NewValue = null,
                            Date = DateTime.Now,
                            User = user,
                            EntityRecord = recordReader
                        }
                    };
                    dbContainer.EntityHistories.AddRange(historyRecordList);

                    if (itemReader.BlackList != null)
                    {
                        // Находим запись нужного читателя в чёрном списке для редактирования в таблице EntityRecords
                        EntityRecord recordBlackList = dbContainer.EntityRecords.Find(itemReader.BlackList.Id);

                        if (recordBlackList != null)
                        {
                            recordBlackList.ModifiedBy = user;
                            recordBlackList.State = "Удалён";
                        }
                        
                        // Создание коллекции историй изменений записи читателей в чёрном списке для таблицы EntityHistories
                        IList<EntityHistory> historyRecordBlackList = new List<EntityHistory>
                        {
                            new EntityHistory
                            {
                                Id = Guid.NewGuid(),
                                FieldName = "ФИО читателя",
                                OldValue = itemReader.BlackList.Name,
                                NewValue = null,
                                Date = DateTime.Now,
                                User = user,
                                EntityRecord = recordBlackList
                            },
                            new EntityHistory
                            {
                                Id = Guid.NewGuid(),
                                FieldName = "Дата создания",
                                OldValue = null,
                                NewValue = itemReader.BlackList.Date.ToString("F", new CultureInfo("ru-RU")),
                                Date = DateTime.Now,
                                User = user,
                                EntityRecord = recordBlackList
                            }
                        };
                        dbContainer.EntityHistories.AddRange(historyRecordBlackList);
                    }

                    if (itemReader.Orders.Count != 0)
                    {
                        foreach (Order itemReaderOrder in itemReader.Orders)
                        {
                            EntityRecord recordOrder = dbContainer.EntityRecords.Find(itemReaderOrder.Id);

                            if (recordOrder != null)
                            {
                                recordOrder.ModifiedBy = user;
                                recordOrder.State = "Удалён";
                            }

                            // Создание коллекции историй изменений записи читателей в чёрном списке для таблицы EntityHistories
                            IList<EntityHistory> historyRecordOrderList = new List<EntityHistory>
                            {
                                new EntityHistory
                                {
                                    Id = Guid.NewGuid(),
                                    FieldName = "Номер",
                                    OldValue = itemReaderOrder.Number.ToString(),
                                    NewValue = null,
                                    Date = DateTime.Now,
                                    User = user,
                                    EntityRecord = recordOrder
                                },
                                new EntityHistory
                                {
                                    Id = Guid.NewGuid(),
                                    FieldName = "Дата регистрации",
                                    OldValue = itemReaderOrder.RegisteredOn.ToString("F", new CultureInfo("ru-RU")),
                                    NewValue = null,
                                    Date = DateTime.Now,
                                    User = user,
                                    EntityRecord = recordOrder
                                },
                                new EntityHistory
                                {
                                    Id = Guid.NewGuid(),
                                    FieldName = "Дата закрытия",
                                    OldValue = itemReaderOrder.DeadlineDate.ToString("F", new CultureInfo("ru-RU")),
                                    NewValue = null,
                                    Date = DateTime.Now,
                                    User = user,
                                    EntityRecord = recordOrder
                                },
                                new EntityHistory
                                {
                                    Id = Guid.NewGuid(),
                                    FieldName = "Фактическое закрытие",
                                    OldValue = itemReaderOrder.ClosureDate?.ToString("F", new CultureInfo("ru-RU")),
                                    NewValue = null,
                                    Date = DateTime.Now,
                                    User = user,
                                    EntityRecord = recordOrder
                                }
                            };
                            dbContainer.EntityHistories.AddRange(historyRecordOrderList);
                        }
                    }
                }

                dbContainer.SaveChanges();

                ReaderForAdminDataGrid.ItemsSource = dbContainer.Readers.ToList();
                BlackListDataGrid.ItemsSource = dbContainer.BlackLists.ToList();
                ReaderDataGrid.ItemsSource = dbContainer.Readers.ToList();
                OrderDataGrid.ItemsSource = dbContainer.Orders.ToList();
                EntityRecordDataGrid.ItemsSource = dbContainer.EntityRecords.Include(c => c.Entity).Include(c => c.CreatedBy).Include(c => c.ModifiedBy).ToList();

                MessageBox.Show("Выбранные читатели успешно удалены из системы.");
            }
        }

        private void AddReaderInBlackListButton_Click(object sender, RoutedEventArgs e) // Добавление читателя в чёрный список
        {
            using (LibraryModelContainer dbContainer = new LibraryModelContainer())
            {
                Entity entity = dbContainer.Entities.First(c => c.Name == "Чёрный список");
                User user = dbContainer.Users.Find(_currentUser.Id); // Ищем текущего авторизованного пользователя системы

                IList<Reader> readerList = new List<Reader>();
                foreach (var item in ReaderForAdminDataGrid.SelectedItems) // Формирование коллекции выбранных читателей
                {
                    Reader reader = dbContainer.Readers.Find(((Reader)item)?.Id);
                    readerList.Add(reader);
                }

                foreach (var itemReader in readerList)
                {
                    BlackList blackList = new BlackList
                    {
                        Id = Guid.NewGuid(),
                        Name = itemReader.Name,
                        Date = DateTime.Now,
                        Reader = itemReader
                    };
                    dbContainer.BlackLists.Add(blackList);

                    // Создание записи для таблицы EntityRecords
                    EntityRecord record = new EntityRecord
                    {
                        Id = Guid.NewGuid(),
                        Entity = entity,
                        Name = blackList.Name,
                        State = "Добавлен",
                        CreatedBy = user,
                        ModifiedBy = null,
                        BlackList = blackList
                    };
                    dbContainer.EntityRecords.Add(record);

                    // Создание коллекции историй изменений записи читателей в чёрном списке для таблицы EntityHistories
                    IList<EntityHistory> historyRecordList = new List<EntityHistory>
                    {
                        new EntityHistory
                        {
                            Id = Guid.NewGuid(),
                            FieldName = "ФИО читателя",
                            OldValue = null,
                            NewValue = blackList.Name,
                            Date = DateTime.Now,
                            User = user,
                            EntityRecord = record
                        },
                        new EntityHistory
                        {
                            Id = Guid.NewGuid(),
                            FieldName = "Дата создания",
                            OldValue = null,
                            NewValue = blackList.Date.ToString("F", new CultureInfo("ru-RU")),
                            Date = DateTime.Now,
                            User = user,
                            EntityRecord = record
                        }
                    };
                    dbContainer.EntityHistories.AddRange(historyRecordList);

                    itemReader.Status = "Заблокирован";

                    // Находим запись нужного читателя для редактирования в таблице EntityRecords
                    EntityRecord recordReader = dbContainer.EntityRecords.First(c => c.Reader.Id == itemReader.Id);
                    recordReader.ModifiedBy = user;
                    recordReader.State = "Изменён";

                    CreateEntityHistory("Статус", "Активен", "Заблокирован", recordReader);
                }

                dbContainer.SaveChanges();

                ReaderForAdminDataGrid.ItemsSource = dbContainer.Readers.ToList();
                BlackListDataGrid.ItemsSource = dbContainer.BlackLists.ToList();
                ReaderDataGrid.ItemsSource = dbContainer.Readers.ToList();
                EntityRecordDataGrid.ItemsSource = dbContainer.EntityRecords.Include(c => c.Entity).Include(c => c.CreatedBy).Include(c => c.ModifiedBy).ToList();

                MessageBox.Show("Выбранные читатели успешно добавлены в чёрный список.");
            }
        }

        #endregion

        // Чёрный список читателей
        #region BlackListDataGrid

        private void BlackListDataGridSelectionChanged(object sender, SelectionChangedEventArgs e) // Выбор читателя в таблице чёрного списка во вкладке "Чёрный список читателей"
        {
            using (LibraryModelContainer dbContainer = new LibraryModelContainer())
            {
                BlackList blackList = dbContainer.BlackLists.Find(((BlackList) BlackListDataGrid.SelectedItem)?.Id);
                DeleteReaderFromBlackListButton.IsEnabled = blackList != null;
            }
        }

        private void BlackListDataGridCancelSelection_DoubleClick(object sender, MouseButtonEventArgs e) // Снимает выборку читателя, оказавшимся в чёрном списке
        {
            BlackListDataGrid.SelectedItem = null;
        }

        private void DeleteReaderFromBlackListButton_Click(object sender, RoutedEventArgs e) // Удалить читателя из чёрного списка
        {
            using (LibraryModelContainer dbContainer = new LibraryModelContainer())
            {
                BlackList selectedBlackList = dbContainer.BlackLists.Find(((BlackList)BlackListDataGrid.SelectedItem)?.Id);
                EntityRecord record = dbContainer.EntityRecords.First(c => c.BlackList.Id == selectedBlackList.Id); // Находим запись нужного читателя в чёрном списке для редактирования в таблице EntityRecords
                User user = dbContainer.Users.Find(_currentUser.Id);

                if (selectedBlackList != null)
                {
                    if (selectedBlackList.Reader.Blocked)
                    {
                        MessageBox.Show("Читателя нельзя исключить из чёрного списка, поскольку он до сих пор находится в состоянии \"Заблокирован\".");
                    }
                    else
                    {
                        record.ModifiedBy = user;
                        record.State = "Удалён";

                        // Создание коллекции историй изменений записи читателей в чёрном списке для таблицы EntityHistories
                        IList<EntityHistory> historyRecordBlackList = new List<EntityHistory>
                        {
                            new EntityHistory
                            {
                                Id = Guid.NewGuid(),
                                FieldName = "ФИО читателя",
                                OldValue = selectedBlackList.Name,
                                NewValue = null,
                                Date = DateTime.Now,
                                User = user,
                                EntityRecord = record
                            },
                            new EntityHistory
                            {
                                Id = Guid.NewGuid(),
                                FieldName = "Дата создания",
                                OldValue = selectedBlackList.Date.ToString("F", new CultureInfo("ru-RU")),
                                NewValue = null,
                                Date = DateTime.Now,
                                User = user,
                                EntityRecord = record
                            }
                        };
                        dbContainer.EntityHistories.AddRange(historyRecordBlackList);

                        Reader reader = dbContainer.Readers.Find(selectedBlackList.Reader.Id);
                        if (reader != null)
                        {
                            reader.Status = "Активный";

                            // Находим запись нужного читателя для редактирования в таблице EntityRecords
                            EntityRecord recordReader = dbContainer.EntityRecords.First(c => c.Reader.Id == reader.Id);
                            recordReader.ModifiedBy = user;
                            recordReader.State = "Изменён";

                            CreateEntityHistory("Статус", "Заблокирован", "Активен", record);
                        }

                        dbContainer.BlackLists.Remove(selectedBlackList);
                        dbContainer.SaveChanges();

                        ReaderForAdminDataGrid.ItemsSource = dbContainer.Readers.ToList();
                        BlackListDataGrid.ItemsSource = dbContainer.BlackLists.ToList();
                        ReaderDataGrid.ItemsSource = dbContainer.Readers.ToList();
                        EntityRecordDataGrid.ItemsSource = dbContainer.EntityRecords.Include(c => c.Entity).Include(c => c.CreatedBy).Include(c => c.ModifiedBy).ToList();

                        MessageBox.Show($"Читатель в чёрном списке: {selectedBlackList.Name}. Указанный читатель успешно исключён из чёрного списка.");
                    }
                }
            }
        }

        #endregion

        #endregion

        // Справочник "Жанры"
        #region GenreDirectory

        private void GenreDirectoryCancelSelection_DoubleClick(object sender, MouseButtonEventArgs e) // Снимает выборку жанра в ListBox
        {
            GenreDirectoryListBox.SelectedItem = null;
        }

        private void GenreDirectorySelectionChanged(object sender, SelectionChangedEventArgs e) // Выборка одного жанра в ListBox
        {
            // Если выбран режим изменения существующего жанра и выбран жанр в ListBox
            if (GenreDirectoryUpdateModeRadioButton.IsChecked == true && GenreDirectoryListBox.SelectedItem != null)
            {
                GenreDirectoryActionButton.IsEnabled = true;
                NameGenreDirectoryTextBox.Text = ((Genre) GenreDirectoryListBox.SelectedItem).Name;
            }

            if (GenreDirectoryDeleteModeRadioButton.IsChecked == true && GenreDirectoryListBox.SelectedItem != null)
            {
                GenreDirectoryActionButton.IsEnabled = true;
            }
        }

        private void GenreDirectoryControlModeChecked(object sender, RoutedEventArgs e) // Выборка режима управления в справочнике жанров. Метод нажатия кнопок у компонента RadioButton
        {
            if (GenreDirectoryTurnOffOperationRadioButton.IsChecked == true) // Если выбран режим вывода списка жанров
            {
                // Во время процесса инициализации окна, графические элементы могут быть не загружены физически. Поскольку графический элемент — это объект какого-либо класса,
                // следовательно, если графический элемент физически не загружен, то значение объекта этого элемента = null, а значит, к такому объекту нельзя обращаться через
                // его свойства и нельзя для этого объекта через свойства указывать какие-либо значения. Поэтому для текущего RadioButton делаем проверку IsLoaded.
                if (GenreDirectoryTurnOffOperationRadioButton.IsLoaded)
                {
                    GenreDirectoryActionButton.Visibility = Visibility.Collapsed; // Убираем кнопку
                    NameGenreDirectoryTextBox.IsEnabled = false; // Текстовую строку жанра делаем недоступным для ввода текста
                    NameGenreDirectoryTextBox.Text = ""; // Убираем текст в текстовом строке жанра
                }
            }
            else if (GenreDirectoryCreateModeRadioButton.IsChecked == true) // Если выбран режим добавления нового жанра
            {
                GenreDirectoryActionButton.Visibility = Visibility.Visible; // Показываем кнопку
                GenreDirectoryActionButton.Content = "Добавить запись"; // Меняем подпись у кнопки
                NameGenreDirectoryTextBox.IsEnabled = true; // Текстовую строку жанра делаем доступным для ввода текста
                GenreDirectoryListBox.SelectedItem = null; // Убираем выборку жанра в справочнике

                if (NameGenreDirectoryTextBox.Text != "") // Если TextBox названия жанра имеет текст
                {
                    GenreDirectoryActionButton.IsEnabled = true;
                }

                // Делаем фильтрацию при переходе режимов от редактирования к добавлению
                using (LibraryModelContainer dbContainer = new LibraryModelContainer())
                {
                    IQueryable<Genre> result = dbContainer.Genres.Where(c => c.Name.Contains(NameGenreDirectoryTextBox.Text));
                    GenreDirectoryListBox.ItemsSource = result.ToList();
                }
            }
            else if (GenreDirectoryUpdateModeRadioButton.IsChecked == true) // Если выбран режим изменения существующего жанра
            {
                GenreDirectoryActionButton.Visibility = Visibility.Visible; // Показываем кнопку
                GenreDirectoryActionButton.IsEnabled = false; // Делаем кнопку недоступной для нажатия
                GenreDirectoryActionButton.Content = "Редактировать запись"; // Меняем подпись у кнопки
                NameGenreDirectoryTextBox.IsEnabled = true; // Текстовую строку жанра делаем доступным для ввода текста

                if (GenreDirectoryListBox.SelectedItem != null) // Если выбран жанр в ListBox
                {
                    NameGenreDirectoryTextBox.Text = ((Genre)GenreDirectoryListBox.SelectedItem).Name;
                    GenreDirectoryActionButton.IsEnabled = true; // Делаем кнопку доступной для нажатия
                }
            }
            else if (GenreDirectoryDeleteModeRadioButton.IsChecked == true) // Если выбран режим удаления существующего жанра
            {
                GenreDirectoryActionButton.Visibility = Visibility.Visible; // Показываем кнопку
                GenreDirectoryActionButton.IsEnabled = false; // Делаем кнопку недоступной для нажатия
                GenreDirectoryActionButton.Content = "Удалить запись"; // Меняем подпись у кнопки
                NameGenreDirectoryTextBox.IsEnabled = true; // Текстовую строку жанра делаем доступным для ввода текста
                GenreDirectoryListBox.SelectedItem = null; // Убираем выборку жанра в справочнике

                // Делаем фильтрацию при переходе режимов от редактирования к удалению
                using (LibraryModelContainer dbContainer = new LibraryModelContainer())
                {
                    IQueryable<Genre> result = dbContainer.Genres.Where(c => c.Name.Contains(NameGenreDirectoryTextBox.Text));
                    GenreDirectoryListBox.ItemsSource = result.ToList();
                }
            }
        }

        private void GenreDirectoryActionButton_Click(object sender, RoutedEventArgs e) // Метод нажатия кнопки в справочнике жанров
        {
            using (LibraryModelContainer dbContainer = new LibraryModelContainer())
            {
                Entity entity = dbContainer.Entities.First(c => c.Name == "Жанр");
                User user = dbContainer.Users.Find(_currentUser.Id);

                if (GenreDirectoryCreateModeRadioButton.IsChecked == true) // Если выбран режим добавления новой записи в справочник жанров
                {
                    if (NameGenreDirectoryTextBox.Text.Length <= 50) // Если поле название имеет длину текста <= 50 символов
                    {
                        // Создание жанра для таблицы Genres
                        Genre genre = new Genre
                        {
                            Id = Guid.NewGuid(),
                            Name = NameGenreDirectoryTextBox.Text
                        };

                        dbContainer.Genres.Add(genre);

                        // Создание записи для таблицы EntityRecords
                        EntityRecord record = new EntityRecord
                        {
                            Id = Guid.NewGuid(),
                            Entity = entity,
                            Name = genre.Name,
                            State = "Добавлен",
                            CreatedBy = user,
                            ModifiedBy = null,
                            Genre = genre
                        };
                        dbContainer.EntityRecords.Add(record);

                        // Создание коллекции историй изменений записи жанра для таблицы EntityHistories
                        IList<EntityHistory> historyRecordList = new List<EntityHistory>
                        {
                            new EntityHistory
                            {
                                Id = Guid.NewGuid(),
                                FieldName = "Название",
                                OldValue = null,
                                NewValue = genre.Name,
                                Date = DateTime.Now,
                                User = user,
                                EntityRecord = record
                            }
                        };
                        dbContainer.EntityHistories.AddRange(historyRecordList);

                        dbContainer.SaveChanges();

                        MessageBox.Show($"Жанр: {NameGenreDirectoryTextBox.Text}. Запись в справочник успешно добавлена.");
                        NameGenreDirectoryTextBox.Text = "";
                    }
                    else // Если поле название имеет длину текста > 50 символов
                    {
                        MessageBox.Show("Поле название. Длина текста не должна превышать больше 50 символов.");
                    }
                }
                else if (GenreDirectoryUpdateModeRadioButton.IsChecked == true) // Если выбран режим изменеия существующей записи в справочнике жанров
                {
                    if (NameGenreDirectoryTextBox.Text.Length <= 50) // Если поле название имеет длину текста <= 50 символов
                    {
                        Genre selectedGenre = dbContainer.Genres.Find(((Genre)GenreDirectoryListBox.SelectedItem)?.Id);
                        Genre oldGenre = selectedGenre;
                        EntityRecord record = dbContainer.EntityRecords.First(c => c.Genre.Id == oldGenre.Id); // Находим запись нужного жанра для редактирования в таблице EntityRecords

                        if (selectedGenre != null)
                        {
                            selectedGenre.Name = NameGenreDirectoryTextBox.Text;

                            if (oldGenre.Name == selectedGenre.Name)
                            {
                                MessageBox.Show("Информация о выбранном жанре не изменилась.");
                            }
                            else
                            {
                                // Меняем значения у объекта типа EntityRecord
                                record.ModifiedBy = user;
                                record.State = "Изменён";

                                if (oldGenre.Name != selectedGenre.Name) // Если изменилось название у жанра
                                {
                                    record.Name = selectedGenre.Name;

                                    CreateEntityHistory("Название", oldGenre.Name, selectedGenre.Name, record);
                                }

                                dbContainer.SaveChanges();

                                BookDataGrid.ItemsSource = dbContainer.Books.ToList(); // Обновляем выводимые данные книг в BookDataGrid после изменения жанра в справочнике

                                MessageBox.Show($"Жанр: {selectedGenre.Name}. Запись в справочнике успешно изменена. Старое значение: {oldGenre.Name}.");
                                NameGenreDirectoryTextBox.Text = "";
                            }
                        }
                    }
                    else // Если поле название имеет длину текста > 50 символов
                    {
                        MessageBox.Show("Поле название. Длина текста не должна превышать больше 50 символов.");
                    }
                    
                }
                else if (GenreDirectoryDeleteModeRadioButton.IsChecked == true) // Если выбран режим удаления существующей записи из справочника жанров
                {
                    Genre selectedGenre = dbContainer.Genres.Find(((Genre)GenreDirectoryListBox.SelectedItem)?.Id);
                    Genre oldGenre = selectedGenre;
                    EntityRecord record = dbContainer.EntityRecords.First(c => c.Genre.Id == oldGenre.Id); // Находим запись нужного жанра для редактирования в таблице EntityRecords

                    if (selectedGenre != null)
                    {
                        record.ModifiedBy = user;
                        record.State = "Удалён";

                        // Создание коллекции историй изменений записи жанра для таблицы EntityHistories
                        IList<EntityHistory> historyRecordList = new List<EntityHistory>
                        {
                            new EntityHistory
                            {
                                Id = Guid.NewGuid(),
                                FieldName = "Название",
                                OldValue = selectedGenre.Name,
                                NewValue = null,
                                Date = DateTime.Now,
                                User = user,
                                EntityRecord = record
                            }
                        };
                        dbContainer.EntityHistories.AddRange(historyRecordList);

                        foreach (Book itemBook in selectedGenre.Books) // Удаляем информацию о тех кнингах в системе, где был удалён выбранный жанр
                        {
                            // Находим запись нужной книги удалённого жанра для удаления в таблице EntityRecords
                            EntityRecord recordBook = dbContainer.EntityRecords.First(c => c.Book.Id == itemBook.Id);

                            recordBook.ModifiedBy = user;
                            recordBook.State = "Удалён";

                            // Создание коллекции историй изменений записи книги для таблицы EntityHistories
                            IList<EntityHistory> historyRecordBookList = new List<EntityHistory>
                            {
                                new EntityHistory
                                {
                                    Id = Guid.NewGuid(),
                                    FieldName = "Название",
                                    OldValue = itemBook.Name,
                                    NewValue = null,
                                    Date = DateTime.Now,
                                    User = user,
                                    EntityRecord = recordBook
                                },
                                new EntityHistory
                                {
                                    Id = Guid.NewGuid(),
                                    FieldName = "Жанр",
                                    OldValue = itemBook.Genre.Name,
                                    NewValue = null,
                                    Date = DateTime.Now,
                                    User = user,
                                    EntityRecord = recordBook
                                },
                                new EntityHistory
                                {
                                    Id = Guid.NewGuid(),
                                    FieldName = "Автор",
                                    OldValue = itemBook.Author,
                                    NewValue = null,
                                    Date = DateTime.Now,
                                    User = user,
                                    EntityRecord = recordBook
                                },
                                new EntityHistory
                                {
                                    Id = Guid.NewGuid(),
                                    FieldName = "Год издания",
                                    OldValue = itemBook.Year.ToString(),
                                    NewValue = null,
                                    Date = DateTime.Now,
                                    User = user,
                                    EntityRecord = recordBook
                                },
                                new EntityHistory
                                {
                                    Id = Guid.NewGuid(),
                                    FieldName = "Каталог",
                                    OldValue = itemBook.Catalog?.Name,
                                    NewValue = null,
                                    Date = DateTime.Now,
                                    User = user,
                                    EntityRecord = recordBook
                                }
                            };
                            dbContainer.EntityHistories.AddRange(historyRecordBookList);
                        }

                        dbContainer.Genres.Remove(selectedGenre);

                        dbContainer.SaveChanges();

                        BookDataGrid.ItemsSource = dbContainer.Books.ToList(); // Обновляем выводимые данные книг в BookDataGrid после удаления жанра из справочника

                        MessageBox.Show($"Жанр: {oldGenre.Name}. Запись из справочника успешно удалена.");

                        // Костыль. Если при удалении выбранного жанра из ListBox в текстовом поле поиска не набран текст, то обновляем ListBox после удаления
                        if (NameGenreDirectoryTextBox.Text == "")
                        {
                            GenreDirectoryListBox.ItemsSource = dbContainer.Genres.ToList();
                            EntityRecordDataGrid.ItemsSource = dbContainer.EntityRecords.Include(c => c.Entity).Include(c => c.CreatedBy).Include(c => c.ModifiedBy).ToList();
                        }
                        else
                        {
                            NameGenreDirectoryTextBox.Text = "";
                        }
                    }
                }
            }
        }

        private void NameGenreDirectoryTextChanged(object sender, TextChangedEventArgs e) // Поиск жанра в справочнике
        {
            using (LibraryModelContainer dbContainer = new LibraryModelContainer())
            {
                if (NameGenreDirectoryTextBox.Text != "") // Если TextBox названия жанра имеет текст
                {
                    // Если выбран режим добавления нового жанра в справочник
                    if (GenreDirectoryCreateModeRadioButton.IsChecked == true)
                    {
                        GenreDirectoryActionButton.IsEnabled = true;

                        IQueryable<Genre> result = dbContainer.Genres.Where(c => c.Name.Contains(NameGenreDirectoryTextBox.Text));
                        GenreDirectoryListBox.ItemsSource = result.ToList();
                        EntityRecordDataGrid.ItemsSource = dbContainer.EntityRecords.Include(c => c.Entity).Include(c => c.CreatedBy).Include(c => c.ModifiedBy).ToList();
                    }
                    // Если выбран режим удаления существующего жанра из справочника
                    else if (GenreDirectoryDeleteModeRadioButton.IsChecked == true)
                    {
                        IQueryable<Genre> result = dbContainer.Genres.Where(c => c.Name.Contains(NameGenreDirectoryTextBox.Text));
                        GenreDirectoryListBox.ItemsSource = result.ToList();
                        EntityRecordDataGrid.ItemsSource = dbContainer.EntityRecords.Include(c => c.Entity).Include(c => c.CreatedBy).Include(c => c.ModifiedBy).ToList();
                    }
                    // Иначе выбран режим изменения существующего жанра в справочнике. Если не выбран жанр в ListBox.
                    // Эту проверку нужно делать для того, чтобы не ломался живой поиск жанра в справочнике
                    else if (GenreDirectoryListBox.SelectedItem == null)
                    {
                        IQueryable<Genre> result = dbContainer.Genres.Where(c => c.Name.Contains(NameGenreDirectoryTextBox.Text));
                        GenreDirectoryListBox.ItemsSource = result.ToList();
                        EntityRecordDataGrid.ItemsSource = dbContainer.EntityRecords.Include(c => c.Entity).Include(c => c.CreatedBy).Include(c => c.ModifiedBy).ToList();
                    }
                }
                else // Если TextBox названия жанра пустое
                {
                    GenreDirectoryActionButton.IsEnabled = false;
                    GenreDirectoryListBox.ItemsSource = dbContainer.Genres.ToList();
                    EntityRecordDataGrid.ItemsSource = dbContainer.EntityRecords.Include(c => c.Entity).Include(c => c.CreatedBy).Include(c => c.ModifiedBy).ToList();
                }
            }
        }

        #endregion

        // Фильтры для таблицы "Пользователи"
        #region UserDataGrid filters

        private void UserRadioButtonChecked(object sender, RoutedEventArgs e) // Фильтрация пользователей системы. Метод нажатия кнопок у компонента RadioButton
        {
            if (UserTurnOffFilter.IsChecked == true) // Если нажата кнопка "Выключить фильтрацию"
            {
                // Во время процесса инициализации окна, графические элементы могут быть не загружены физически. Поскольку графический элемент — это объект какого-либо класса,
                // следовательно, если графический элемент физически не загружен, то значение объекта этого элемента = null, а значит, к такому объекту нельзя обращаться через
                // его свойства и нельзя для этого объекта через свойства указывать какие-либо значения. Поэтому для текущего RadioButton делаем проверку IsLoaded.
                if (UserTurnOffFilter.IsLoaded)
                {
                    ChangeLogFilterGroupBox.IsEnabled = true; // GroupBox фильтрации журнала делаем доступным

                    // Скрываем фильтры для таблицы пользователей системы
                    UserSearchFilterWrapPanel.Visibility = Visibility.Collapsed;
                    if (NameUserFilterCheckBox.IsChecked == true)
                    {
                        NameUserFilterCheckBox.IsChecked = false;
                    }
                    else if (LoginUserFilterCheckBox.IsChecked == true)
                    {
                        LoginUserFilterCheckBox.IsChecked = false;
                    }

                    UserTextSearchWrapPanel.Visibility = Visibility.Collapsed;

                    SelectedUserTurnOnFilterWrapPanel.Visibility = Visibility.Collapsed;
                    if (SelectedUserCheckBox.IsChecked == true)
                    {
                        SelectedUserCheckBox.IsChecked = false;
                        ChangeLogFilterGroupBox.IsEnabled = true;
                    }
                }
            }
            else if (UserTurnOnFilter.IsChecked == true) // Если нажата кнопка "Включить фильтрацию"
            {
                ChangeLogFilterGroupBox.IsEnabled = false; // GroupBox фильтрации журнала делаем недоступным

                // Показываем фильтры для таблицы пользователей системы
                UserSearchFilterWrapPanel.Visibility = Visibility.Visible;
                UserTextSearchWrapPanel.Visibility = Visibility.Visible;
                SelectedUserTurnOnFilterWrapPanel.Visibility = Visibility.Visible;
            }
        }

        // Фильтрация журнала последних действий над записями в базе данных для выбранного пользователя системы
        private void EntityRecordSearchTutnOnFilterForSelectedUser(object sender, RoutedEventArgs e)
        {
            switch (SelectedUserCheckBox.IsChecked)
            {
                // Включена фильтрация журнала последних действий над записями в базе данных для выбранного пользователя системы
                case true:
                {
                    UserDataGrid.IsEnabled = false;
                    CreateUserButton.IsEnabled = false;
                    DeleteUserButton.IsEnabled = false;
                    UpdateUserButton.IsEnabled = false;
                    UserSearchFilterWrapPanel.IsEnabled = false;
                    UserTextSearchWrapPanel.IsEnabled = false;

                    ChangeLogFilterGroupBox.IsEnabled = true;
                    break;
                }
                // Выключена фильтрация журнала последних действий над записями в базе данных для выбранного пользователя системы
                case false:
                {
                    using (LibraryModelContainer dbContainer = new LibraryModelContainer())
                    {
                        User currentUser = dbContainer.Users.Find(_currentUser.Id);
                        User selectedUser = dbContainer.Users.Find(((User)UserDataGrid.SelectedItem)?.Id);

                        // Если текущий авторизованный пользователь системы по ID совпадает с выбранным пользователем системы 
                        if (currentUser != null && selectedUser != null && currentUser.Id == selectedUser.Id)
                        {
                            UserDataGrid.IsEnabled = true;
                            CreateUserButton.IsEnabled = true;
                            DeleteUserButton.IsEnabled = false;
                            UpdateUserButton.IsEnabled = true;
                            UserSearchFilterWrapPanel.IsEnabled = true;
                            UserTextSearchWrapPanel.IsEnabled = true;

                            ChangeLogFilterGroupBox.IsEnabled = false;
                        }
                        else
                        {
                            UserDataGrid.IsEnabled = true;
                            CreateUserButton.IsEnabled = true;
                            DeleteUserButton.IsEnabled = true;
                            UpdateUserButton.IsEnabled = true;
                            UserSearchFilterWrapPanel.IsEnabled = true;
                            UserTextSearchWrapPanel.IsEnabled = true;

                            ChangeLogFilterGroupBox.IsEnabled = false;
                        }
                    }
                    
                    break;
                }
            }
        }

        private void UserSearchFilterChecked(object sender, RoutedEventArgs e) // Включение CheckBoxes-фильтров поиска для таблицы "Пользователи системы" 
        {
            if (NameUserFilterCheckBox.IsChecked == true) // Если включён фильтр поиска по ФИО пользователя
            {
                UserSearchTextBox.IsEnabled = true;
                LoginUserFilterCheckBox.IsEnabled = false;
            }
            else if (LoginUserFilterCheckBox.IsChecked == true) // Если включён фильтр поиска по Login пользователя
            {
                UserSearchTextBox.IsEnabled = true;
                NameUserFilterCheckBox.IsEnabled = false;
            }
            else // Если не включен фильтр поиска для текстового поля
            {
                UserSearchTextBox.IsEnabled = false;
                NameUserFilterCheckBox.IsEnabled = true;
                LoginUserFilterCheckBox.IsEnabled = true;
                UserSearchTextBox.Text = "";
            }
        }

        private void UserSqlLikeSearch(object sender, TextChangedEventArgs e) // Поиск пользователя системы по ФИО или его Login
        {
            using (LibraryModelContainer dbContainer = new LibraryModelContainer())
            {
                if (NameUserFilterCheckBox.IsChecked == true && UserSearchTextBox.Text != "") // Если выбран фильтр поиска пользователя системы по ФИО
                {
                    IQueryable<User> result = dbContainer.Users.Where(c => c.Name.Contains(UserSearchTextBox.Text)); // LINQ. Like SQL запрос в БД по ФИО пользователя системы
                    UserDataGrid.ItemsSource = result.ToList();
                }
                else if (LoginUserFilterCheckBox.IsChecked == true && UserSearchTextBox.Text != "") // Если выбран фильтр поиска пользователя системы по его Login
                {
                    IQueryable<User> result = dbContainer.Users.Where(c => c.Login.Contains(UserSearchTextBox.Text)); // LINQ. Like SQL запрос в БД по Login пользователя системы
                    UserDataGrid.ItemsSource = result.ToList();
                }
                else // Если не выбран фильтр поиска пользователя системы
                {
                    UserDataGrid.ItemsSource = dbContainer.Users.ToList();
                }
            }
        }

        #endregion

        // Основные операции по работе с таблицей "Пользователи"
        #region UserDataGrid functions

        private void UserDataGridSelectionChanged(object sender, SelectionChangedEventArgs e) // Выборка пользователя системы в UserDataGrid
        {
            using (LibraryModelContainer dbContainer = new LibraryModelContainer())
            {
                User selectedUser = dbContainer.Users.Find(((User)UserDataGrid.SelectedItem)?.Id);

                if (selectedUser != null)
                {
                    SelectedUserCheckBox.IsEnabled = true;

                    IQueryable<EntityRecord> result = dbContainer.EntityRecords.Include(c => c.EntityHistories).Where(c =>
                        c.CreatedBy.Id == selectedUser.Id || c.ModifiedBy.Id == selectedUser.Id || c.EntityHistories.Any(k => k.User.Id == selectedUser.Id));

                    EntityRecordDataGrid.ItemsSource = result.Include(c => c.Entity).ToList();
                }
                else
                {
                    SelectedUserCheckBox.IsEnabled = false;

                    EntityRecordDataGrid.ItemsSource = dbContainer.EntityRecords.Include(c => c.Entity).Include(c => c.CreatedBy).Include(c => c.ModifiedBy).ToList();
                    EntityHistoryDataGrid.ItemsSource = null;
                }

                User currentUser = dbContainer.Users.Find(_currentUser.Id);

                if (UserDataGrid.SelectedItem != null && currentUser != null) // Если выбран пользователь системы
                {
                    if (((User)UserDataGrid.SelectedItem)?.Id == currentUser.Id) // Если выбранный пользователь в таблице по ID совпадает с авторизованным пользователем системы
                    {
                        UpdateUserButton.IsEnabled = true;
                    }
                    else
                    {
                        DeleteUserButton.IsEnabled = true;
                        UpdateUserButton.IsEnabled = true;
                    }
                }
                else // Если не выбран пользователь системы
                {
                    DeleteUserButton.IsEnabled = false;
                    UpdateUserButton.IsEnabled = false;
                }
            }
        }

        private void UserDataGridCancelSelection_DoubleClick(object sender, MouseButtonEventArgs e) // Снимает выборку пользователя системы в UserDataGrid
        {
            UserDataGrid.SelectedItem = null;
        }

        private void CreateUserButton_Click(object sender, RoutedEventArgs e)
        {
            using (LibraryModelContainer dbContainer = new LibraryModelContainer())
            {
                User currentUser = dbContainer.Users.Find(_currentUser.Id); // Текущий авторизованный пользователь

                UserWindow window = new UserWindow(true, null, currentUser);

                window.ShowDialog();

                if (currentUser != null && currentUser.UserRole == "Администратор")
                {
                    IQueryable<User> result = dbContainer.Users.Where(c =>
                        c.UserRole == "Библиотекарь" || c.Id == currentUser.Id); // Для пользователей с ролью "Администратор" ищем всех пользовталей с ролью "Библиотекарь" и самого себя

                    UserDataGrid.ItemsSource = result.ToList();
                }
                else
                {
                    UserDataGrid.ItemsSource = dbContainer.Users.ToList();
                }
            }
        }

        private void DeleteUserButton_Click(object sender, RoutedEventArgs e)
        {
            using (LibraryModelContainer dbContainer = new LibraryModelContainer())
            {
                User user = dbContainer.Users.Find(((User)UserDataGrid.SelectedItem)?.Id);
                if (user != null)
                {
                    string oldName = user.Name;

                    dbContainer.Users.Remove(user);
                    dbContainer.SaveChanges();

                    User currentUser = dbContainer.Users.Find(_currentUser.Id); // Текущий авторизованный пользователь
                    if (currentUser != null && currentUser.UserRole == "Администратор")
                    {
                        IQueryable<User> result = dbContainer.Users.Where(c =>
                            c.UserRole == "Библиотекарь" || c.Id == currentUser.Id); // Для пользователей с ролью "Администратор" ищем всех пользовталей с ролью "Библиотекарь" и самого себя

                        UserDataGrid.ItemsSource = result.ToList();
                        MessageBox.Show($"Пользователь системы: {oldName}. Пользователь системы успешно удалён.");
                    }
                    else
                    {
                        UserDataGrid.ItemsSource = dbContainer.Users.ToList();
                        MessageBox.Show($"Пользователь системы: {oldName}. Пользователь системы успешно удалён.");
                    }
                }
            }
        }

        private void UpdateUserButton_Click(object sender, RoutedEventArgs e)
        {
            using (LibraryModelContainer dbContainer = new LibraryModelContainer())
            {
                User currentUser = dbContainer.Users.Find(_currentUser.Id); // Текущий авторизованный пользователь

                User user = dbContainer.Users.Find(((User)UserDataGrid.SelectedItem)?.Id);
                if (user != null)
                {
                    UserWindow window = new UserWindow(false, user, currentUser);

                    window.ShowDialog();
                }

                if (currentUser != null && currentUser.UserRole == "Администратор")
                {
                    IQueryable<User> result = dbContainer.Users.Where(c =>
                        c.UserRole == "Библиотекарь" || c.Id == currentUser.Id); // Для пользователей с ролью "Администратор" ищем всех пользовталей с ролью "Библиотекарь" и самого себя

                    dbContainer.Entry(user).Reload();
                    UserDataGrid.ItemsSource = result.ToList();
                }
                else
                {
                    dbContainer.Entry(user).Reload();
                    UserDataGrid.ItemsSource = dbContainer.Users.ToList();
                }
            }
        }

        #endregion

        // Журнал последних действий пользователей системы
        #region Recent Activity Log

        #region EntityRecordDataGrid

        private void EntityRecordDataGridSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            using (LibraryModelContainer dbContainer = new LibraryModelContainer())
            {
                EntityRecord selectedRecord = dbContainer.EntityRecords.Find(((EntityRecord)EntityRecordDataGrid.SelectedItem)?.Id);

                if (selectedRecord != null)
                {
                    EntityHistoryFilterGroupBox.IsEnabled = true;

                    IQueryable<EntityHistory> resultHistories = dbContainer.EntityHistories.Where(c => c.EntityRecord.Id == selectedRecord.Id);

                    EntityHistoryDataGrid.ItemsSource = resultHistories.OrderBy(c => c.Date).Include(c => c.User).ToList();

                    DeleteEntityRecordButton.IsEnabled = selectedRecord.State == "Удалён";
                }
                else
                {
                    EntityHistoryFilterGroupBox.IsEnabled = false;

                    EntityHistoryDataGrid.ItemsSource = null;
                }
            }
        }

        private void EntityRecordDataGridCancelSelection_DoubleClick(object sender, MouseButtonEventArgs e)
        {
            EntityRecordDataGrid.SelectedItem = null;
        }

        private void DeleteEntityRecord_Click(object sender, RoutedEventArgs e)
        {
            using (LibraryModelContainer dbContainer = new LibraryModelContainer())
            {
                EntityRecord selectedRecord = dbContainer.EntityRecords.Find(((EntityRecord)EntityRecordDataGrid.SelectedItem)?.Id);

                if (selectedRecord != null)
                {
                    dbContainer.EntityRecords.Remove(selectedRecord);
                    dbContainer.SaveChanges();

                    EntityRecordDataGrid.ItemsSource = dbContainer.EntityRecords.Include(c => c.Entity).ToList();
                    MessageBox.Show($"Запись в журнале: {selectedRecord.Name}. Выбранная запись успешно удалена из системы.");
                }
            }
        }

        #endregion

        #region EntityRecordDataGrid filters

        private void ChangeLogRadioButtonChecked(object sender, RoutedEventArgs e) // Фильтрация журнала. Метод нажатия кнопок у компонента RadioButton
        {
            if (ChangeLogTurnOffFilter.IsChecked == true) // Если нажата кнопка "Выключить фильтрацию"
            {
                // Во время процесса инициализации окна, графические элементы могут быть не загружены физически. Поскольку графический элемент — это объект какого-либо класса,
                // следовательно, если графический элемент физически не загружен, то значение объекта этого элемента = null, а значит, к такому объекту нельзя обращаться через
                // его свойства и нельзя для этого объекта через свойства указывать какие-либо значения. Поэтому для текущего RadioButton делаем проверку IsLoaded.
                if (ChangeLogTurnOffFilter.IsLoaded)
                {
                    UserFilterGroupBox.IsEnabled = true; // GroupBox фильтрации пользователей системы делаем доступным

                    NameEntityRecordWrapPanel.Visibility = Visibility.Collapsed;
                    if (NameEntityRecordWrapPanel.IsEnabled)
                    {
                        NameEntityRecordTextBox.Text = "";
                    }

                    StateEntityRecordWrapPanel.Visibility = Visibility.Collapsed;
                    if (StateEntityRecordWrapPanel.IsEnabled)
                    {
                        StateEntityRecordComboBox.SelectedItem = null;
                    }

                    UserEntityRecordWrapPanel.Visibility = Visibility.Collapsed;
                    if (UserEntityRecordWrapPanel.IsEnabled)
                    {
                        if (CreatedByCheckBox.IsChecked == true)
                        {
                            CreatedByCheckBox.IsChecked = false;
                        }
                        else if (ModifiedByCheckBox.IsChecked == true)
                        {
                            ModifiedByCheckBox.IsChecked = false;
                        }
                    }

                    EntityHistoryFilterGroupBox.Visibility = Visibility.Collapsed;
                    if (EntityHistoryRecordCheckBox.IsChecked == true)
                    {
                        EntityHistoryRecordCheckBox.IsChecked = false;
                    }
                }
            }
            else if (ChangeLogTurnOnFilter.IsChecked == true) // Если нажата кнопка "Включить фильтрацию"
            {
                UserFilterGroupBox.IsEnabled = false; // GroupBox фильтрации пользователей системы делаем недоступным

                NameEntityRecordWrapPanel.Visibility = Visibility.Visible;
                StateEntityRecordWrapPanel.Visibility = Visibility.Visible;
                UserEntityRecordWrapPanel.Visibility = Visibility.Visible;
                EntityHistoryFilterGroupBox.Visibility = Visibility.Visible;
            }
        }

        private void EntityRecordNameSqlLikeSearch(object sender, TextChangedEventArgs e) // Поиск записей журнала последних действий по названию
        {
            using (LibraryModelContainer dbContainer = new LibraryModelContainer())
            {
                EntityHistoryDataGrid.ItemsSource = null;

                if (NameEntityRecordTextBox.Text != "") // Если TextBox поиска по названию имеет какой-либо текст
                {
                    switch (SelectedUserCheckBox.IsChecked)
                    {
                        // Поиск записей в таблице "Журнал последних действий пользователей системы" по названию записи для выбранного пользователя системы
                        case true:
                        {
                            User selectedUser = dbContainer.Users.Find(((User) UserDataGrid.SelectedItem)?.Id);

                            IQueryable<EntityRecord> result = dbContainer.EntityRecords
                                .Include(c => c.Entity)
                                .Include(c => c.CreatedBy)
                                .Include(c => c.ModifiedBy)
                                .Include(c => c.EntityHistories)
                                .Where(c => 
                                    c.Name.Contains(NameEntityRecordTextBox.Text) && 
                                    (c.CreatedBy.Id == selectedUser.Id || 
                                     c.ModifiedBy.Id == selectedUser.Id || 
                                     c.EntityHistories.Any(k => k.User.Id == selectedUser.Id)));

                            EntityRecordDataGrid.ItemsSource = result.Include(c => c.Entity).Include(c => c.CreatedBy).Include(c => c.ModifiedBy).ToList();
                            break;
                        }
                        // Поиск записей в таблице "Журнал последних действий пользователей системы" по названию записи в базе данных
                        case false:
                        {
                            IQueryable<EntityRecord> result = dbContainer.EntityRecords
                                .Include(c => c.Entity)
                                .Include(c => c.CreatedBy)
                                .Include(c => c.ModifiedBy)
                                .Where(c => c.Name.Contains(NameEntityRecordTextBox.Text));

                            EntityRecordDataGrid.ItemsSource = result.Include(c => c.Entity).Include(c => c.CreatedBy).Include(c => c.ModifiedBy).ToList();
                            break;
                        }
                    }

                    StateEntityRecordWrapPanel.IsEnabled = false;
                    UserEntityRecordWrapPanel.IsEnabled = false;
                }
                else // Если TextBox поиска по названию пустой
                {
                    switch (SelectedUserCheckBox.IsChecked)
                    {
                        // Вывод в таблицу "Журнал последних действий пользователей системы" всех существующих записей, которые принадлежат выбранному пользователю системы
                        case true:
                        {
                            User selectedUser = dbContainer.Users.Find(((User) UserDataGrid.SelectedItem)?.Id);

                            IQueryable<EntityRecord> result = dbContainer.EntityRecords.Include(c => c.EntityHistories).Where(c =>
                                c.CreatedBy.Id == selectedUser.Id || c.ModifiedBy.Id == selectedUser.Id || c.EntityHistories.Any(k => k.User.Id == selectedUser.Id));

                            EntityRecordDataGrid.ItemsSource = result.Include(c => c.Entity).Include(c => c.CreatedBy).Include(c => c.ModifiedBy).ToList();
                            break;
                        }
                        // Вывод в таблицу "Журнал последних действий пользователей системы" всех существующих записей в базе данных
                        case false:
                        {
                            EntityRecordDataGrid.ItemsSource = dbContainer.EntityRecords.Include(c => c.Entity).Include(c => c.CreatedBy).Include(c => c.ModifiedBy).ToList();
                            break;
                        }
                    }

                    StateEntityRecordWrapPanel.IsEnabled = true;
                    UserEntityRecordWrapPanel.IsEnabled = true;
                }
            }
        }

        private void EntityRecordStateComboBoxSelection(object sender, SelectionChangedEventArgs e) // Поиск записей журнала последних действий по состоянию
        {
            using (LibraryModelContainer dbContainer = new LibraryModelContainer())
            {
                EntityHistoryDataGrid.ItemsSource = null;

                if (StateEntityRecordComboBox.SelectedItem != null) // Если в ComboBox выбрано какое-либо состояние
                {
                    switch (SelectedUserCheckBox.IsChecked)
                    {
                        // Поиск записей в таблице "Журнал последних действий пользователей системы" по названию записи для выбранного пользователя системы
                        case true:
                        {
                            User selectedUser = dbContainer.Users.Find(((User) UserDataGrid.SelectedItem)?.Id);

                            IQueryable<EntityRecord> result = dbContainer.EntityRecords
                                .Include(c => c.Entity)
                                .Include(c => c.CreatedBy)
                                .Include(c => c.ModifiedBy)
                                .Include(c => c.EntityHistories)
                                .Where(c =>
                                    c.State == ((ContentControl)StateEntityRecordComboBox.SelectedItem).Content.ToString() && 
                                    (c.CreatedBy.Id == selectedUser.Id || 
                                     c.ModifiedBy.Id == selectedUser.Id || 
                                     c.EntityHistories.Any(k => k.User.Id == selectedUser.Id)));

                            EntityRecordDataGrid.ItemsSource = result.Include(c => c.Entity).Include(c => c.CreatedBy).Include(c => c.ModifiedBy).ToList();
                            break;
                        }
                        // Поиск записей в таблице "Журнал последних действий пользователей системы" по названию записи в базе данных
                        case false:
                        {
                            IQueryable<EntityRecord> result = dbContainer.EntityRecords
                                .Include(c => c.Entity)
                                .Include(c => c.CreatedBy)
                                .Include(c => c.ModifiedBy)
                                .Where(c => c.State == ((ContentControl)StateEntityRecordComboBox.SelectedItem).Content.ToString());

                            EntityRecordDataGrid.ItemsSource = result.Include(c => c.Entity).Include(c => c.CreatedBy).Include(c => c.ModifiedBy).ToList();
                            break;
                        }
                    }

                    StateEntityRecordCancelFilterButton.IsEnabled = true;
                    NameEntityRecordWrapPanel.IsEnabled = false;
                    UserEntityRecordWrapPanel.IsEnabled = false;
                }
                else // Если в ComboBox пустое значение выборки
                {
                    switch (SelectedUserCheckBox.IsChecked)
                    {
                        // Вывод в таблицу "Журнал последних действий пользователей системы" всех существующих записей, которые принадлежат выбранному пользователю системы
                        case true:
                        {
                            User selectedUser = dbContainer.Users.Find(((User) UserDataGrid.SelectedItem)?.Id);

                            IQueryable<EntityRecord> result = dbContainer.EntityRecords.Include(c => c.EntityHistories).Where(c =>
                                c.CreatedBy.Id == selectedUser.Id || c.ModifiedBy.Id == selectedUser.Id || c.EntityHistories.Any(k => k.User.Id == selectedUser.Id));

                            EntityRecordDataGrid.ItemsSource = result.Include(c => c.Entity).Include(c => c.CreatedBy).Include(c => c.ModifiedBy).ToList();
                            break;
                        }
                        // Вывод в таблицу "Журнал последних действий пользователей системы" всех существующих записей в базе данных
                        case false:
                        {
                            EntityRecordDataGrid.ItemsSource = dbContainer.EntityRecords.Include(c => c.Entity).Include(c => c.CreatedBy).Include(c => c.ModifiedBy).ToList();
                            break;
                        }
                    }

                    StateEntityRecordCancelFilterButton.IsEnabled = false;
                    NameEntityRecordWrapPanel.IsEnabled = true;
                    UserEntityRecordWrapPanel.IsEnabled = true;
                }
            }
        }

        private void EntityRecordStateCancelSearchButton_Click(object sender, RoutedEventArgs e) // Метод перетаскивания окна за любую область
        {
            StateEntityRecordComboBox.SelectedItem = null;
        }

        private void EntityRecordUserSearchFilterChecked(object sender, RoutedEventArgs e) // CheckBox-фильтры поиска записей журнала последних действий по ФИО пользователя системы (Создал или Изменил)
        {
            if (CreatedByCheckBox.IsChecked == true) // Если выбран поиск "Создал"
            {
                UserEntityRecordTextBox.IsEnabled = true;
                ModifiedByCheckBox.IsEnabled = false;
            }
            else if (ModifiedByCheckBox.IsChecked == true) // Если выбран поиск "Изменил"
            {
                UserEntityRecordTextBox.IsEnabled = true;
                CreatedByCheckBox.IsEnabled = false;
            }
            else // Если ни один из фильтров не был выбран
            {
                CreatedByCheckBox.IsEnabled = true;
                ModifiedByCheckBox.IsEnabled = true;
                UserEntityRecordTextBox.IsEnabled = false;
                UserEntityRecordTextBox.Text = "";
            }
        }

        private void EntityRecordUserSqlLikeSearch(object sender, TextChangedEventArgs e) // Поиск записей журнала последних действий по ФИО пользователя системы (Создал или Изменил)
        {
            using (LibraryModelContainer dbContainer = new LibraryModelContainer())
            {
                EntityHistoryDataGrid.ItemsSource = null;

                if (UserEntityRecordTextBox.Text != "") // Если TextBox поиска по ФИО пользователя системы имеет какой-либо текст
                {
                    if (CreatedByCheckBox.IsChecked == true) // Если выбран поиск "Создал"
                    {
                        switch (SelectedUserCheckBox.IsChecked)
                        {
                            // Поиск записей в таблице "Журнал последних действий пользователей системы" по названию записи для выбранного пользователя системы
                            case true:
                            {
                                User selectedUser = dbContainer.Users.Find(((User) UserDataGrid.SelectedItem)?.Id);

                                IQueryable<EntityRecord> result = dbContainer.EntityRecords
                                    .Include(c => c.Entity)
                                    .Include(c => c.CreatedBy)
                                    .Include(c => c.ModifiedBy)
                                    .Include(c => c.EntityHistories)
                                    .Where(c =>
                                        c.CreatedBy.Name.Contains(UserEntityRecordTextBox.Text) && 
                                        (c.CreatedBy.Id == selectedUser.Id || 
                                         c.ModifiedBy.Id == selectedUser.Id || 
                                         c.EntityHistories.Any(k => k.User.Id == selectedUser.Id)));

                                EntityRecordDataGrid.ItemsSource = result.Include(c => c.Entity).Include(c => c.CreatedBy).Include(c => c.ModifiedBy).ToList();
                                break;
                            }
                            // Поиск записей в таблице "Журнал последних действий пользователей системы" по названию записи в базе данных
                            case false:
                            {
                                IQueryable<EntityRecord> result = dbContainer.EntityRecords.Include(c => c.Entity).Include(c => c.CreatedBy).Include(c => c.ModifiedBy).Where(c => c.CreatedBy.Name.Contains(UserEntityRecordTextBox.Text));

                                EntityRecordDataGrid.ItemsSource = result.Include(c => c.Entity).Include(c => c.CreatedBy).Include(c => c.ModifiedBy).ToList(); 
                                break;
                            }
                        }
                    }

                    if (ModifiedByCheckBox.IsChecked == true) // Если выбран поиск "Изменил"
                    {
                        switch (SelectedUserCheckBox.IsChecked)
                        {
                            // Поиск записей в таблице "Журнал последних действий пользователей системы" по названию записи для выбранного пользователя системы
                            case true:
                            {
                                User selectedUser = dbContainer.Users.Find(((User) UserDataGrid.SelectedItem)?.Id);

                                IQueryable<EntityRecord> result = dbContainer.EntityRecords
                                    .Include(c => c.Entity)
                                    .Include(c => c.CreatedBy)
                                    .Include(c => c.ModifiedBy)
                                    .Include(c => c.EntityHistories)
                                    .Where(c =>
                                        c.ModifiedBy.Name.Contains(UserEntityRecordTextBox.Text) && 
                                        (c.CreatedBy.Id == selectedUser.Id || 
                                         c.ModifiedBy.Id == selectedUser.Id || 
                                         c.EntityHistories.Any(k => k.User.Id == selectedUser.Id)));

                                EntityRecordDataGrid.ItemsSource = result.Include(c => c.Entity).Include(c => c.CreatedBy).Include(c => c.ModifiedBy).ToList();
                                break;
                            }
                            // Поиск записей в таблице "Журнал последних действий пользователей системы" по названию записи в базе данных
                            case false:
                            {
                                IQueryable<EntityRecord> result = dbContainer.EntityRecords.Include(c => c.Entity).Include(c => c.CreatedBy).Include(c => c.ModifiedBy).Where(c => c.ModifiedBy.Name.Contains(UserEntityRecordTextBox.Text));

                                EntityRecordDataGrid.ItemsSource = result.Include(c => c.Entity).Include(c => c.CreatedBy).Include(c => c.ModifiedBy).ToList(); 
                                break;
                            }
                        }
                    }

                    NameEntityRecordWrapPanel.IsEnabled = false;
                    StateEntityRecordWrapPanel.IsEnabled = false;
                }
                else // Если TextBox поиска по ФИО пользователя системы пустой
                {
                    switch (SelectedUserCheckBox.IsChecked)
                    {
                        // Вывод в таблицу "Журнал последних действий пользователей системы" всех существующих записей, которые принадлежат выбранному пользователю системы
                        case true:
                        {
                            User selectedUser = dbContainer.Users.Find(((User) UserDataGrid.SelectedItem)?.Id);

                            IQueryable<EntityRecord> result = dbContainer.EntityRecords.Include(c => c.EntityHistories).Where(c =>
                                c.CreatedBy.Id == selectedUser.Id || c.ModifiedBy.Id == selectedUser.Id || c.EntityHistories.Any(k => k.User.Id == selectedUser.Id));

                            EntityRecordDataGrid.ItemsSource = result.Include(c => c.Entity).Include(c => c.CreatedBy).Include(c => c.ModifiedBy).ToList();
                            break;
                        }
                        // Вывод в таблицу "Журнал последних действий пользователей системы" всех существующих записей в базе данных
                        case false:
                        {
                            EntityRecordDataGrid.ItemsSource = dbContainer.EntityRecords.Include(c => c.Entity).Include(c => c.CreatedBy).Include(c => c.ModifiedBy).ToList();
                            break;
                        }
                    }

                    NameEntityRecordWrapPanel.IsEnabled = true;
                    StateEntityRecordWrapPanel.IsEnabled = true;
                }
            }
        }

        #endregion

        #region EntityHistoryDataGrid filters

        private void EntityHistoryRecordChecked(object sender, RoutedEventArgs e)
        {
            switch (EntityHistoryRecordCheckBox.IsChecked)
            {
                case true:
                {
                    EntityHistoryStackPanel.Visibility = Visibility.Visible;

                    EntityRecordDataGrid.IsEnabled = false;
                    DeleteEntityRecordButton.IsEnabled = false;
                    break;
                }
                case false:
                {
                    EntityHistoryStackPanel.Visibility = Visibility.Collapsed;
                    if (ValueEntityHistoryWrapPanel.IsEnabled)
                    {
                        if (OldValueEntityHistoryCheckBox.IsChecked == true)
                        {
                            OldValueEntityHistoryCheckBox.IsChecked = false;
                        }
                        else if (NewValueEntityHistoryCheckBox.IsChecked == true)
                        {
                            NewValueEntityHistoryCheckBox.IsChecked = false;
                        }
                    }

                    if (DateTimeEntityHistoryWrapPanel.IsEnabled)
                    {
                        DateTimeFromEntityHistory.Value = null;
                        DateTimeToEntityHistory.Value = null;
                    }
                    
                    EntityRecordDataGrid.IsEnabled = true;
                    DeleteEntityRecordButton.IsEnabled = true;
                    break;
                }
            }
        }

        private void ValueEntityHistoryChecked(object sender, RoutedEventArgs e)
        {
            if (OldValueEntityHistoryCheckBox.IsChecked == true || NewValueEntityHistoryCheckBox.IsChecked == true)
            {
                ValueEntityHistoryTextBox.IsEnabled = true;

                UserEntityHistoryWrapPanel.IsEnabled = false;
                DateTimeEntityHistoryWrapPanel.IsEnabled = false;
            }
            else
            {
                ValueEntityHistoryTextBox.IsEnabled = false;
                ValueEntityHistoryTextBox.Text = "";

                UserEntityHistoryWrapPanel.IsEnabled = true;
                DateTimeEntityHistoryWrapPanel.IsEnabled = true;
            }

        }

        private void ValueEntityHistorySqlLikeSearch(object sender, TextChangedEventArgs e)
        {
            using (LibraryModelContainer dbContainer = new LibraryModelContainer())
            {
                EntityRecord selectedRecord = dbContainer.EntityRecords.Find(((EntityRecord)EntityRecordDataGrid.SelectedItem)?.Id);
                if (ValueEntityHistoryTextBox.Text != "")
                {
                    if (OldValueEntityHistoryCheckBox.IsChecked == true)
                    {
                        IQueryable<EntityHistory> result = dbContainer.EntityHistories.Where(c => c.OldValue.Contains(ValueEntityHistoryTextBox.Text) && c.EntityRecord.Id == selectedRecord.Id);
                        EntityHistoryDataGrid.ItemsSource = result.OrderBy(c => c.Date).Include(c => c.User).ToList();
                    }
                    else if (NewValueEntityHistoryCheckBox.IsChecked == true)
                    {
                        IQueryable<EntityHistory> result = dbContainer.EntityHistories.Where(c => c.NewValue.Contains(ValueEntityHistoryTextBox.Text) && c.EntityRecord.Id == selectedRecord.Id);
                        EntityHistoryDataGrid.ItemsSource = result.OrderBy(c => c.Date).Include(c => c.User).ToList();
                    }
                }
                else
                {
                    IQueryable<EntityHistory> resultHistories = dbContainer.EntityHistories.Where(c => c.EntityRecord.Id == selectedRecord.Id);
                    EntityHistoryDataGrid.ItemsSource = resultHistories.OrderBy(c => c.Date).Include(c => c.User).ToList();
                }
            }
        }

        private void UserEntityHistorySqlLikeSearch(object sender, TextChangedEventArgs e)
        {
            using (LibraryModelContainer dbContainer = new LibraryModelContainer())
            {
                EntityRecord selectedRecord = dbContainer.EntityRecords.Find(((EntityRecord)EntityRecordDataGrid.SelectedItem)?.Id);
                if (selectedRecord != null)
                {
                    if (UserEntityHistoryTextBox.Text != "")
                    {
                        ValueEntityHistoryWrapPanel.IsEnabled = false;
                        DateTimeEntityHistoryWrapPanel.IsEnabled = false;

                        IQueryable<EntityHistory> result = dbContainer.EntityHistories.Where(c => c.EntityRecord.Id == selectedRecord.Id && c.User.Name.Contains(UserEntityHistoryTextBox.Text));
                        EntityHistoryDataGrid.ItemsSource = result.OrderBy(c => c.Date).Include(c => c.User).ToList();
                    }
                    else
                    {
                        ValueEntityHistoryWrapPanel.IsEnabled = true;
                        DateTimeEntityHistoryWrapPanel.IsEnabled = true;

                        IQueryable<EntityHistory> result = dbContainer.EntityHistories.Where(c => c.EntityRecord.Id == selectedRecord.Id);
                        EntityHistoryDataGrid.ItemsSource = result.OrderBy(c => c.Date).Include(c => c.User).ToList();
                    }
                }
            }
        }

        private void DateTimeEntityHistoryConditionFiltersChanged(object sender, SelectionChangedEventArgs e)
        {
            // Если указаны значение времени "Дата от" и в ComboBox выбрано условие фильтрации
            if (DateTimeFromEntityHistory.Value != null && EntityHistoryDateTimeConditionFiltersComboBox.SelectedItem != null && 
                ((ContentControl)EntityHistoryDateTimeConditionFiltersComboBox.SelectedItem)?.Content.ToString() != "")
            {
                DateTimeToEntityHistory.IsEnabled = false;
                DateTimeSearchEntityHistoryButton.IsEnabled = true;
            }
            else
            {
                DateTimeToEntityHistory.IsEnabled = true;
                DateTimeSearchEntityHistoryButton.IsEnabled = false;
            }
        }

        private void DateTimeEntityHistoryValueChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            if (DateTimeFromEntityHistory.Value != null && DateTimeToEntityHistory.Value != null) // Если указаны значения времён "Даты от" и "Даты до"
            {
                EntityHistoryDateTimeConditionFiltersComboBox.IsEnabled = false;
                DateTimeSearchEntityHistoryButton.IsEnabled = true;
            }
            else if (DateTimeFromEntityHistory.Value != null && EntityHistoryDateTimeConditionFiltersComboBox.SelectedItem != null && 
                     ((ContentControl)EntityHistoryDateTimeConditionFiltersComboBox.SelectedItem)?.Content.ToString() != "") // Если указаны значение времени "Дата от" и в ComboBox выбрано условие фильтрации
            {
                DateTimeToEntityHistory.IsEnabled = false;
                DateTimeSearchEntityHistoryButton.IsEnabled = true;
            }
            else
            {
                DateTimeToEntityHistory.IsEnabled = true;
                EntityHistoryDateTimeConditionFiltersComboBox.IsEnabled = true;
                DateTimeSearchEntityHistoryButton.IsEnabled = false;
            }
        }

        private void DateTimeEntityHistorySearchButton_Click(object sender, RoutedEventArgs e)
        {
            using (LibraryModelContainer dbContainer = new LibraryModelContainer())
            {
                EntityRecord selectedRecord = dbContainer.EntityRecords.Find(((EntityRecord)EntityRecordDataGrid.SelectedItem)?.Id);

                // Если указаны значение времени "Дата от" и в ComboBox выбрано условие фильтрации
                if (DateTimeFromEntityHistory.Value != null && EntityHistoryDateTimeConditionFiltersComboBox.SelectedItem != null && 
                    ((ContentControl)EntityHistoryDateTimeConditionFiltersComboBox.SelectedItem)?.Content.ToString() != "")
                {
                    DateTime? tempDateTimeFrom = DateTimeFromEntityHistory.Value.Value.AddSeconds(1);

                    switch (EntityHistoryDateTimeConditionFiltersComboBox.SelectedIndex)
                    {
                        case 1: // Если в ComboBox выбрано значение =
                        {
                            IQueryable<EntityHistory> result = dbContainer.EntityHistories.Where(c => 
                                c.Date >= DateTimeFromEntityHistory.Value && 
                                c.Date < tempDateTimeFrom.Value && 
                                c.EntityRecord.Id == selectedRecord.Id);

                            EntityHistoryDataGrid.ItemsSource = result.OrderBy(c => c.Date).Include(c => c.User).ToList();
                            break;
                        }
                        case 2: // Если в ComboBox выбрано значение ≠
                        {
                            IQueryable<EntityHistory> result = dbContainer.EntityHistories.Where(c => 
                                (c.Date < DateTimeFromEntityHistory.Value || 
                                c.Date >= tempDateTimeFrom.Value) &&
                                c.EntityRecord.Id == selectedRecord.Id);

                            EntityHistoryDataGrid.ItemsSource = result.OrderBy(c => c.Date).Include(c => c.User).ToList();
                            break;
                        }
                        case 3: // Если в ComboBox выбрано значение >
                        {
                            IQueryable<EntityHistory> result = dbContainer.EntityHistories.Where(c => 
                                c.Date >= tempDateTimeFrom.Value && 
                                c.EntityRecord.Id == selectedRecord.Id);

                            EntityHistoryDataGrid.ItemsSource = result.OrderBy(c => c.Date).Include(c => c.User).ToList();
                            break;
                        }
                        case 4: // Если в ComboBox выбрано значение <
                        {
                            IQueryable<EntityHistory> result = dbContainer.EntityHistories.Where(c => 
                                c.Date < DateTimeFromEntityHistory.Value && 
                                c.EntityRecord.Id == selectedRecord.Id);

                            EntityHistoryDataGrid.ItemsSource = result.OrderBy(c => c.Date).Include(c => c.User).ToList();
                            break;
                        }
                        case 5: // Если в ComboBox выбрано значение >=
                        {
                            IQueryable<EntityHistory> result = dbContainer.EntityHistories.Where(c => 
                                c.Date >= DateTimeFromEntityHistory.Value && 
                                c.EntityRecord.Id == selectedRecord.Id);

                            EntityHistoryDataGrid.ItemsSource = result.OrderBy(c => c.Date).Include(c => c.User).ToList();
                            break;
                        }
                        case 6: // Если в ComboBox выбрано значение <=
                        {
                            IQueryable<EntityHistory> result = dbContainer.EntityHistories.Where(c => 
                                c.Date < tempDateTimeFrom.Value && 
                                c.EntityRecord.Id == selectedRecord.Id);

                            EntityHistoryDataGrid.ItemsSource = result.OrderBy(c => c.Date).Include(c => c.User).ToList();
                            break;
                        }
                    }
                }
                else if (DateTimeFromEntityHistory.Value != null && DateTimeToEntityHistory.Value != null) // Если указаны значения времён "Даты от" и "Даты до"
                {
                    DateTime? tempDateTimeTo = DateTimeToEntityHistory.Value.Value.AddSeconds(1);

                    IQueryable<EntityHistory> result = dbContainer.EntityHistories.Where(c => 
                        c.Date >= DateTimeFromEntityHistory.Value && 
                        c.Date < tempDateTimeTo.Value && 
                        c.EntityRecord.Id == selectedRecord.Id);

                    EntityHistoryDataGrid.ItemsSource = result.OrderBy(c => c.Date).Include(c => c.User).ToList();
                }
            }
        }

        private void DateTimeEntityHistoryCancelSearchButton_Click(object sender, RoutedEventArgs e)
        {
            using (LibraryModelContainer dbContainer = new LibraryModelContainer())
            {
                DateTimeFromEntityHistory.Value = null;
                DateTimeToEntityHistory.Value = null;
                EntityHistoryDateTimeConditionFiltersComboBox.SelectedIndex = 0;

                EntityRecord selectedRecord = dbContainer.EntityRecords.Find(((EntityRecord)EntityRecordDataGrid.SelectedItem)?.Id);
                IQueryable<EntityHistory> result = dbContainer.EntityHistories.Where(c => c.EntityRecord.Id == selectedRecord.Id);
                EntityHistoryDataGrid.ItemsSource = result.OrderBy(c => c.Date).Include(c => c.User).ToList();
            }
        }

        #endregion

        #endregion
    }
}