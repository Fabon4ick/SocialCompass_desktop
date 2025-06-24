using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Office.Interop.Word;
using SocialCompass.Models;
using SocialCompass.Views;
using System.IO;
using System.Reflection;

namespace SocialCompass
{
    public partial class StaffWindow : System.Windows.Window
    {
        private readonly ApiService _apiService; // Инициализация сервиса API
        public ObservableCollection<StaffResponse> StaffList { get; set; }
        private UserResponse user;
        private FeedbackResponse feedback;
        private List<ApplicationResponse> applications = new List<ApplicationResponse>();
        private List<FeedbackResponse> feedbacks = new List<FeedbackResponse>();

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
                    LoadStaffs(); // Перезагружаем список сотрудников
                }
                else
                {
                    MessageBox.Show("Ошибка при добавлении сотрудника.");
                }
            }
        }

        private async void GenerateReport_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Получаем все необходимые данные
                var staffs = await _apiService.GetStaffsAsync();
                var applications = await _apiService.GetActiveApplicationsAsync();
                var users = await _apiService.GetUsersAsync();
                var feedbacks = await _apiService.GetVisibleFeedbacksAsync();
                const int FIXED_VISITS_PER_CLIENT = 10; // Фиксированное число посещений на клиента в месяц
                const decimal PAYMENT_PER_CLIENT = 5000m;

                // Добавляем новые методы в ApiService для получения этих данных
                var diseases = await _apiService.GetDiseasesAsync();
                var disabilityCategories = await _apiService.GetDisabilityCategoriesAsync();

                // Создаем Word-документ
                var wordApp = new Microsoft.Office.Interop.Word.Application();
                var document = wordApp.Documents.Add();
                wordApp.Visible = true;

                // Добавляем заголовок отчета
                var paragraph = document.Content.Paragraphs.Add();
                paragraph.Range.Text = "Отчет по заработной плате сотрудников";
                paragraph.Range.Font.Bold = 1;
                paragraph.Range.Font.Size = 16;
                paragraph.Alignment = WdParagraphAlignment.wdAlignParagraphCenter;
                paragraph.Range.InsertParagraphAfter();

                // Добавляем дату генерации
                paragraph = document.Content.Paragraphs.Add();
                paragraph.Range.Text = $"Дата генерации: {DateTime.Now.ToString("dd.MM.yyyy HH:mm")}";
                paragraph.Range.Font.Italic = 1;
                paragraph.Range.Font.Size = 10;
                paragraph.Alignment = WdParagraphAlignment.wdAlignParagraphRight;
                paragraph.Range.InsertParagraphAfter();

                // Добавляем таблицу с данными
                var table = document.Tables.Add(document.Range(document.Content.End - 1), staffs.Count + 1, 7);
                table.Borders.Enable = 1;

                // Заголовки таблицы
                table.Cell(1, 1).Range.Text = "Сотрудник";
                table.Cell(1, 2).Range.Text = "Заявки";
                table.Cell(1, 3).Range.Text = "Количество посещений";
                table.Cell(1, 4).Range.Text = "Рейтинг";
                table.Cell(1, 5).Range.Text = "Базовая ЗП";
                table.Cell(1, 6).Range.Text = "Надбавки";
                table.Cell(1, 7).Range.Text = "Итого";

                // Форматирование заголовков
                for (int i = 1; i <= 7; i++)
                {
                    table.Cell(1, i).Range.Font.Bold = 1;
                    table.Cell(1, i).Range.ParagraphFormat.Alignment = WdParagraphAlignment.wdAlignParagraphCenter;
                }

                // Заполняем данные по сотрудникам
                int row = 2;
                foreach (var staff in staffs)
                {
                    // Изменяем условие для фильтрации заявок
                    var staffApps = applications.Where(a => a.Staff != null && a.Staff.Id == staff.Id).ToList();
                    var staffFeedbacks = feedbacks.Where(f => f.Staff != null && f.Staff.Id == staff.Id).ToList();

                    double avgRating = staffFeedbacks.Any() ? staffFeedbacks.Average(f => f.Rating) : 0;
                    int clientCount = staffApps.Count;

                    int plannedVisits = clientCount * FIXED_VISITS_PER_CLIENT;
                    decimal baseSalary = clientCount * PAYMENT_PER_CLIENT;
                    decimal bonuses = CalculateBonuses(staffApps, users, diseases, disabilityCategories);

                    decimal total = (baseSalary + bonuses) * (decimal)GetRatingMultiplier(avgRating);

                    // Заполнение строки
                    table.Cell(row, 1).Range.Text = $"{staff.Surname} {staff.Name}";
                    table.Cell(row, 2).Range.Text = clientCount.ToString();
                    table.Cell(row, 3).Range.Text = plannedVisits.ToString();
                    table.Cell(row, 4).Range.Text = avgRating.ToString("0.00");
                    table.Cell(row, 5).Range.Text = baseSalary.ToString("C");
                    table.Cell(row, 6).Range.Text = bonuses.ToString("C");
                    table.Cell(row, 7).Range.Text = total.ToString("C");

                    row++;
                }

                // Автоподбор ширины столбцов
                table.Columns.AutoFit();

                // Добавляем итоговую информацию
                paragraph = document.Content.Paragraphs.Add();
                paragraph.Range.Text = $"Всего сотрудников: {staffs.Count}";
                paragraph.Range.InsertParagraphAfter();

                // Правильный расчет общей суммы зарплат
                decimal totalSalary = staffs.Sum(s => {
                    var sa = applications.Where(a => a.Staff != null && a.Staff.Id == s.Id).ToList();
                    var sf = feedbacks.Where(f => f.Staff != null && f.Staff.Id == s.Id).ToList();

                    double avgRating = sf.Any() ? sf.Average(f => f.Rating) : 0;
                    int clientCount = sa.Count;

                    decimal baseSalary = clientCount * PAYMENT_PER_CLIENT;
                    decimal bonuses = CalculateBonuses(sa, users, diseases, disabilityCategories);

                    return (baseSalary + bonuses) * (decimal)GetRatingMultiplier(avgRating);
                });

                paragraph = document.Content.Paragraphs.Add();
                paragraph.Range.Text = $"Общая сумма зарплат: {totalSalary.ToString("C")}";
                paragraph.Range.Font.Bold = 1;

                // Сохраняем документ
                string fileName = $"ОтчётПоЗП_{DateTime.Now:yyyyMMdd_HHmmss}.docx";
                string path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), fileName);
                document.SaveAs2(path);

                MessageBox.Show($"Отчет сохранен в: {path}", "Отчет создан", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при создании отчета: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // Методы для расчета (добавьте их в класс StaffWindow)
        private decimal CalculateBonuses(List<ApplicationResponse> apps, List<UserResponse> users,
        List<DiseaseResponse> diseases, List<DisabilityCategorieResponse> disabilityCategories)
        {
            decimal totalBonus = 0;

            foreach (var app in apps)
            {
                if (app.User == null) continue;

                // Надбавки рассчитываются на клиента (не зависят от количества посещений)
                if (app.User?.DisabilityCategory != null) // Проверяем и null, и пустую строку
                {
                    var disability = disabilityCategories.FirstOrDefault(d =>
                        d.Name.Equals(app.User.DisabilityCategory, StringComparison.OrdinalIgnoreCase));

                    if (disability != null)
                    {
                        switch (disability.Name)
                        {
                            case "Инвалид первой группы": totalBonus += 300; break;
                            case "Инвалид второй группы": totalBonus += 200; break;
                            case "Инвалид третьей группы": totalBonus += 100; break;
                            case "Ребёнок инвалид": totalBonus += 250; break;
                        }
                    }
                }

                if (app.IsHaveReabilitation == "Да")
                    totalBonus += 70;

                if (app.ExistingDiseases != null && app.ExistingDiseases.Any())
                    totalBonus += 30;
            }

            return totalBonus;
        }

        private double GetRatingMultiplier(double rating)
        {
            if (rating == 0) return 1.0;
            if (rating >= 4.5) return 1.10;
            if (rating >= 3.5) return 1.05;
            if (rating >= 2.5) return 1.0;
            if (rating >= 1.5) return 0.95;
            return 0.90;
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
                                MessageBox.Show("Сотрудник успешно удалён и заявки изменены!");
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

        private void OpenUserCommentPage_Click(object sender, RoutedEventArgs e)
        {
            UserCommentWindow userCommentWindow = new UserCommentWindow(feedback, feedbacks);
            userCommentWindow.Show();
            this.Close();
        }
    }
}
