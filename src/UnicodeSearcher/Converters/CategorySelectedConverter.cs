using System.Globalization;
using System.Windows.Data;
using UnicodeSearcher.Models;

namespace UnicodeSearcher.Converters;

/// <summary>
/// 현재 카테고리와 선택된 카테고리를 비교하여 선택 여부를 반환
/// </summary>
public class CategorySelectedConverter : IMultiValueConverter
{
    public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
    {
        if (values.Length < 2 || values[0] == null || values[1] == null)
            return false;

        var category = values[0] as Category;
        var selectedCategory = values[1] as Category;

        return category?.Id == selectedCategory?.Id;
    }

    public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
