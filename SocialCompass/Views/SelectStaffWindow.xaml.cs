using System.Collections.Generic;
using System.Linq;
using System.Windows;

namespace SocialCompass.Views
{
    public partial class SelectStaffWindow : Window
    {
        public int SelectedStaffId { get; private set; }

        public SelectStaffWindow(List<StaffResponse> availableStaffs)
        {
            InitializeComponent();

            // Заполнение ComboBox данными о сотрудниках
            StaffComboBox.ItemsSource = availableStaffs;
            StaffComboBox.DisplayMemberPath = "FullName";  // Убедитесь, что у класса StaffResponse есть свойство FullName
            StaffComboBox.SelectedValuePath = "Id"; // Указываем, что значение элемента будет его Id
        }

        private void Confirm_Click(object sender, RoutedEventArgs e)
        {
            if (StaffComboBox.SelectedItem is StaffResponse selectedStaff)
            {
                SelectedStaffId = selectedStaff.Id;
                DialogResult = true;
                Close();
            }
            else
            {
                MessageBox.Show("Пожалуйста, выберите сотрудника для замены.");
            }
        }
    }
}

