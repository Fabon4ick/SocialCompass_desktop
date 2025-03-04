using System;
using System.IO;
using System.Windows;
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
                Photo = _photoData,
                isVisible = isVisible
            };

            DialogResult = true; // Завершаем окно и передаём результат
            Close();
        }

        private void SelectPhotoButton_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog
            {
                Filter = "Изображения (*.jpg;*.jpeg;*.png)|*.jpg;*.jpeg;*.png",
                Title = "Выберите фото сотрудника"
            };

            if (openFileDialog.ShowDialog() == true)
            {
                try
                {
                    _photoData = File.ReadAllBytes(openFileDialog.FileName);
                    MessageBox.Show("Фото успешно загружено!", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка при загрузке фото: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }
    }
}
