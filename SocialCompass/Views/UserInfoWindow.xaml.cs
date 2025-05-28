using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using SocialCompass.Models;
using SocialCompass.Views;

namespace SocialCompass
{
    public partial class UserInfoWindow : Window
    {
        private UserResponse user;
        private FeedbackResponse feedback;
        private List<ApplicationResponse> applications = new List<ApplicationResponse>();
        private List<FeedbackResponse> feedbacks = new List<FeedbackResponse>();
        private int currentApplicationIndex = 0;
        private DatePicker startDatePicker;
        private DatePicker endDatePicker;
        private ComboBox staffComboBox;

        public UserInfoWindow(UserResponse user, List<ApplicationResponse> applications)
        {
            InitializeComponent();
            this.user = user;
            LoadApplicationsAsync();

            if (applications.Count > 0)
            {
                UpdateApplicationDisplay();
                UpdateApplicationCounter();
            }
        }

        private async Task LoadApplicationsAsync()
        {
            try
            {
                var apiService = new ApiService();
                applications = await apiService.GetApplicationsAsync();

                if (applications.Count > 0)
                {
                    UpdateApplicationDisplay();
                    UpdateApplicationCounter();
                }
                else
                {
                    ApplicationContent.Content = new TextBlock
                    {
                        Text = "Нет активных заявок.",
                        FontSize = 18,
                        Foreground = Brushes.Gray,
                        HorizontalAlignment = HorizontalAlignment.Center,
                        VerticalAlignment = VerticalAlignment.Center,
                        Margin = new Thickness(20)
                    };
                }
            }
            catch (Exception ex)
            {
                ApplicationContent.Content = new TextBlock
                {
                    Text = "Не удалось загрузить заявки.",
                    FontSize = 18,
                    Foreground = Brushes.Red,
                    HorizontalAlignment = HorizontalAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Center,
                    Margin = new Thickness(20)
                };
            }
        }

        private void UpdateApplicationDisplay()
        {
            if (applications != null && applications.Count > 0 && currentApplicationIndex >= 0 && currentApplicationIndex < applications.Count)
            {
                ApplicationContent.Content = CreateApplicationUI(applications[currentApplicationIndex]);
                UpdateNavigationButtons();
            }
        }

        private UIElement CreateApplicationUI(ApplicationResponse application)
        {
            Grid mainGrid = new Grid { Margin = new Thickness(10) };

            mainGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            mainGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            mainGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            mainGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });


            TextBlock applicationIdBlock = new TextBlock
            {
                Text = $"Номер заявки: {application.ApplicationId}",
                FontSize = 20,
                FontWeight = FontWeights.Bold,
                HorizontalAlignment = HorizontalAlignment.Center,
                Margin = new Thickness(0, 10, 0, 10)
            };
            Grid.SetRow(applicationIdBlock, 0);
            mainGrid.Children.Add(applicationIdBlock);

