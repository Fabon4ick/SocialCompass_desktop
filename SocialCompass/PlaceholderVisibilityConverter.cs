using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace SocialCompass
{
    public class PlaceholderVisibilityConverter : IValueConverter
    {
        // Преобразует данные для управления видимостью плейсхолдера
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            // Показывать плейсхолдер, если поле пустое
            return string.IsNullOrEmpty(value?.ToString()) ? Visibility.Visible : Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
