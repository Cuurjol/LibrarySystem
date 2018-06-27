using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Input;

namespace Library
{
    /// <summary>
    /// Логика взаимодействия для ReaderWindow.xaml
    /// </summary>
    public partial class ReaderWindow
    {
        private readonly User _currentUser; // Текущий пользователь системы
        private readonly bool _isAddMode; // Булевское поле для проверки режима операции над записью
        private readonly Reader _selectedReader; // Выбранный читатель в ReaderDataGrid для редактирования

        public ReaderWindow(bool isAddMode, Reader reader, User user)
        {
            InitializeComponent();
            _isAddMode = isAddMode;
            _selectedReader = reader;
            _currentUser = user;

            using (LibraryModelContainer dbContainer = new LibraryModelContainer())
            {
                if (!_isAddMode) // Режим редактирования существующего читателя
                {
                    AddUpdateButton.Content = "Редактировать";

                    BlockedReaderCheckBox.IsEnabled = true;
                    RemovedReaderCheckBox.IsEnabled = true;

                    reader = dbContainer.Readers.Find(reader.Id);

                    if (reader != null)
                    {
                        ReaderNameTextBox.Text = reader.Name;
                        BlockedReaderCheckBox.IsChecked = reader.Blocked;
                        RemovedReaderCheckBox.IsChecked = reader.Removed;
                    }
                }
            }
        }

        private void CreateEntityHistory(string fieldName, string oldValue,  string newValue, EntityRecord record) // Метод создания историй изменений записи читателя для таблицы EntityHistory
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

        private void BlockedOrRemovedCheckBoxChecked(object sender, RoutedEventArgs e) // Метод переключения CheckBoxes
        {
            BlockedReaderCheckBox.IsEnabled = RemovedReaderCheckBox.IsChecked != true;
            RemovedReaderCheckBox.IsEnabled = BlockedReaderCheckBox.IsChecked != true;
        }

        private void AddUpdateRecord_Click(object sender, RoutedEventArgs e)
        {
            if (ReaderNameTextBox.Text != "" && ReaderNameTextBox.Text.Length <= 50) // Если заполнено поле ФИО и длина текста <= 50 символов
            {
                using (LibraryModelContainer dbContainer = new LibraryModelContainer())
                {
                    Entity entity = dbContainer.Entities.First(c => c.Name == "Читатель");
                    User user = dbContainer.Users.Find(_currentUser.Id);

                    if (_isAddMode) // Режим добавления записи
                    {
                        // Создание читателя для таблицы Readers
                        Reader reader = new Reader
                        {
                            Id = Guid.NewGuid(),
                            Name = ReaderNameTextBox.Text,
                            Status = "Активен",
                            Blocked = false,
                            Removed = false
                        };
                        dbContainer.Readers.Add(reader);

                        // Создание записи для таблицы EntityRecords
                        EntityRecord record = new EntityRecord
                        {
                            Id = Guid.NewGuid(),
                            Entity = entity,
                            Name = reader.Name,
                            State = "Добавлен",
                            CreatedBy = user,
                            ModifiedBy = null,
                            Reader = reader
                        };
                        dbContainer.EntityRecords.Add(record);

                        // Создание коллекции историй изменений записи читателя для таблицы EntityHistories
                        IList<EntityHistory> historyRecordList = new List<EntityHistory>
                        {
                            new EntityHistory
                            {
                                Id = Guid.NewGuid(),
                                FieldName = "ФИО",
                                OldValue = null,
                                NewValue = reader.Name,
                                Date = DateTime.Now,
                                User = user,
                                EntityRecord = record
                            },
                            new EntityHistory
                            {
                                Id = Guid.NewGuid(),
                                FieldName = "Статус",
                                OldValue = null,
                                NewValue = reader.Status,
                                Date = DateTime.Now,
                                User = user,
                                EntityRecord = record
                            },
                            new EntityHistory
                            {
                                Id = Guid.NewGuid(),
                                FieldName = "Заблокирован",
                                OldValue = null,
                                NewValue = reader.Blocked ? "Да" : "Нет",
                                Date = DateTime.Now,
                                User = user,
                                EntityRecord = record
                            },
                            new EntityHistory
                            {
                                Id = Guid.NewGuid(),
                                FieldName = "Удалён",
                                OldValue = null,
                                NewValue = reader.Removed ? "Да" : "Нет",
                                Date = DateTime.Now,
                                User = user,
                                EntityRecord = record
                            }
                        };
                        dbContainer.EntityHistories.AddRange(historyRecordList);

                        dbContainer.SaveChanges();
                        MessageBox.Show("Новый читатель успешно добавлен в систему.");
                    }
                    else // Режим редактирования записи
                    {
                        Reader oldReader = dbContainer.Readers.Find(_selectedReader.Id);

                        if (oldReader != null)
                        {
                            // Сохраняем старые значения читателя
                            string name = oldReader.Name;
                            bool blocked = oldReader.Blocked;
                            bool removed = oldReader.Removed;

                            oldReader.Name = ReaderNameTextBox.Text;
                            oldReader.Blocked = BlockedReaderCheckBox.IsChecked == true;
                            oldReader.Removed = RemovedReaderCheckBox.IsChecked == true;

                            dbContainer.SaveChanges();

                            // Если пользователь решил не изменять информацию о выбранном читателе в таблице ReaderDataGrid и нажал кнопку "Редактировать"
                            if (name == oldReader.Name && blocked == oldReader.Blocked && removed == oldReader.Removed)
                            {
                                MessageBox.Show("Информация о выбранном читателе не изменилась.");
                            }
                            else
                            {
                                EntityRecord record = dbContainer.EntityRecords.First(c => c.Reader.Id == oldReader.Id); // Находим запись нужного читателя для редактирования в таблице EntityRecords

                                // Меняем значения у объекта типа EntityRecord
                                record.ModifiedBy = user;
                                record.State = "Изменён";

                                if (name != oldReader.Name) // Если изменилось ФИО у читателя
                                {
                                    record.Name = oldReader.Name;

                                    CreateEntityHistory("Название", name, oldReader.Name, record);
                                }

                                if (blocked != oldReader.Blocked) // Если изменилось значение поля "Заблокирован" у читателя
                                {
                                    CreateEntityHistory("Заблокирован", blocked ? "Да" : "Нет", oldReader.Blocked ? "Да" : "Нет", record);
                                }

                                if (removed != oldReader.Removed) // Если изменилось значение поля "Удалён" у читателя
                                {
                                    CreateEntityHistory("Удалён", removed ? "Да" : "Нет", oldReader.Removed ? "Да" : "Нет", record);
                                }

                                dbContainer.SaveChanges();
                                MessageBox.Show("Информация о выбранном читателе успешно изменена в системе.");
                            }
                        }
                    }
                }

                Close();
            }
            else if (ReaderNameTextBox.Text.Length > 50)
            {
                MessageBox.Show("Поле ФИО. Длина текста не должна превышать больше 50 символов.");
            }
            else // Если поле ФИО пустое
            {
                MessageBox.Show("Заполните поле для читателя: ФИО.");
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
    }
}