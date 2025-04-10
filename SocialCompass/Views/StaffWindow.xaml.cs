using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using SocialCompass.Views;
using static System.Net.Mime.MediaTypeNames;

namespace SocialCompass
{
    public partial class StaffWindow : Window
    {
        private readonly ApiService _apiService; // Инициализация сервиса API
        public ObservableCollection<StaffResponse> StaffList { get; set; }
        private UserResponse user;
        private List<ApplicationResponse> applications = new List<ApplicationResponse>();

        public StaffWindow()
        {
            InitializeComponent();
            _apiService = new ApiService(); // Создание экземпляра сервиса
            StaffList = new ObservableCollection<StaffResponse>();
            DataContext = this;
            LoadStaffs();
        }

        private async void LoadStaffs()
        {
            try
            {
                // Получаем список сотрудников через сервис
                var staffs = await _apiService.GetStaffsAsync();

                // Очищаем текущий список и заполняем его новыми данными
                StaffList.Clear();
                foreach (var staff in staffs)
                {
                    StaffList.Add(staff);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки данных: {ex.Message}");
            }
        }

        private async void AddStaff_Click(object sender, RoutedEventArgs e)
        {
            AddStaffWindow addStaffWindow = new AddStaffWindow();
            bool? result = addStaffWindow.ShowDialog(); // ShowDialog() возвращает bool?, а не bool

            if (result.HasValue && result.Value) // Проверяем, что пользователь нажал "Сохранить"
            {
                StaffRequest newStaff = addStaffWindow.NewStaffData;
                bool isAdded = await _apiService.AddStaffAsync(newStaff);

                if (isAdded)
                {
                    MessageBox.Show("Сотрудник успешно добавлен!");
                    LoadStaffs(); // Перезагружаем список сотрудников
                }
                else
                {
                    MessageBox.Show("Ошибка при добавлении сотрудника.");
                }
            }
        }

        private void StaffGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // Логика выбора сотрудника в DataGrid
        }

        private void EditStaff_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.DataContext is StaffResponse staff)
            {
                // Открытие окна редактирования и передача данных
                EditStaffWindow editWindow = new EditStaffWindow(staff, _apiService);
                editWindow.StaffUpdated += LoadStaffs; // Подписка на событие
                editWindow.ShowDialog();
            }
        }

        private async void DeleteStaff_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.DataContext is StaffResponse staffToDelete)
            {
                if (MessageBox.Show($"Удалить сотрудника {staffToDelete.Surname} {staffToDelete.Name}?",
                                    "Удаление", MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.Yes)
                {
                    try
                    {
                        var staffs = await _apiService.GetStaffsAsync();
                        var availableStaffs = staffs.Where(s => s.Id != staffToDelete.Id).ToList();

                        if (!availableStaffs.Any())
                        {
                            MessageBox.Show("Нет доступных сотрудников для замены!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                            return;
                        }

                        // Открываем окно выбора замены
                        SelectStaffWindow selectStaffWindow = new SelectStaffWindow(availableStaffs);
                        bool? result = selectStaffWindow.ShowDialog();

                        if (result == true) // Проверка, что результат окна "ShowDialog" == true
                        {
                            int newStaffId = selectStaffWindow.SelectedStaffId;

                            // Отправляем API-запрос на замену и удаление
                            bool isReplaced = await _apiService.ReplaceAndDeleteStaffAsync(staffToDelete.Id, newStaffId);

                            if (isReplaced)
                            {
                                MessageBox.Show("Сотрудник успешно удалён и заявки перенесены!");
                                StaffList.Remove(staffToDelete); // Удаляем из списка
                            }
                            else
                            {
                                MessageBox.Show("Ошибка при замене сотрудника.");
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Ошибка удаления сотрудника: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
        }

        private void OpenActiveApplicationsPage_Click(object sender, RoutedEventArgs e)
        {
            // Открываем новое окно для страницы активных заявок
            ActiveApplicationsWindow activeApplicationsWindow = new ActiveApplicationsWindow(user, applications);
            activeApplicationsWindow.Show();
            this.Close();
        }

        private void OpenApplicationsPage_Click(object sender, RoutedEventArgs e)
        {
            // Открываем новое окно для страницы заявок
            UserInfoWindow applicationsWindow = new UserInfoWindow(user, applications);
            applicationsWindow.Show();
            this.Close(); // Закрытие текущего окна
        }

        private void OpenAddItemsPage_Click(object sender, RoutedEventArgs e)
        {
            AddDeleteItemsWindow addDeleteItemsWindow = new AddDeleteItemsWindow();
            addDeleteItemsWindow.Show();
            this.Close();
        }
    }
}
