using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using ApplAudition.Models;

namespace ApplAudition.Controls;

/// <summary>
/// UserControl représentant une jauge visuelle dB(A) avec code couleur (Phase 6 - Tâche 14).
/// </summary>
public partial class DbGauge : UserControl
{
    #region Dependency Properties

    public static readonly DependencyProperty ValueProperty =
        DependencyProperty.Register(
            nameof(Value),
            typeof(float),
            typeof(DbGauge),
            new PropertyMetadata(0f, OnValueChanged));

    public static readonly DependencyProperty CategoryProperty =
        DependencyProperty.Register(
            nameof(Category),
            typeof(ExposureCategory),
            typeof(DbGauge),
            new PropertyMetadata(ExposureCategory.Safe, OnCategoryChanged));

    public static readonly DependencyProperty ModeLabelProperty =
        DependencyProperty.Register(
            nameof(ModeLabel),
            typeof(string),
            typeof(DbGauge),
            new PropertyMetadata("dB(A) relatif"));

    #endregion

    #region Public Properties

    /// <summary>
    /// Valeur dB(A) actuelle.
    /// </summary>
    public float Value
    {
        get => (float)GetValue(ValueProperty);
        set => SetValue(ValueProperty, value);
    }

    /// <summary>
    /// Catégorie d'exposition (Safe/Moderate/Hazardous).
    /// </summary>
    public ExposureCategory Category
    {
        get => (ExposureCategory)GetValue(CategoryProperty);
        set => SetValue(CategoryProperty, value);
    }

    /// <summary>
    /// Label du mode d'estimation.
    /// </summary>
    public string ModeLabel
    {
        get => (string)GetValue(ModeLabelProperty);
        set => SetValue(ModeLabelProperty, value);
    }

    #endregion

    #region Internal Properties (pour binding XAML)

    public static readonly DependencyProperty ValueTextProperty =
        DependencyProperty.Register(nameof(ValueText), typeof(string), typeof(DbGauge), new PropertyMetadata("--"));

    public static readonly DependencyProperty CategoryTextProperty =
        DependencyProperty.Register(nameof(CategoryText), typeof(string), typeof(DbGauge), new PropertyMetadata("--"));

    public static readonly DependencyProperty CategoryBrushProperty =
        DependencyProperty.Register(nameof(CategoryBrush), typeof(Brush), typeof(DbGauge), new PropertyMetadata(Brushes.Gray));

    public static readonly DependencyProperty GaugeWidthProperty =
        DependencyProperty.Register(nameof(GaugeWidth), typeof(double), typeof(DbGauge), new PropertyMetadata(0.0));

    public string ValueText
    {
        get => (string)GetValue(ValueTextProperty);
        set => SetValue(ValueTextProperty, value);
    }

    public string CategoryText
    {
        get => (string)GetValue(CategoryTextProperty);
        set => SetValue(CategoryTextProperty, value);
    }

    public Brush CategoryBrush
    {
        get => (Brush)GetValue(CategoryBrushProperty);
        set => SetValue(CategoryBrushProperty, value);
    }

    public double GaugeWidth
    {
        get => (double)GetValue(GaugeWidthProperty);
        set => SetValue(GaugeWidthProperty, value);
    }

    #endregion

    public DbGauge()
    {
        InitializeComponent();
    }

    private static void OnValueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is DbGauge gauge)
        {
            gauge.UpdateGauge();
        }
    }

    private static void OnCategoryChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is DbGauge gauge)
        {
            gauge.UpdateCategory();
        }
    }

    /// <summary>
    /// Met à jour la jauge visuelle (largeur, texte).
    /// </summary>
    private void UpdateGauge()
    {
        ValueText = $"{Value:F0}";

        // Calculer largeur de la barre (échelle 0-120 dB(A) → 0-350px)
        // Avec clamp pour éviter dépassement
        double normalizedValue = Math.Clamp(Value, 0, 120);
        GaugeWidth = (normalizedValue / 120.0) * 350;
    }

    /// <summary>
    /// Met à jour la couleur et le texte de la catégorie.
    /// </summary>
    private void UpdateCategory()
    {
        CategoryText = Category switch
        {
            ExposureCategory.Safe => "✓ Niveau sûr",
            ExposureCategory.Moderate => "⚠ Niveau modéré (limiter exposition)",
            ExposureCategory.Hazardous => "⚠ Niveau dangereux (risque auditif)",
            ExposureCategory.Critical => "⛔ NIVEAU CRITIQUE (danger immédiat)",
            _ => "Inconnu"
        };

        CategoryBrush = Category switch
        {
            ExposureCategory.Safe => new SolidColorBrush(Color.FromRgb(76, 175, 80)),      // Vert
            ExposureCategory.Moderate => new SolidColorBrush(Color.FromRgb(255, 152, 0)),  // Orange
            ExposureCategory.Hazardous => new SolidColorBrush(Color.FromRgb(244, 67, 54)), // Rouge
            ExposureCategory.Critical => new SolidColorBrush(Color.FromRgb(183, 28, 28)),  // Rouge très foncé
            _ => Brushes.Gray
        };
    }
}
