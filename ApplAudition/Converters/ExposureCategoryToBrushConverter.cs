using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;
using ApplAudition.Models;

namespace ApplAudition.Converters;

/// <summary>
/// Convertisseur pour mapper ExposureCategory vers une couleur (Phase 6 - Tâche 14).
/// Safe → Vert, Moderate → Orange, Hazardous → Rouge.
/// </summary>
public class ExposureCategoryToBrushConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is not ExposureCategory category)
            return Brushes.Gray;

        return category switch
        {
            ExposureCategory.Safe => new SolidColorBrush(Color.FromRgb(76, 175, 80)), // Vert Material Design
            ExposureCategory.Moderate => new SolidColorBrush(Color.FromRgb(255, 152, 0)), // Orange Material Design
            ExposureCategory.Hazardous => new SolidColorBrush(Color.FromRgb(244, 67, 54)), // Rouge Material Design
            _ => Brushes.Gray
        };
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
