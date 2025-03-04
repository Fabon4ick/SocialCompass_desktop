using System;
using System.IO;
using System.Threading.Tasks;
using System.Windows;

namespace SocialCompass
{
    public partial class EditStaffWindow : Window
    {
        private readonly StaffResponse _staff;
        private readonly ApiService _apiService;
        public event Action StaffUpdated;
        private byte[] selectedPhoto;

        public EditStaffWindow(StaffResponse staff, ApiService apiService)
        {
            InitializeComponent();
            _staff = staff;
            _apiService = apiService;

            NameTextBox.Text = staff.Name;
            SurnameTextBox.Text = staff.Surname;
            PatronymicTextBox.Text = staff.Patronymic;
            BirthDatePicker.SelectedDate = staff.Birth;
            EmploymentDatePicker.SelectedDate = staff.EmploymentDay;
            BioTextBox.Text = staff.Bio;
            IsVisibleCheckBox.IsChecked = staff.isVisible;
        }

        private void SelectPhotoButton_Click(object sender, RoutedEventArgs e)
        {
            var openFileDialog = new Microsoft.Win32.OpenFileDialog
            {
                Filter = "Image Files|*.jpg;*.jpeg;*.png;*.bmp;*.gif"
            };

            if (openFileDialog.ShowDialog() == true)
            {
                // Загружаем фото
                var filePath = openFileDialog.FileName;
                selectedPhoto = File.ReadAllBytes(filePath);

                // Фото теперь будет сохранено в selectedPhoto, но не будет отображаться
            }
        }

        private async void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var staffUpdate = new StaffUpdate
                {
                    Photo = selectedPhoto ?? _staff.Photo, // Используем выбранное фото, если оно есть
                    Name = NameTextBox.Text,
                    Surname = SurnameTextBox.Text,
                    Patronymic = PatronymicTextBox.Text,
                    Birth = BirthDatePicker.SelectedDate?.ToString("yyyy-MM-dd"),
                    EmploymentDay = EmploymentDatePicker.SelectedDate?.ToString("yyyy-MM-dd"),
                    Bio = BioTextBox.Text,
                    isVisible = IsVisibleCheckBox.IsChecked ?? false
                };

                await _apiService.UpdateStaffAsync(_staff.Id, staffUpdate);

                MessageBox.Show("Данные успешно сохранены!");
                StaffUpdated?.Invoke(); // Вызываем событие
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка: {ex.Message}");
            }
        }
    }
}
