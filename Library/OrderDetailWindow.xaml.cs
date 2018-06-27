using System;
using System.Linq;
using System.Windows;
using System.Windows.Input;

namespace Library
{
    /// <summary>
    /// Логика взаимодействия для OrderDetailWindow.xaml
    /// </summary>
    public partial class OrderDetailWindow
    {
        private readonly Order _selectedOrder;
        private readonly User _currentUser;

        public OrderDetailWindow(Order order, User user)
        {
            InitializeComponent();

            using (LibraryModelContainer dbContainer = new LibraryModelContainer())
            {
                Order findOrder = dbContainer.Orders.Find(order.Id);

                if (findOrder != null)
                {
                    _selectedOrder = findOrder;
                    _currentUser = user;
                }
            }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            using (LibraryModelContainer dbContainer = new LibraryModelContainer())
            {
                Order findOrder = dbContainer.Orders.Find(_selectedOrder.Id);

                if (findOrder != null)
                {
                    DateTime now = DateTime.Now;

                    if (findOrder.DeadlineDate.CompareTo(now) < 0) // Если дата закрытия у заказа наступило раньше сегодняшнего дня
                    {
                        BlackListButton.IsEnabled = true;
                    }
                }
            }
        }

        private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e) // Метод перетаскивания окна за любую область
        {
            DragMove();
        }

        private void AddReaderInBlackList_Click(object sender, RoutedEventArgs e)
        {
            using (LibraryModelContainer dbContainer = new LibraryModelContainer())
            {
                Order order = dbContainer.Orders.Find(_selectedOrder.Id);
                User user = dbContainer.Users.Find(_currentUser.Id);

                if (order != null)
                {
                    bool blocked = order.Reader.Blocked; // Сохраняем старое значение поля Blocked у читателя
                    order.Reader.Blocked = true;

                    EntityRecord record = dbContainer.EntityRecords.First(c => c.Reader.Id == order.Reader.Id); // Находим запись нужного читателя для редактирования в таблице EntityRecords

                    // Меняем значения у объекта типа EntityRecord
                    record.ModifiedBy = user;
                    record.State = "Изменён";

                    CreateEntityHistory("Заблокирован", blocked ? "Да" : "Нет", order.Reader.Blocked ? "Да" : "Нет", record);

                    dbContainer.SaveChanges();
                    MessageBox.Show($"Читатель: {order.Reader.Name}. Указанный читатель успешно отправлен в очередь на добавление в чёрный список.");
                }
            }

            Close();
        }

        private void CreateEntityHistory(string fieldName, string oldValue, string newValue, EntityRecord record) // Метод создания историй изменений записи читателя для таблицы EntityHistory
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
    }
}
