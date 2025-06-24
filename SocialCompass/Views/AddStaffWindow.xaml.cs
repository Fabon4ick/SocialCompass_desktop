using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Microsoft.Win32;

namespace SocialCompass
{
    public partial class AddStaffWindow : Window
    {
        private byte[] _photoData; // Переменная для хранения фото

        public StaffRequest NewStaffData { get; private set; } // Свойство для передачи данных

        public AddStaffWindow()
        {
            InitializeComponent();

            SetupDatePickerTextBox(BirthDatePicker);
            SetupDatePickerTextBox(EmploymentDatePicker);
        }

        private void SetupDatePickerTextBox(DatePicker datePicker)
        {
            datePicker.Loaded += (s, e) =>
            {
                if (datePicker.Template.FindName("PART_TextBox", datePicker) is TextBox textBox)
                {
                    // Установим начальный цвет
                    textBox.Foreground = datePicker.SelectedDate.HasValue ? System.Windows.Media.Brushes.Black : System.Windows.Media.Brushes.Gray;

                    // Когда фокус теряется, обновим текст и цвет
                    textBox.LostFocus += (s2, e2) =>
                    {
                        if (datePicker.SelectedDate.HasValue)
                        {
                            textBox.Text = datePicker.SelectedDate.Value.ToString("yyyy-MM-dd");
                            textBox.Foreground = System.Windows.Media.Brushes.Black;
                        }
                    };

                    // Также реагируем на изменение выбранной даты
                    datePicker.SelectedDateChanged += (s3, e3) =>
                    {
                        if (datePicker.SelectedDate.HasValue)
                        {
                            textBox.Text = datePicker.SelectedDate.Value.ToString("yyyy-MM-dd");
                            textBox.Foreground = System.Windows.Media.Brushes.Black;
                        }
                        else
                        {
                            textBox.Text = string.Empty;
                            textBox.Foreground = System.Windows.Media.Brushes.Gray;
                        }
                    };
                }
            };
        }


        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            // Получаем данные из полей
            string name = NameTextBox.Text;
            string surname = SurnameTextBox.Text;
            string patronymic = PatronymicTextBox.Text;
            DateTime? birthDate = BirthDatePicker.SelectedDate;
            DateTime? employmentDate = EmploymentDatePicker.SelectedDate;
            string bio = BioTextBox.Text;
            bool isVisible = IsVisibleCheckBox.IsChecked ?? false;

            if (string.IsNullOrWhiteSpace(name) || string.IsNullOrWhiteSpace(surname) || birthDate == null || employmentDate == null)
            {
                MessageBox.Show("Заполните все обязательные поля!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            // Создаём объект для API
            NewStaffData = new StaffRequest
            {
                Name = name,
                Surname = surname,
                Patronymic = patronymic,
                Birth = birthDate.Value,
                EmploymentDay = employmentDate.Value,
                Bio = bio,
                Photo = null,
                isVisible = isVisible
            };

            DialogResult = true; // Завершаем окно и передаём результат
            Close();
        }

        private void SelectPhotoButton_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Хостинг не поддерживает хранение фотографий", "Информация",
                           MessageBoxButton.OK, MessageBoxImage.Information);
            _photoData = null; // Очищаем возможные предыдущие данные
        }
    }
}
