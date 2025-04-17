using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using SocialCompass.Models;

namespace SocialCompass.Views
{
    public partial class UserCommentWindow : Window
    {
        private UserResponse user;
        private FeedbackResponse feedback;
        private List<ApplicationResponse> applications = new List<ApplicationResponse>();
        private List<FeedbackResponse> feedbacks = new List<FeedbackResponse>();
        private int currentFeedbackIndex = 0;

        public UserCommentWindow(FeedbackResponse feedback, List<FeedbackResponse> feedbacks)
        {
            InitializeComponent();
            this.feedback = feedback;
            LoadCommentAsync();

            if (feedbacks.Count > 0)
            {
                UpdateFeedbackDisplay();
                UpdateFeedbackCounter();
            }
        }

        private async Task LoadCommentAsync()
        {
            try
            {
                var apiService = new ApiService();
                feedbacks = await apiService.GetFeedbacksAsync();
                if (feedbacks.Count > 0)
                {
                    UpdateFeedbackDisplay();
                    UpdateFeedbackCounter();
                }
                else
                {
                    MessageBox.Show("Нет активных комментариев.", "Информация", MessageBoxButton.OK, MessageBoxImage.Information);

                    FeedbacksContent.Content = new TextBlock
                    {
                        Text = "Нет активных комментариев.",
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
                MessageBox.Show($"Ошибка при загрузке комментариев: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);

                FeedbacksContent.Content = new TextBlock
                {
                    Text = "Не удалось загрузить комментарии.",
                    FontSize = 18,
                    Foreground = Brushes.Red,
                    HorizontalAlignment = HorizontalAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Center,
                    Margin = new Thickness(20)
                };
            }
        }

        private void UpdateFeedbackDisplay()
        {
            if (feedbacks.Count > 0)
            {
                FeedbacksContent.Content = CreateFeedbackUI(feedbacks[currentFeedbackIndex]);
                UpdateNavigationButtons();
            }
        }

        private UIElement CreateFeedbackUI(FeedbackResponse feedback)
        {
            Grid mainGrid = new Grid { Margin = new Thickness(10) };

            mainGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto }); // ID
            mainGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto }); // Клиент + Фото
            mainGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto }); // Работник
            mainGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto }); // Отзыв и оценка
            mainGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto }); // Кнопки
            

            // --- ID комментария ---
            TextBlock commentIdBlock = new TextBlock
            {
                Text = $"Номер комментария: {feedback.CommentId}",
                FontSize = 20,
                FontWeight = FontWeights.Bold,
                HorizontalAlignment = HorizontalAlignment.Center,
                Margin = new Thickness(0, 10, 0, 10)
            };
            Grid.SetRow(commentIdBlock, 0);
            mainGrid.Children.Add(commentIdBlock);

            // --- Клиент (с фото) ---
            Grid combinedGrid = new Grid { Margin = new Thickness(10) };
            combinedGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(150) }); // Фото
            combinedGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) }); // Текст

            string photoPath = string.IsNullOrEmpty(feedback.User.Photo)
                ? "D:\\Visual Studio\\Project\\SocialCompass\\SocialCompass\\Images\\default_photo.png"
                : feedback.User.Photo;

            Image userPhoto = new Image
            {
                Width = 390,
                Height = 224,
                Margin = new Thickness(10, 0, 20, 0),
                HorizontalAlignment = HorizontalAlignment.Left,
                Source = new BitmapImage(new Uri(photoPath, UriKind.Absolute))
            };
            Grid.SetColumn(userPhoto, 0);
            combinedGrid.Children.Add(userPhoto);

            // ФИО клиента и работника по центру относительно фото
            StackPanel infoPanel = new StackPanel
            {
                VerticalAlignment = VerticalAlignment.Center,
                Orientation = Orientation.Vertical
            };
            infoPanel.Children.Add(CreateHorizontalRow("ФИО клиента:", $"{feedback.User.Surname} {feedback.User.Name} {feedback.User.Patronymic}", false));
            infoPanel.Children.Add(CreateHorizontalRow("ФИО работника:", $"{feedback.Staff.Surname} {feedback.Staff.Name} {feedback.Staff.Patronymic}", false));

            Grid.SetColumn(infoPanel, 1);
            combinedGrid.Children.Add(infoPanel);

            Grid.SetRow(combinedGrid, 1);
            mainGrid.Children.Add(combinedGrid);

            // --- Отзыв и оценка ---
            Grid feedbackGrid = new Grid { Margin = new Thickness(10) };
            feedbackGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });

            StackPanel feedbackPanel = new StackPanel { Orientation = Orientation.Vertical };

            feedbackPanel.Children.Add(CreateHorizontalRow("Оценка:", $"{feedback.Rating}/5", false));
            feedbackPanel.Children.Add(CreateHorizontalRow("Отзыв:", feedback.Comment, true));
            
            Grid.SetColumn(feedbackPanel, 0);
            feedbackGrid.Children.Add(feedbackPanel);

            Grid.SetRow(feedbackGrid, 3);
            mainGrid.Children.Add(feedbackGrid);

            // --- Кнопки ---
            StackPanel buttonPanel = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                HorizontalAlignment = HorizontalAlignment.Center,
                Margin = new Thickness(0, 0, 0, 0)
            };

            Button rejectButton = new Button
            {
                Content = "Отклонить",
                Width = 120,
                Height = 45,
                Background = Brushes.Red,
                Margin = new Thickness(0, 0, 100, 0)
            };
            rejectButton.SetResourceReference(Control.StyleProperty, "CustomButtonStyle");
            rejectButton.Click += (sender, e) => RejectFeedback(feedback.CommentId);

            Button confirmButton = new Button
            {
                Content = "Подтвердить",
                Width = 120,
                Height = 45,
                Background = Brushes.Green,
                Margin = new Thickness(0, 0, 0, 0)
            };
            confirmButton.SetResourceReference(Control.StyleProperty, "CustomButtonStyle");
            confirmButton.Click += (sender, e) => ConfirmFeedback(feedback.CommentId);

            buttonPanel.Children.Add(rejectButton);
            buttonPanel.Children.Add(confirmButton);

            Grid.SetRow(buttonPanel, 4);
            mainGrid.Children.Add(buttonPanel);

            // --- Счётчик комментариев ---
            TextBlock commentCounter = new TextBlock
            {
                Text = $"{currentFeedbackIndex + 1} / {feedbacks.Count}",
                FontSize = 16,
                Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#3E72AF")),
                FontWeight = FontWeights.Bold,
                HorizontalAlignment = HorizontalAlignment.Center,
                Margin = new Thickness(0, 40, 0, 0)
            };
            mainGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            Grid.SetRow(commentCounter, 5);
            mainGrid.Children.Add(commentCounter);

            return new ScrollViewer
            {
                Content = mainGrid,
                VerticalScrollBarVisibility = ScrollBarVisibility.Auto
            };
        }

        private StackPanel CreateHorizontalRow(string label, string value, bool allowWrap = false)
        {
            StackPanel rowPanel = new StackPanel { Orientation = Orientation.Horizontal };

            TextBlock labelBlock = new TextBlock
            {
                Text = label,
                FontSize = 14,
                FontWeight = FontWeights.Bold,
                Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#3E72AF")),
                Margin = new Thickness(20, 5, 5, 15)
            };

            TextBlock valueBlock = new TextBlock
            {
                Text = value,
                FontSize = 14,
                Foreground = Brushes.Black,
                Margin = new Thickness(5, 5, 0, 15),
                TextWrapping = allowWrap ? TextWrapping.Wrap : TextWrapping.NoWrap,
                MaxWidth = allowWrap ? 700 : double.PositiveInfinity
            };

            rowPanel.Children.Add(labelBlock);
            rowPanel.Children.Add(valueBlock);

            return rowPanel;
        }

        private void UpdateFeedbackCounter()
        {
            if (FeedbacksContent.Content is ScrollViewer scrollViewer &&
                scrollViewer.Content is Grid mainGrid &&
                mainGrid.Children.OfType<TextBlock>().FirstOrDefault(tb => tb.Text.Contains("/")) is TextBlock counter)
            {
                counter.Text = $"{currentFeedbackIndex + 1} / {feedbacks.Count}";
            }
        }

        private void NextButton_Click(object sender, RoutedEventArgs e)
        {
            if (currentFeedbackIndex < feedbacks.Count - 1)
            {
                currentFeedbackIndex++;
                UpdateFeedbackDisplay();
                UpdateFeedbackCounter();
            }
        }

        private void PreviousButton_Click(object sender, RoutedEventArgs e)
        {
            if (currentFeedbackIndex > 0)
            {
                currentFeedbackIndex--;
                UpdateFeedbackDisplay();
                UpdateFeedbackCounter();
            }
        }

        private void UpdateNavigationButtons()
        {
            PreviousButton.Visibility = currentFeedbackIndex == 0 ? Visibility.Collapsed : Visibility.Visible;
            NextButton.Visibility = currentFeedbackIndex == feedbacks.Count - 1 ? Visibility.Collapsed : Visibility.Visible;
        }

        private async void ConfirmFeedback(int feedbackId)
        {
            try
            {
                var apiService = new ApiService();
                await apiService.UpdateFeedbackVisibilityAsync(feedbackId, false);

                MessageBox.Show($"Комментарий с ID {feedbackId} подтверждён!", "Успешно", MessageBoxButton.OK, MessageBoxImage.Information);

                // Удаляем подтверждённый комментарий из списка
                feedbacks.RemoveAt(currentFeedbackIndex);

                // Если комментарии закончились
                if (feedbacks.Count == 0)
                {
                    FeedbacksContent.Content = new TextBlock
                    {
                        Text = "Нет активных комментариев.",
                        FontSize = 18,
                        Foreground = Brushes.Gray,
                        HorizontalAlignment = HorizontalAlignment.Center,
                        VerticalAlignment = VerticalAlignment.Center,
                        Margin = new Thickness(20)
                    };
                    return;
                }

                // Корректировка индекса
                if (currentFeedbackIndex >= feedbacks.Count)
                {
                    currentFeedbackIndex = feedbacks.Count - 1;
                }

                UpdateFeedbackDisplay();
                UpdateFeedbackCounter();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при подтверждении комментария: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async void RejectFeedback(int feedbackId)
        {
            try
            {
                var apiService = new ApiService();
                await apiService.DeleteFeedbackAsync(feedbackId);
                MessageBox.Show($"Комментарий с ID {feedbackId} успешно удалён.", "Удаление", MessageBoxButton.OK, MessageBoxImage.Information);

                feedbacks.RemoveAt(currentFeedbackIndex);

                if (feedbacks.Count == 0)
                {
                    MessageBox.Show("Нет больше комментариев.", "Информация", MessageBoxButton.OK, MessageBoxImage.Information);

                    LoadCommentAsync();
                }

                if (currentFeedbackIndex >= feedbacks.Count)
                {
                    currentFeedbackIndex = feedbacks.Count - 1;
                }

                UpdateFeedbackDisplay();
                UpdateFeedbackCounter();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при удалении комментария: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void OpenApplicationsPage_Click(object sender, RoutedEventArgs e)
        {
            // Открываем новое окно для страницы заявок
            UserInfoWindow applicationsWindow = new UserInfoWindow(user, applications);
            applicationsWindow.Show();
            this.Close(); // Закрытие текущего окна
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
    }
}
