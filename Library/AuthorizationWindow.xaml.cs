using System.Linq;
using System.Windows;
using System.Windows.Input;

namespace Library
{
    /// <summary>
    /// Логика взаимодействия для AuthorizationWindow.xaml
    /// </summary>
    public partial class AuthorizationWindow
    {
        public User CurrentUser { get; set; }

        public AuthorizationWindow()
        {
            InitializeComponent();
        }

        private void LoginTextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e) // Метод изменения текста логина
        {
            if (LoginTextBox.Text != "" && PasswordTextBox.Password != "")
            {
                LoginButton.IsEnabled = true;
            }
            else
            {
                LoginButton.IsEnabled = false;
            }
        }

        private void PasswordTextBoxChanged(object sender, RoutedEventArgs e) // Метод изменения текста пароля
        {
            if (LoginTextBox.Text != "" && PasswordTextBox.Password != "")
            {
                LoginButton.IsEnabled = true;
            }
            else
            {
                LoginButton.IsEnabled = false;
            }
        }

        private void Login_Click(object sender, RoutedEventArgs e) // Кнопка входа в систему "БИБЛИОТЕКА"
        {
            using (LibraryModelContainer dbContainer = new LibraryModelContainer())
            {
                User user = dbContainer.Users.FirstOrDefault(c => c.Login == LoginTextBox.Text && c.Password == PasswordTextBox.Password);

                if (user == null)
                {
                    MessageBox.Show("Неправильные данные для входа в систему. Пожалуйста, попробуйте снова.");
                }
                else
                {
                    CurrentUser = user;
                    DialogResult = true;
                }
            }
        }

        private void ExitingTheProgram_Click(object sender, RoutedEventArgs e) // Закрыть окно авторизации
        {
            Close();
        }

        private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e) // Метод перетаскивания окна за любую область
        {
            DragMove();
        }
    }
}
