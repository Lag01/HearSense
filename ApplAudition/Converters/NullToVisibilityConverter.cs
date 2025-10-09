using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace ApplAudition.Converters;

/// <summary>
/// Convertit une valeur null en Visibility (null = Collapsed, non-null = Visible).
/// Supporte l'inversion via le paramètre "Invert".
/// </summary>
public class NullToVisibilityConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        bool isNotNull = value != null;

        // Inversion si le paramètre est "Invert"
        if (parameter is string param && param.Equals("Invert", StringComparison.OrdinalIgnoreCase))
        {
            isNotNull = !isNotNull;
        }

        return isNotNull ? Visibility.Visible : Visibility.Collapsed;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
