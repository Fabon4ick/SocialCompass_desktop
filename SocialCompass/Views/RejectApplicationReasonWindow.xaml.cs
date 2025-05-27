using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace SocialCompass.Views
{
    /// <summary>
    /// Логика взаимодействия для RejectApplicationReasonWindow.xaml
    /// </summary>
    public partial class RejectApplicationReasonWindow : Window
    {

        private readonly int _applicationId;
        private readonly ApiService _apiService = new ApiService();

        public RejectApplicationReasonWindow(int applicationId)
        {
            InitializeComponent();
            _applicationId = applicationId;
            LoadReasons();
        }

        private async void LoadReasons()
        {
            try
            {
                var reasons = await _apiService.GetRejectionReasonsAsync();
                ReasonComboBox.ItemsSource = reasons;
                ReasonComboBox.DisplayMemberPath = "Name";
                ReasonComboBox.SelectedValuePath = "Id";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при загрузке причин отказа: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async void Confirm_Click(object sender, RoutedEventArgs e)
        {
            if (ReasonComboBox.SelectedValue is int reasonId)
            {
                try
                {
                    var success = await _apiService.RejectApplicationWithReasonAsync(_applicationId, reasonId);

                    if (success)
                    {
                        MessageBox.Show("Заявка успешно отклонена", "Успешно", MessageBoxButton.OK, MessageBoxImage.Information);
                        DialogResult = true;
                        Close();
                    }
                    else
                    {
                        MessageBox.Show("Не удалось отклонить заявку", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка при отклонении заявки: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            else
            {
                MessageBox.Show("Выберите причину отказа", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }
    }
}
