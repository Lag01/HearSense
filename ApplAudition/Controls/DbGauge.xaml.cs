using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using ApplAudition.Models;

namespace ApplAudition.Controls;

/// <summary>
/// UserControl représentant une jauge visuelle dB(A) avec code couleur (Phase 6 - Tâche 14).
/// </summary>
public partial class DbGauge : System.Windows.Controls.UserControl
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
        DependencyProperty.Register(nameof(CategoryBrush), typeof(System.Windows.Media.Brush), typeof(DbGauge), new PropertyMetadata(System.Windows.Media.Brushes.Gray));

    public static readonly DependencyProperty GaugeWidthProperty =
        DependencyProperty.Register(nameof(GaugeWidth), typeof(double), typeof(DbGauge), new PropertyMetadata(0.0));

    // Positions des marqueurs de seuils (calculées dynamiquement au runtime)
    public static readonly DependencyProperty Marker1PositionProperty =
        DependencyProperty.Register(nameof(Marker1Position), typeof(double), typeof(DbGauge), new PropertyMetadata(0.0)); // 70 dB

    public static readonly DependencyProperty Marker2PositionProperty =
        DependencyProperty.Register(nameof(Marker2Position), typeof(double), typeof(DbGauge), new PropertyMetadata(0.0)); // 85 dB

    public static readonly DependencyProperty Marker3PositionProperty =
        DependencyProperty.Register(nameof(Marker3Position), typeof(double), typeof(DbGauge), new PropertyMetadata(0.0)); // 100 dB

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

    public System.Windows.Media.Brush CategoryBrush
    {
        get => (System.Windows.Media.Brush)GetValue(CategoryBrushProperty);
        set => SetValue(CategoryBrushProperty, value);
    }

    public double GaugeWidth
    {
        get => (double)GetValue(GaugeWidthProperty);
        set => SetValue(GaugeWidthProperty, value);
    }

    public double Marker1Position
    {
        get => (double)GetValue(Marker1PositionProperty);
        set => SetValue(Marker1PositionProperty, value);
    }

    public double Marker2Position
    {
        get => (double)GetValue(Marker2PositionProperty);
        set => SetValue(Marker2PositionProperty, value);
    }

    public double Marker3Position
    {
        get => (double)GetValue(Marker3PositionProperty);
        set => SetValue(Marker3PositionProperty, value);
    }

    #endregion

    // Constante pour l'échelle max de la jauge (0-120 dB)
    private const double MAX_DB_VALUE = 120.0;

    public DbGauge()
    {
        InitializeComponent();

        // Initialiser la couleur et le texte de la catégorie par défaut (vert pour Safe)
        UpdateCategory();

        // Écouter le chargement pour initialiser les positions des marqueurs
        Loaded += OnLoaded;
    }

    /// <summary>
    /// Handler du chargement : initialise les positions dynamiques des marqueurs.
    /// </summary>
    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        // Hook l'événement SizeChanged pour recalculer quand la taille change
        if (GaugeBarContainer != null)
        {
            GaugeBarContainer.SizeChanged += OnGaugeBarContainerSizeChanged;
            UpdateMarkerPositions(); // Calcul initial
        }
    }

    /// <summary>
    /// Handler du changement de taille : recalcule les positions des marqueurs et de la barre.
    /// </summary>
    private void OnGaugeBarContainerSizeChanged(object sender, SizeChangedEventArgs e)
    {
        UpdateMarkerPositions();
        UpdateGauge(); // Recalculer aussi la largeur de la barre
    }

    /// <summary>
    /// Met à jour les positions des marqueurs basées sur la largeur réelle du conteneur.
    /// </summary>
    private void UpdateMarkerPositions()
    {
        if (GaugeBarContainer == null) return;

        double availableWidth = GaugeBarContainer.ActualWidth;
        if (availableWidth <= 0) return;

        // Calculer positions en pourcentage de la largeur disponible
        Marker1Position = (70.0 / MAX_DB_VALUE) * availableWidth;   // 70 dB : 58.3%
        Marker2Position = (85.0 / MAX_DB_VALUE) * availableWidth;   // 85 dB : 70.8%
        Marker3Position = (100.0 / MAX_DB_VALUE) * availableWidth;  // 100 dB : 83.3%
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
    /// Met à jour la jauge visuelle (largeur, texte) avec animation fluide.
    /// Calcule la largeur basée sur la largeur réelle du conteneur (responsive).
    /// </summary>
    private void UpdateGauge()
    {
        ValueText = $"{Value:F0}";

        // Calculer largeur de la barre basée sur la largeur RÉELLE disponible (responsive)
        double availableWidth = GaugeBarContainer?.ActualWidth ?? 350.0; // Fallback 350px si pas encore chargé
        double normalizedValue = Math.Clamp(Value, 0, MAX_DB_VALUE);
        double targetWidth = (normalizedValue / MAX_DB_VALUE) * availableWidth;

        // Animation fluide pour la largeur de la barre
        var animation = new DoubleAnimation
        {
            To = targetWidth,
            Duration = TimeSpan.FromMilliseconds(250), // 250ms pour une transition fluide
            EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseOut }
        };

        BeginAnimation(GaugeWidthProperty, animation);
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
            ExposureCategory.Safe => new SolidColorBrush(System.Windows.Media.Color.FromRgb(76, 175, 80)),      // Vert
            ExposureCategory.Moderate => new SolidColorBrush(System.Windows.Media.Color.FromRgb(255, 152, 0)),  // Orange
            ExposureCategory.Hazardous => new SolidColorBrush(System.Windows.Media.Color.FromRgb(244, 67, 54)), // Rouge
            ExposureCategory.Critical => new SolidColorBrush(System.Windows.Media.Color.FromRgb(183, 28, 28)),  // Rouge très foncé
            _ => System.Windows.Media.Brushes.Gray
        };
    }
}
