using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace ApplAudition.Converters;

/// <summary>
/// Convertit un booléen en Visibility (true = Visible, false = Collapsed).
/// Supporte l'inversion via le paramètre "Invert".
/// </summary>
public class BooleanToVisibilityConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        bool isVisible = value is bool boolValue && boolValue;

        // Inversion si le paramètre est "Invert"
        if (parameter is string param && param.Equals("Invert", StringComparison.OrdinalIgnoreCase))
        {
            isVisible = !isVisible;
        }

        return isVisible ? Visibility.Visible : Visibility.Collapsed;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        bool isVisible = value is Visibility visibility && visibility == Visibility.Visible;

        // Inversion si le paramètre est "Invert"
        if (parameter is string param && param.Equals("Invert", StringComparison.OrdinalIgnoreCase))
        {
            isVisible = !isVisible;
        }

        return isVisible;
    }
}
