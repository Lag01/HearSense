using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;
using HearSense.Models;

namespace HearSense.Converters;

/// <summary>
/// Convertisseur pour mapper ExposureCategory vers une couleur (Phase 6 - Tâche 14).
/// Safe → Vert, Moderate → Orange, Hazardous → Rouge, Critical → Rouge très foncé.
/// </summary>
public class ExposureCategoryToBrushConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is not ExposureCategory category)
            return System.Windows.Media.Brushes.Gray;

        return category switch
        {
            ExposureCategory.Safe => new SolidColorBrush(System.Windows.Media.Color.FromRgb(76, 175, 80)), // Vert Material Design
            ExposureCategory.Moderate => new SolidColorBrush(System.Windows.Media.Color.FromRgb(255, 152, 0)), // Orange Material Design
            ExposureCategory.Hazardous => new SolidColorBrush(System.Windows.Media.Color.FromRgb(244, 67, 54)), // Rouge Material Design
            ExposureCategory.Critical => new SolidColorBrush(System.Windows.Media.Color.FromRgb(183, 28, 28)), // Rouge très foncé Material Design
            _ => System.Windows.Media.Brushes.Gray
        };
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
