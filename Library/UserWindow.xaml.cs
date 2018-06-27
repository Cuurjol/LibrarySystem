using System;
using System.Windows;
using System.Windows.Input;

namespace Library
{
    /// <summary>
    /// Логика взаимодействия для UserWindow.xaml
    /// </summary>
    public partial class UserWindow
    {
        private readonly bool _isAddMode; // Булевское поле для проверки режима операции над записью
        private readonly User _selectedReader; // Выбранный пользователь системы в UserDataGrid для редактирования

        public UserWindow(bool isAddMode, User user, User currentUser)
        {
            InitializeComponent();
            _isAddMode = isAddMode;
            _selectedReader = user;

            if (currentUser.UserRole == "Администратор")
            {
                UserUserRoleComboBox.Items.Remove(UserUserRoleComboBox.Items[1]);
            }

            using (LibraryModelContainer dbContainer = new LibraryModelContainer())
            {
                if (!_isAddMode) // Режим редактирования существующего пользователя системы
                {
                    AddUpdateButton.Content = "Редактировать";

                    user = dbContainer.Users.Find(user.Id);

                    if (user != null)
                    {
                        UserNameTextBox.Text = user.Name;
                        UserUserRoleComboBox.Text = user.UserRole;
                        UserLoginTextBox.Text = user.Login;
                        UserPasswordTextBox.Text = user.Password;
                    }
                }
            }
        }

        private void AddUpdateRecord_Click(object sender, RoutedEventArgs e)
        {
            if (UserNameTextBox.Text != "" && UserNameTextBox.Text.Length <= 50 && UserUserRoleComboBox.SelectedItem != null
                && UserLoginTextBox.Text != "" && UserLoginTextBox.Text.Length <= 30 && 
                UserPasswordTextBox.Text != "" && UserPasswordTextBox.Text.Length <= 30) // Если в окне заполнены все поля и их длины <= заявленного значения
            {
                using (LibraryModelContainer dbContainer = new LibraryModelContainer())
                {
                    if (_isAddMode) // Режим добавления записи
                    {
                        User user = new User
                        {
                            Id = Guid.NewGuid(),
                            Name = UserNameTextBox.Text,
                            UserRole = UserUserRoleComboBox.Text,
                            Login = UserLoginTextBox.Text,
                            Password = UserPasswordTextBox.Text
                        };

                        dbContainer.Users.Add(user);
                        dbContainer.SaveChanges();
                        MessageBox.Show("Новый пользователь системы успешно добавлен в систему.");
                    }
                    else // Режим редактирования записи
                    {
                        _selectedReader.Name = UserNameTextBox.Text;
                        _selectedReader.UserRole = UserUserRoleComboBox.Text;
                        _selectedReader.Login = UserLoginTextBox.Text;
                        _selectedReader.Password = UserPasswordTextBox.Text;

                        dbContainer.SaveChanges();
                        MessageBox.Show("Информация о выбранном пользователе системы успешно изменена в системе.");
                    }
                }

                Close();
            }
            else if (UserNameTextBox.Text.Length > 50) // Если поле ФИО имеет длину текста > 50 символов
            {
                MessageBox.Show("Поле ФИО. Длина текста не должна превышать больше 50 символов.");
            }
            else if (UserLoginTextBox.Text.Length > 30) // Если поле login имеет длину текста > 30 символов
            {
                MessageBox.Show("Поле login. Длина текста  не должна превышать больше 30 символов.");
            }
            else if (UserPasswordTextBox.Text.Length > 30) // Если поле password имеет длину текста > 30 символов
            {
                MessageBox.Show("Поле password. Длина текста  не должна превышать больше 30 символов.");
            }
            else if (UserNameTextBox.Text == "" && UserUserRoleComboBox.SelectedItem == null &&
                UserLoginTextBox.Text == "" && UserPasswordTextBox.Text == "") // Если в окне все поля не заполнены 
            {
                MessageBox.Show("Заполните поля для пользователя системы: ФИО, Роль пользователя, Login, Password.");
            }
            else // Проверяем на пустоту заполнения некоторых полей в окне
            {
                if (UserNameTextBox.Text == "")
                {
                    MessageBox.Show("Заполните поле для пользователя системы: ФИО.");
                }
                else if (UserUserRoleComboBox.SelectedItem == null)
                {
                    MessageBox.Show("Заполните поле для пользователя системы: Роль пользователя.");
                }
                else if (UserLoginTextBox.Text == "")
                {
                    MessageBox.Show("Заполните поле для пользователя системы: Login.");
                }
                else
                {
                    MessageBox.Show("Заполните поле для пользователя системы: Password.");
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
    }
}
