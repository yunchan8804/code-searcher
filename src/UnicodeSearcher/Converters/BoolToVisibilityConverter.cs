using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace UnicodeSearcher.Converters;

/// <summary>
/// bool 값을 Visibility로 변환하는 컨버터
/// parameter가 "Inverse"이면 반전
/// </summary>
public class BoolToVisibilityConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is bool boolValue)
        {
            var inverse = parameter is string s && s.Equals("Inverse", StringComparison.OrdinalIgnoreCase);
            if (inverse) boolValue = !boolValue;
            return boolValue ? Visibility.Visible : Visibility.Collapsed;
        }
        return Visibility.Collapsed;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is Visibility visibility)
        {
            var result = visibility == Visibility.Visible;
            var inverse = parameter is string s && s.Equals("Inverse", StringComparison.OrdinalIgnoreCase);
            return inverse ? !result : result;
        }
        return false;
    }
}

/// <summary>
/// null 값을 Visibility로 변환하는 컨버터 (null이면 Collapsed)
/// </summary>
public class NullToVisibilityConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return value != null ? Visibility.Visible : Visibility.Collapsed;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

/// <summary>
/// Count 값을 Visibility로 변환하는 컨버터 (0이면 Collapsed)
/// </summary>
public class CountToVisibilityConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is int count)
        {
            return count > 0 ? Visibility.Visible : Visibility.Collapsed;
        }
        return Visibility.Collapsed;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
