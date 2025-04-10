using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using SocialCompass.Models;
using static System.Net.Mime.MediaTypeNames;

namespace SocialCompass.Views
{
    public partial class AddDeleteItemsWindow : Window
    {
        private UserResponse user;
        private List<ApplicationResponse> applications = new List<ApplicationResponse>();
        private readonly ApiService _apiService;
        private readonly Dictionary<string, string> _tableMappings;

        public AddDeleteItemsWindow()
        {
            InitializeComponent();
            _apiService = new ApiService();

            // Словарь соответствий (что видит пользователь -> что отправляется)
            _tableMappings = new Dictionary<string, string>
            {
                { "Гражданская категория", "civil_category" },
                { "Категория инвалидности", "disability_category" },
                { "Заболевания", "disease" },
                { "Семейное положение", "family_status" },
                { "Услуги", "service" }
            };

            // Привязываем к ComboBox
            EntityComboBox.ItemsSource = _tableMappings.Keys;
            EntityComboBox.DisplayMemberPath = ".";
            EntityComboBox.SelectedItem = "Гражданская категория";
        }

        private async void EntityComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (EntityComboBox.SelectedItem is string selectedKey && _tableMappings.TryGetValue(selectedKey, out string selectedTable))
            {
                try
                {
                    var items = await _apiService.GetItemsAsync<EntityItem>(selectedTable);
                    EntityDataGrid.ItemsSource = items;
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка при получении данных: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private async void AddItem(object sender, RoutedEventArgs e)
        {
            var newItem = NewItemTextBox.Text.Trim();
            if (string.IsNullOrEmpty(newItem) || EntityComboBox.SelectedItem == null)
            {
                MessageBox.Show("Выберите таблицу и введите данные!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Получаем название таблицы по ключу (на русском)
            if (EntityComboBox.SelectedItem is string selectedKey && _tableMappings.TryGetValue(selectedKey, out string selectedTable))
            {
                try
                {
                    // Отправляем POST-запрос
                    bool isAdded = await _apiService.AddItemAsync(selectedTable, newItem);

                    if (isAdded)
                    {
                        MessageBox.Show("Запись успешно добавлена!", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);

                        // Обновляем список элементов
                        var items = await _apiService.GetItemsAsync<EntityItem>(selectedTable);
                        EntityDataGrid.ItemsSource = items;
                    }
                    else
                    {
                        MessageBox.Show("Ошибка при добавлении!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка при добавлении записи: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            else
            {
                MessageBox.Show("Ошибка: выбранное значение отсутствует в таблице!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }


        private void DeleteItem_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button deleteButton && deleteButton.Tag is int itemId)
            {
                if (EntityComboBox.SelectedItem is string selectedKey && _tableMappings.TryGetValue(selectedKey, out string selectedTable))
                {
                    // Открываем окно замены
                    var replaceWindow = new ReplaceItemWindow(selectedTable, itemId);
                    replaceWindow.ShowDialog(); // Просто открываем окно без удаления записи
                }
            }
            else
            {
                MessageBox.Show("Ошибка: Tag не является int!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        public async void RefreshEntityData(string tableName)
        {
            if (string.IsNullOrEmpty(tableName)) return;

            try
            {
                var updatedData = await _apiService.GetItemsAsync<EntityItem>(tableName);
                EntityDataGrid.ItemsSource = updatedData;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при загрузке данных: {ex.Message}");
            }
        }


        private void TextBox_GotFocus(object sender, RoutedEventArgs e)
        {
            if (NewItemTextBox.Text == "Введите данные")
            {
                NewItemTextBox.Text = "";
                NewItemTextBox.Foreground = System.Windows.Media.Brushes.Black; // Меняем цвет текста на чёрный
            }
        }

        private void TextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(NewItemTextBox.Text))
            {
                NewItemTextBox.Text = "Введите данные";
                NewItemTextBox.Foreground = System.Windows.Media.Brushes.Gray; // Меняем цвет текста на серый
            }
        }

        private void OpenActiveApplicationsPage_Click(object sender, RoutedEventArgs e)
        {
            // Открываем новое окно для страницы активных заявок
            ActiveApplicationsWindow activeApplicationsWindow = new ActiveApplicationsWindow(user, applications);
            activeApplicationsWindow.Show();
            this.Close();
        }

        private void OpenStaffPage_Click(object sender, RoutedEventArgs e)
        {
            // Открываем новое окно для страницы работников
            StaffWindow staffWindow = new StaffWindow();
            staffWindow.Show();
            this.Close(); // Закрытие текущего окна
        }
        private void OpenApplicationsPage_Click(object sender, RoutedEventArgs e)
        {
            // Открываем новое окно для страницы заявок
            UserInfoWindow applicationsWindow = new UserInfoWindow(user, applications);
            applicationsWindow.Show();
            this.Close(); // Закрытие текущего окна
        }
    }
}