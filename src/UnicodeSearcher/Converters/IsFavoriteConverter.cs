using System.Collections.ObjectModel;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace UnicodeSearcher.Converters;

/// <summary>
/// 문자가 즐겨찾기에 포함되어 있는지 확인하여 Visibility 반환
/// </summary>
public class IsFavoriteConverter : IMultiValueConverter
{
    public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
    {
        if (values.Length < 2 || values[0] == null || values[1] == null)
            return Visibility.Collapsed;

        var charValue = values[0] as string;
        var favorites = values[1] as ObservableCollection<string>;

        if (charValue == null || favorites == null)
            return Visibility.Collapsed;

        return favorites.Contains(charValue) ? Visibility.Visible : Visibility.Collapsed;
    }

    public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
