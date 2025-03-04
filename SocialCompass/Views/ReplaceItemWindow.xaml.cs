using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using SocialCompass.Models;

namespace SocialCompass.Views
{
    public partial class ReplaceItemWindow : Window
    {
        private readonly ApiService _apiService;
        private readonly string _tableName;
        private readonly int _itemId;
        public int? SelectedReplacementId { get; private set; }

        public ReplaceItemWindow(string tableName, int itemId)
        {
            InitializeComponent();
            _apiService = new ApiService();
            _tableName = tableName;
            _itemId = itemId;
            LoadItems();
        }

        private async void LoadItems()
        {
            try
            {
                var items = await _apiService.GetItemsAsync<EntityItem>(_tableName);
                ReplacementComboBox.ItemsSource = items.Where(i => i.Id != _itemId).ToList();
                ReplacementComboBox.DisplayMemberPath = "Name";
                ReplacementComboBox.SelectedValuePath = "Id";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при загрузке данных: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async void ConfirmReplacement_Click(object sender, RoutedEventArgs e)
        {
            if (ReplacementComboBox.SelectedValue is int replacementId)
            {
                try
                {
                    bool isReplaced = await _apiService.ReplaceItemAsync(_tableName, _itemId, replacementId);
                    if (isReplaced)
                    {
                        SelectedReplacementId = replacementId;
                        DialogResult = true;
                        Close();

                        // 🔥 Передаём tableName, чтобы данные обновились
                        if (Application.Current.Windows.OfType<AddDeleteItemsWindow>().FirstOrDefault() is AddDeleteItemsWindow mainWindow)
                        {
                            mainWindow.RefreshEntityData(_tableName);
                        }
                    }
                    else
                    {
                        MessageBox.Show("Ошибка при замене!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка при замене: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            else
            {
                MessageBox.Show("Выберите замену!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }
    }
}
