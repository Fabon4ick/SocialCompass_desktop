using System;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Threading.Tasks;

namespace SocialCompass
{
    public partial class LoginWindow : Window
    {
        private readonly ApiService _apiService;

        public LoginWindow()
        {
            InitializeComponent();
            _apiService = new ApiService();  // Инициализируем ApiService
        }

        // Функция для очистки номера телефона
        private string CleanPhoneNumber(string phoneNumber)
        {
            return new string(phoneNumber.Where(char.IsDigit).ToArray());
        }

        // Обработчик изменения пароля
        private void PasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
        {
            if (PasswordBox.Password.Length > 0)
            {
                PasswordPlaceholder.Visibility = Visibility.Hidden;
            }
            else
            {
                PasswordPlaceholder.Visibility = Visibility.Visible;
            }
        }

        // Обработчик для кнопки входа
        private async void LoginButton_Click(object sender, RoutedEventArgs e)
        {
            string phoneNumber = PhoneNumberTextBox.Text;
            phoneNumber = CleanPhoneNumber(phoneNumber);
            string password = PasswordBox.Password;

            try
            {
                var user = await _apiService.AuthenticateUserAsync(phoneNumber, password);

                // Запрашиваем список заявок
                var applications = await _apiService.GetApplicationsAsync();

                // Передаем пользователя и заявки в следующее окно
                var userInfoWindow = new UserInfoWindow(user, applications);
                userInfoWindow.Show();

                // Закрываем окно авторизации
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка авторизации: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // Обработчик для регистрации
        private void RegisterText_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            // Логика для перехода на страницу регистрации
            MessageBox.Show("Переход к регистрации");
        }

        // Обработчик для изменения текста (маска номера телефона)
        private void PhoneNumberTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            var textBox = sender as TextBox;

            // Удаляем обработчик, чтобы избежать зацикливания
            textBox.TextChanged -= PhoneNumberTextBox_TextChanged;

            // Убираем все нецифровые символы
            string text = new string(textBox.Text.Where(char.IsDigit).ToArray());

            // Удаляем код страны, если пользователь вручную его вводит
            if (text.StartsWith("7"))
                text = text.Substring(1);

            // Ограничиваем длину текста до 10 символов
            if (text.Length > 10)
                text = text.Substring(0, 10);

            // Форматируем текст
            var formattedText = new StringBuilder("+7 ");

            if (text.Length > 0) formattedText.Append("(");
            formattedText.Append(string.Concat(text.Take(3)));
            if (text.Length > 3) formattedText.Append(") ");
            formattedText.Append(string.Concat(text.Skip(3).Take(3)));
            if (text.Length > 6) formattedText.Append("-");
            formattedText.Append(string.Concat(text.Skip(6).Take(2)));
            if (text.Length > 8) formattedText.Append("-");
            formattedText.Append(string.Concat(text.Skip(8).Take(2)));

            // Устанавливаем форматированный текст
            textBox.Text = formattedText.ToString();

            // Устанавливаем курсор в конец текста
            textBox.SelectionStart = textBox.Text.Length;

            // Восстанавливаем обработчик
            textBox.TextChanged += PhoneNumberTextBox_TextChanged;
        }

        private void PhoneNumberTextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            // Разрешаем вводить только цифры
            e.Handled = !char.IsDigit(e.Text, 0);
        }
    }
}
