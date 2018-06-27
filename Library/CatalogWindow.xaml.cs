using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Input;

namespace Library
{
    /// <summary>
    /// Логика взаимодействия для CatalogWindow.xaml
    /// </summary>
    public partial class CatalogWindow
    {
        private readonly User _currentUser; // Текущий пользователь системы
        private readonly bool _isAddMode; // Булевское поле для проверки режима операции над записью 
        private readonly Catalog _selectedCatalog; // Выбранный каталог в CatalogDataGrid для редактирования

        public CatalogWindow(bool isAddMode, Catalog catalog, User user)
        {
            InitializeComponent();
            _isAddMode = isAddMode;
            _selectedCatalog = catalog;
            _currentUser = user;

            using (LibraryModelContainer dbContainer = new LibraryModelContainer())
            {
                if (!_isAddMode) // Режим редактирования существующего каталога
                {
                    AddUpdateButton.Content = "Редактировать";

                    catalog = dbContainer.Catalogs.Find(catalog.Id);

                    if (catalog != null)
                    {
                        NameCatalogTextBox.Text = catalog.Name;
                        DescriptionCatalogTextBox.Text = catalog.Description;
                    }
                }
            }
        }

        private void CreateEntityHistory(string fieldName, string oldValue, string newValue, EntityRecord record) // Метод создания историй изменений записи каталога для таблицы EntityHistory
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
            if (NameCatalogTextBox.Text != "" && NameCatalogTextBox.Text.Length <= 50 && 
                DescriptionCatalogTextBox.Text != "" && DescriptionCatalogTextBox.Text.Length <= 150) // Если в окне заполнены все поля и их длины <= заявленного значения
            {
                using (LibraryModelContainer dbContainer = new LibraryModelContainer())
                {
                    Entity entity = dbContainer.Entities.First(c => c.Name == "Каталог");
                    User user = dbContainer.Users.Find(_currentUser.Id);

                    if (_isAddMode) // Режим добавления записи
                    {
                        // Создание каталога для таблицы Catalogs
                        Catalog catalog = new Catalog
                        {
                            Id = Guid.NewGuid(),
                            Name = NameCatalogTextBox.Text,
                            Description = DescriptionCatalogTextBox.Text
                        };
                        dbContainer.Catalogs.Add(catalog);

                        // Создание записи для таблицы EntityRecords
                        EntityRecord record = new EntityRecord
                        {
                            Id = Guid.NewGuid(),
                            Entity = entity,
                            Name = catalog.Name,
                            State = "Добавлен",
                            CreatedBy = user,
                            ModifiedBy = null,
                            Catalog = catalog
                        };
                        dbContainer.EntityRecords.Add(record);

                        // Создание коллекции историй изменений записи каталога для таблицы EntityHistories
                        IList<EntityHistory> historyRecordList = new List<EntityHistory>
                        {
                            new EntityHistory
                            {
                                Id = Guid.NewGuid(),
                                FieldName = "Название",
                                OldValue = null,
                                NewValue = catalog.Name,
                                Date = DateTime.Now,
                                User = user,
                                EntityRecord = record
                            },
                            new EntityHistory
                            {
                                Id = Guid.NewGuid(),
                                FieldName = "Описание",
                                OldValue = null,
                                NewValue = catalog.Description,
                                Date = DateTime.Now,
                                User = user,
                                EntityRecord = record
                            }
                        };
                        dbContainer.EntityHistories.AddRange(historyRecordList);

                        dbContainer.SaveChanges();
                        MessageBox.Show("Новый каталог успешно добавлен в систему.");
                    }
                    else // Режим редактирования записи
                    {
                        Catalog oldCatalog = dbContainer.Catalogs.Find(_selectedCatalog.Id);

                        if (oldCatalog != null)
                        {
                            // Сохраняем старое значения читателя
                            string name = oldCatalog.Name;
                            string description = oldCatalog.Description;

                            oldCatalog.Name = NameCatalogTextBox.Text;
                            oldCatalog.Description = DescriptionCatalogTextBox.Text;

                            // Если пользователь решил не изменять информацию о выбранном каталоге в таблице CatalogDataGrid и нажал кнопку "Редактировать"
                            if (name == oldCatalog.Name && description == oldCatalog.Description)
                            {
                                MessageBox.Show("Информация о выбранном каталоге не изменилась.");
                            }
                            else
                            {
                                EntityRecord record = dbContainer.EntityRecords.First(c => c.Catalog.Id == oldCatalog.Id); // Находим запись нужного каталога для редактирования в таблице EntityRecords

                                // Меняем значения у объекта типа EntityRecord
                                record.ModifiedBy = user;
                                record.State = "Изменён";

                                if (name != oldCatalog.Name) // Если изменилось название у каталога
                                {
                                    record.Name = oldCatalog.Name;

                                    CreateEntityHistory("Название", name, oldCatalog.Name, record);
                                }

                                if (description != oldCatalog.Description) // Если изменилось описание у каталога
                                {
                                    CreateEntityHistory("Описание", description, oldCatalog.Description, record);
                                }

                                dbContainer.SaveChanges();
                                MessageBox.Show("Информация о выбранном каталоге успешно изменена в системе.");
                            }
                        }
                    }
                }

                Close();
            }
            else if (NameCatalogTextBox.Text.Length > 50) // Если поле название имеет длину текста > 50 символов
            {
                MessageBox.Show("Поле название. Длина текста не должна превышать больше 50 символов.");
            }
            else if (DescriptionCatalogTextBox.Text.Length > 150) // Если поле описание имеет длину текста > 150 символов
            {
                MessageBox.Show("Поле описание. Длина текста не должна превышать больше 150 символов.");
            }
            else if (NameCatalogTextBox.Text == "" && DescriptionCatalogTextBox.Text == "") // Если в окне все поля не заполнены
            {
                MessageBox.Show("Заполните поля для каталога: Название, Описание.");
            }
            else // Проверяем на пустоту заполнения некоторых полей в окне
            {
                if (NameCatalogTextBox.Text == "")
                {
                    MessageBox.Show("Заполните поле для каталога: Название.");
                }
                else if (DescriptionCatalogTextBox.Text == "")
                {
                    MessageBox.Show("Заполните поле для каталога: Описание.");
                }
            }
        }

        private void Close_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e) // Метод перетаскивания окна за любую область
        {
            DragMove();
        }
    }
}