            Grid userGrid = new Grid { Margin = new Thickness(10) };
            userGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(150) });
            userGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });

            string photoPath = string.IsNullOrEmpty(application.User.Photo) ? "D:\\Visual Studio\\Project\\SocialCompass\\SocialCompass\\Images\\default_photo.png" : application.User.Photo;

            Image userPhoto = new Image
            {
                Width = 390,
                Height = 224,
                Margin = new Thickness(10, 0, 20, 0),
                HorizontalAlignment = HorizontalAlignment.Left,
                Source = new BitmapImage(new Uri(photoPath, UriKind.Absolute))
            };
            Grid.SetColumn(userPhoto, 0);
            userGrid.Children.Add(userPhoto);

            StackPanel userDataPanel = new StackPanel { Margin = new Thickness(10, 0, 0, 0), Orientation = Orientation.Vertical };

            userDataPanel.Children.Add(CreateHorizontalRow("ФИО:", $"{application.User.Surname} {application.User.Name} {application.User.Patronymic}", false));
            userDataPanel.Children.Add(CreateHorizontalRow("Телефон:", application.User.PhoneNumber));
            userDataPanel.Children.Add(CreateHorizontalRow("Дата рождения:", application.User.Birthday));
            userDataPanel.Children.Add(CreateHorizontalRow("Серия паспорта:", application.User.PassportSeries));
            userDataPanel.Children.Add(CreateHorizontalRow("Номер паспорта:", application.User.PassportNumber));
            Grid.SetColumn(userDataPanel, 1);
            userGrid.Children.Add(userDataPanel);

            Grid.SetRow(userGrid, 1);
            mainGrid.Children.Add(userGrid);

            // Сетку для меток и значений
            Grid detailsGrid = new Grid { Margin = new Thickness(10) };
            detailsGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) }); // Левая колонка
            detailsGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) }); // Правая колонка

            int leftRow = 0, rightRow = 0;

            // Функция для добавления строк в детали
            void AddRow(string label, UIElement element, bool isLeftColumn)
            {
                detailsGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

                StackPanel rowPanel = new StackPanel { Orientation = Orientation.Horizontal };

                // Метка
                TextBlock labelBlock = new TextBlock
                {
                    Text = label,
                    FontSize = 14,
                    FontWeight = FontWeights.Bold,
                    Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#3E72AF")), // Новый цвет метки
                    Margin = new Thickness(20, 5, 5, 25)
                };

                // Проверяем длину значения
                if (element is TextBlock textBlock)
                {
                    bool shouldWrap = textBlock.Text.Length > 82;
                    textBlock.TextWrapping = shouldWrap ? TextWrapping.Wrap : TextWrapping.NoWrap;
                    textBlock.MaxWidth = shouldWrap ? 700 : double.PositiveInfinity;
                }

                rowPanel.Children.Add(labelBlock);
                rowPanel.Children.Add(element);

                int rowIndex = isLeftColumn ? leftRow++ : rightRow++;

                Grid.SetRow(rowPanel, rowIndex);
                Grid.SetColumn(rowPanel, isLeftColumn ? 0 : 1);
                detailsGrid.Children.Add(rowPanel);
            }

            startDatePicker = CreateStyledDatePicker(application.DateStart);
            endDatePicker = CreateStyledDatePicker(application.DateEnd);

            // Левые поля
            AddRow("Кто выдал:", new TextBlock { Text = application.User.WhoGave, FontSize = 14, Foreground = Brushes.Black, Margin = new Thickness(0, 5, 0, 5) }, true);
            AddRow("Когда выдан:", new TextBlock { Text = application.User.WhenGet, FontSize = 14, Foreground = Brushes.Black, Margin = new Thickness(0, 5, 0, 5) }, true);
            AddRow("Код подразделения:", new TextBlock {Text = application.User.DepartmentCode, FontSize = 14, Foreground = Brushes.Black, Margin = new Thickness(0, 5, 0, 5) }, true);
            AddRow("Адрес:", new TextBlock {Text = application.User.Address, FontSize = 14, Foreground = Brushes.Black, Margin = new Thickness(0, 5, 0, 5) }, true);
            AddRow("Категория инвалидности:", new TextBlock {Text = application.User.DisabilityCategory, FontSize = 14, Foreground = Brushes.Black, Margin = new Thickness(0, 5, 0, 5) }, true);
            
            string userCivilCategories = string.Join(", ", application.UserCivilCategories);
            AddRow("Категория гражданина:", new TextBlock { Text = userCivilCategories, FontSize = 14, Foreground = Brushes.Black, Margin = new Thickness(0, 5, 0, 5) }, true);
            
            AddRow("Размер пенсии:", new TextBlock { Text = application.User.PensionAmount.ToString() + " руб.", FontSize = 14, Foreground = Brushes.Black, Margin = new Thickness(0, 5, 0, 5) }, true);
            AddRow("Период услуги:", new TextBlock { Text = application.ApplicationDuration, FontSize = 14, Foreground = Brushes.Black, Margin = new Thickness(0, 5, 0, 5) }, true);

            // Правые поля
            AddRow("Семейный статус:", new TextBlock {Text = application.User.FamilyStatus, FontSize = 14, Foreground = Brushes.Black, Margin = new Thickness(0, 5, 0, 5) }, false);
            
            string applicationServises = string.Join(", ", application.ApplicationServices ?? new List<string>());
            AddRow("Услуга:", new TextBlock {Text = applicationServises, FontSize = 14, Foreground = Brushes.Black, Margin = new Thickness(0, 5, 0, 5) }, false);

            // Добавляем DatePicker для изменения даты начала и окончания
            AddRow("Дата начала:", startDatePicker, false);
            AddRow("Дата окончания:", endDatePicker, false);

            AddRow("Статус реабилитации:", new TextBlock {Text = application.IsHaveReabilitation.ToString(), FontSize = 14, Foreground = Brushes.Black, Margin = new Thickness(0, 5, 0, 5) }, false);
            string staffInfo = application.Staff != null 
                ? $"{application.Staff.Surname} {application.Staff.Name} {application.Staff.Patronymic}"
    :           "Работник отсутствует";

            string diseases = string.Join(", ", application.ExistingDiseases);
            AddRow("Заболевания:", new TextBlock { Text = diseases, FontSize = 14, Foreground = Brushes.Black, Margin = new Thickness(0, 5, 0, 5) }, false);

            

            staffComboBox = new ComboBox
            {
                Width = 300,
                Margin = new Thickness(0, 0, 0, 20),
                FontSize = 14,
                Foreground = Brushes.Black
            };

            // Загружаем всех сотрудников и устанавливаем выбранного
            _ = LoadStaffsAsync(staffComboBox, application.Staff?.Id ?? 0);

            AddRow("Работник:", staffComboBox, false);

            // Добавляем детали в основной Grid
            Grid.SetRow(detailsGrid, 2);
            mainGrid.Children.Add(detailsGrid);

            // Создаем контейнер для кнопок
            StackPanel buttonPanel = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                HorizontalAlignment = HorizontalAlignment.Center,
                Margin = new Thickness(0, 0, 0, 0)
            };

            // Кнопка "Отклонить"
            Button rejectButton = new Button
            {
                Content = "Отклонить",
                Width = 120,
                Height = 45,
                Background = Brushes.Red,
                Margin = new Thickness(0, 0, 100 , 0)
            };
            rejectButton.SetResourceReference(Control.StyleProperty, "CustomButtonStyle");
            rejectButton.Click += (sender, e) => RejectApplication(application.ApplicationId);

            // Кнопка "Подтвердить"
            Button confirmButton = new Button
            {
                Content = "Подтвердить",
                Width = 120,
                Height = 45,
                Background = Brushes.Green,
                Margin = new Thickness(0, 0, 0, 0)
            };
            confirmButton.SetResourceReference(Control.StyleProperty, "CustomButtonStyle");
            confirmButton.Click += (sender, e) => ConfirmApplication(application.ApplicationId);

            TextBlock applicationCounter = new TextBlock
            {
                Text = $"{currentApplicationIndex + 1} / {applications.Count}",
                FontSize = 16,
                Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#3E72AF")),
                FontWeight = FontWeights.Bold,
                HorizontalAlignment = HorizontalAlignment.Center,
                Margin = new Thickness(0, 40, 0, 0)
            };
            Grid.SetRow(applicationCounter, 4);
            mainGrid.Children.Add(applicationCounter);

            // Добавляем кнопки в панель
            buttonPanel.Children.Add(rejectButton);
            buttonPanel.Children.Add(confirmButton);

            // Добавляем панель кнопок в основной Grid
            Grid.SetRow(buttonPanel, 3);
            mainGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            mainGrid.Children.Add(buttonPanel);

            return new ScrollViewer { Content = mainGrid, VerticalScrollBarVisibility = ScrollBarVisibility.Auto };
        }

        private async Task LoadStaffsAsync(ComboBox staffComboBox, int selectedStaffId)
        {
            try
            {
                var apiService = new ApiService();
                var staffs = await apiService.GetStaffsAsync();

                staffComboBox.Items.Clear(); // Очищаем ComboBox перед добавлением новых элементов

                staffComboBox.DisplayMemberPath = "DisplayName";

                foreach (var staff in staffs)
                {
                    staffComboBox.Items.Add(new
                    {
                        DisplayName = $"{staff.Surname} {staff.Name} {staff.Patronymic}",
                        Id = staff.Id
                    });
                }

                // Устанавливаем текущего сотрудника как выбранного
                var selectedStaff = staffs.FirstOrDefault(s => s.Id == selectedStaffId);
                if (selectedStaff != null)
                {
                    staffComboBox.SelectedItem = staffComboBox.Items
                        .Cast<dynamic>()
                        .FirstOrDefault(item => item.Id == selectedStaff.Id);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при загрузке сотрудников: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private DatePicker CreateStyledDatePicker(string date)
        {
            DatePicker datePicker = new DatePicker
            {
                SelectedDateFormat = DatePickerFormat.Long, // Для корректного отображения выбранной даты
                Width = 120,
                Margin = new Thickness(0, 0, 0, 20),
                FontSize = 14,
                Foreground = Brushes.Black,
                VerticalContentAlignment = VerticalAlignment.Center,
                HorizontalContentAlignment = HorizontalAlignment.Center
            };

            // Преобразуем строку даты в DateTime
            if (DateTime.TryParse(date, out DateTime parsedDate))
            {
                datePicker.SelectedDate = parsedDate;
            }

            // Программно задаём формат даты
            datePicker.Loaded += (s, e) =>
            {
                if (datePicker.Template.FindName("PART_TextBox", datePicker) is TextBox textBox)
                {
                    textBox.Text = parsedDate.ToString("yyyy-MM-dd");
                    textBox.LostFocus += (s2, e2) =>
                    {
                        if (datePicker.SelectedDate.HasValue)
                        {
                            textBox.Text = datePicker.SelectedDate.Value.ToString("yyyy-MM-dd");
                        }
                    };
                }
            };

            return datePicker;
        }

        // Метод для создания DatePicker
        private DatePicker CreateDatePicker(string date)
        {
            return new DatePicker
            {
                SelectedDate = DateTime.TryParse(date, out DateTime parsedDate) ? parsedDate : (DateTime?)null,
                Width = 150,
                Margin = new Thickness(5)
            };
        }

        private void RejectApplication(int applicationId)
        {
            var rejectWindow = new RejectApplicationReasonWindow(applicationId);
            if (rejectWindow.ShowDialog() == true)
            {
                // Удаляем заявку из списка
                applications.RemoveAll(a => a.ApplicationId == applicationId);
                if (applications.Count == 0)
                {
                    LoadApplicationsAsync();
                }
                else
                {
                    currentApplicationIndex = Math.Min(currentApplicationIndex, applications.Count - 1);
                    UpdateApplicationDisplay();
                    UpdateApplicationCounter();
                }
            }
        }

        private async void ConfirmApplication(int applicationId)
        {
            var selectedStaff = staffComboBox.SelectedItem as dynamic;
            int? staffId = selectedStaff?.Id;

            string newStartDate = startDatePicker.SelectedDate?.ToString("yyyy-MM-dd");
            string newEndDate = endDatePicker.SelectedDate?.ToString("yyyy-MM-dd");

            try
            {
                var apiService = new ApiService();
                await apiService.UpdateApplicationAsync(applicationId, newStartDate, newEndDate, staffId);

                await LoadApplicationsAsync();

                if (applications.Count == 0)
                {
                    currentApplicationIndex = -1;
                }
                else
                {
                    if (currentApplicationIndex >= applications.Count)
                    {
                        currentApplicationIndex = applications.Count - 1;
                    }
                }

                UpdateApplicationDisplay();
                UpdateApplicationCounter();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при подтверждении заявки: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // Метод для создания горизонтального ряда
        private StackPanel CreateHorizontalRow(string label, string value, bool allowWrap = true)
        {
            StackPanel rowPanel = new StackPanel { Orientation = Orientation.Horizontal };

            // Метка
            TextBlock labelBlock = new TextBlock
            {
                Text = label,
                FontSize = 14,
                FontWeight = FontWeights.Bold,
                Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#3E72AF")),
                Margin = new Thickness(0, 5, 5, 20)
            };

            // Значение
            TextBlock valueBlock = new TextBlock
            {
                Text = value,
                FontSize = 14,
                Foreground = Brushes.Black,
                Margin = new Thickness(0, 5, 0, 5),
                TextWrapping = allowWrap ? TextWrapping.Wrap : TextWrapping.NoWrap, // Отключаем перенос строк
                TextTrimming = TextTrimming.CharacterEllipsis, // Добавляет "..." если текст не влезает
                MaxWidth = allowWrap ? 200 : double.PositiveInfinity // Убираем ограничение ширины
            };

            rowPanel.Children.Add(labelBlock);
            rowPanel.Children.Add(valueBlock);

            return rowPanel;
        }

        private void NextButton_Click(object sender, RoutedEventArgs e)
        {
            if (currentApplicationIndex < applications.Count - 1)
            {
                currentApplicationIndex++;
                UpdateApplicationDisplay();
                UpdateApplicationCounter();
            }
        }

        private void PreviousButton_Click(object sender, RoutedEventArgs e)
        {
            if (currentApplicationIndex > 0)
            {
                currentApplicationIndex--;
                UpdateApplicationDisplay();
                UpdateApplicationCounter();
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

        private void UpdateApplicationCounter()
        {
            if (ApplicationContent.Content is ScrollViewer scrollViewer &&
                scrollViewer.Content is Grid mainGrid &&
                mainGrid.Children.OfType<TextBlock>().FirstOrDefault(tb => tb.Text.Contains("/")) is TextBlock counter)
            {
                counter.Text = $"{currentApplicationIndex + 1} / {applications.Count}";
            }
        }

        private void UpdateNavigationButtons()
        {
            PreviousButton.Visibility = currentApplicationIndex == 0 ? Visibility.Collapsed : Visibility.Visible;
            NextButton.Visibility = currentApplicationIndex == applications.Count - 1 ? Visibility.Collapsed : Visibility.Visible;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}
