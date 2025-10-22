# README_DEV.md - Documentation Développeur

## 📊 Audit du Repository - Appli Audition

### Vue d'ensemble technique

**Projet** : Application Windows d'estimation du niveau sonore au casque
**Langage** : C# 12
**Framework** : .NET 8 + WPF (Windows Presentation Foundation)
**Pattern architectural** : MVVM strict avec Dependency Injection

---

## 🏗️ Architecture de l'application

### Stack technique

| Composant | Version | Usage |
|-----------|---------|-------|
| .NET | 8.0 | Runtime Windows |
| WPF | Built-in | Interface utilisateur |
| CommunityToolkit.Mvvm | 8.2.2 | MVVM helpers (ObservableObject, RelayCommand) |
| NAudio | 2.2.1 | Capture audio WASAPI loopback |
| LiveChartsCore.SkiaSharpView.WPF | 2.0.0-rc2 | Graphiques temps réel |
| Serilog + Serilog.Sinks.File | 3.1.1 / 5.0.0 | Logging structuré |
| Microsoft.Extensions.DependencyInjection | 8.0.0 | Injection de dépendances |

### Structure des répertoires

```
ApplAudition/
├── App.xaml / App.xaml.cs          # Point d'entrée, configuration DI
├── Models/                          # Modèles de données
│   ├── AppSettings.cs              # Paramètres persistants
│   ├── ExposureCategory.cs         # Enum (Safe/Moderate/Hazardous)
│   ├── EstimationMode.cs           # Enum (ModeA uniquement)
│   ├── Profile.cs                  # Profil audio (obsolète)
│   ├── ProfileDatabase.cs          # Base de profils
│   ├── DataPoint.cs                # Point de données graphe
│   └── ExportDataPoint.cs          # Point export CSV
├── Services/                        # Services métier (Singletons)
│   ├── AudioCaptureService.cs      # Capture WASAPI loopback
│   ├── AudioDeviceService.cs       # Détection périphérique
│   ├── DspEngine.cs                # Pipeline DSP (RMS, dBFS, Hann)
│   ├── AWeightingFilter.cs         # Filtre pondération A (IEC 61672)
│   ├── LeqCalculator.cs            # Calcul Leq_1min + historique
│   ├── EstimationModeManager.cs    # Estimation SPL (Mode A)
│   ├── ExposureCategorizer.cs      # Catégorisation (Vert/Orange/Rouge)
│   ├── SystemVolumeService.cs      # Volume système Windows
│   ├── SettingsService.cs          # Persistance JSON
│   └── ExportService.cs            # Export CSV
├── ViewModels/                      # MVVM ViewModels
│   ├── BaseViewModel.cs            # Classe de base MVVM
│   ├── MainViewModel.cs            # ViewModel principal
│   └── CalibrationViewModel.cs     # ViewModel calibration
├── Views/                           # Vues WPF
│   └── MainWindow.xaml[.cs]        # Fenêtre principale
├── Converters/                      # Convertisseurs XAML
├── Controls/                        # UserControls WPF custom
└── Resources/                       # Ressources (thèmes, JSON)
    ├── Themes/Light.xaml
    └── profiles.json
```

---

## 🎯 Points d'entrée et flux de l'application

### 1. Point d'entrée principal

**Fichier** : `App.xaml.cs`
**Méthode** : `OnStartup(object sender, StartupEventArgs e)` (ligne 72)

**Flux de démarrage** :
```
OnStartup()
  ↓
ConfigureServices() // Configuration DI (ligne 21)
  ↓
InitializeServicesAsync() // Initialisation services (ligne 91)
  ↓
mainWindow.Show() // Affichage fenêtre principale (ligne 85)
```

### 2. Pipeline de calcul dB

**Service principal** : `DspEngine.cs`

**Flux de traitement audio** :
```
AudioCaptureService (WASAPI Loopback 48kHz)
  ↓ float[] buffer (125ms = 6000 samples)
AWeightingFilter (Filtre biquad IIR)
  ↓ buffer pondéré A
DspEngine.ProcessBuffer()
  ├─ Fenêtrage Hann : w[n] = 0.5*(1 - cos(2πn/(N-1)))
  ├─ Calcul RMS : sqrt(Σ(x²)/N)
  └─ Conversion dBFS : 20*log10(RMS)
  ↓
LeqCalculator (buffer circulaire 1 min)
  ├─ Leq_1min : 10*log10(mean(10^(Li/10)))
  └─ Peak : max(buffer)
  ↓
EstimationModeManager.EstimateSpl()
  ├─ SPL_est = dBFS + volumeSystemDb + offsetDynamique(dBFS)
  └─ Offset varie : 80 dB (silence) → 120 dB (fort)
  ↓
ExposureCategorizer
  ├─ < 70 dB(A) → Safe (Vert)
  ├─ 70-80 dB(A) → Moderate (Orange)
  └─ > 80 dB(A) → Hazardous (Rouge)
  ↓
MainViewModel (propriétés observables)
  └─ UI WPF (jauge + graphe LiveCharts2)
```

**Fichiers impliqués** :
- `AudioCaptureService.cs` : Capture via NAudio.WasapiLoopbackCapture
- `DspEngine.cs` : RMS/dBFS (méthodes `CalculateRms`, `RmsToDbfs`)
- `AWeightingFilter.cs` : Filtre biquad cascade (norme IEC 61672:2003)
- `LeqCalculator.cs` : Moyenne logarithmique glissante
- `EstimationModeManager.cs` : Estimation SPL avec offset adaptatif
- `ExposureCategorizer.cs` : Seuils conservateurs (biais -5dB)

---

## 🔧 Intégration Mode Tray + Auto-démarrage

### Stratégie retenue : Minimal-invasive

**Objectif** : Permettre à l'application de démarrer minimisée dans la zone de notification (system tray) et de s'exécuter en arrière-plan sans fenêtre principale visible.

### Composants à créer

#### A) TrayController (Contrôleur System Tray)

**Fichiers** :
- `Services/ITrayController.cs` (interface)
- `Services/TrayController.cs` (implémentation)

**Responsabilités** :
- Gérer `System.Windows.Forms.NotifyIcon`
- Menu contextuel :
  - "Afficher" → restaurer fenêtre principale
  - "Quitter" → fermer application
- Tooltip dynamique avec niveau dB(A) actuel
- Double-clic → restaurer fenêtre
- Icône avec états visuels (vert/orange/rouge selon exposition)

**API clés** :
```csharp
// System.Windows.Forms.NotifyIcon
notifyIcon.Icon = new System.Drawing.Icon(stream);
notifyIcon.Text = $"Appli Audition - {currentDbA:F0} dB(A)";
notifyIcon.ContextMenuStrip = contextMenu;
notifyIcon.Visible = true;
notifyIcon.DoubleClick += OnTrayIconDoubleClick;
```

**Dépendances NuGet** :
- Ajouter référence à `System.Windows.Forms` (déjà inclus dans .NET 8 Windows)

**Points d'ancrage** :
- `App.xaml.cs:ConfigureServices()` → Enregistrer `TrayController` en singleton
- `MainWindow.xaml.cs` → Initialiser tray dans constructeur
- `MainWindow.xaml.cs:OnClosing()` → Intercepter fermeture, minimiser vers tray

**Impact** : ⚠️ **MOYEN** (ajout référence WindowsForms, hook lifecycle fenêtre)

---

#### B) StartupManager (Gestionnaire auto-démarrage)

**Fichiers** :
- `Services/IStartupManager.cs` (interface)
- `Services/StartupManager.cs` (implémentation)

**Responsabilités** :
- Activer/désactiver démarrage automatique Windows
- Vérifier état actuel (activé/désactivé)
- Support argument `--minimized` pour démarrage silencieux

**Méthodes Windows supportées** :

##### Option 1 : Clé de registre HKCU (Recommandée) ✅

**Clé** : `HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Run`
**Valeur** : `"ApplAudition" = "C:\path\to\ApplAudition.exe --minimized"`

**Avantages** :
- Pas de droits admin requis
- Standard Windows
- Désactivable via Gestionnaire de tâches (onglet "Démarrage")

**Code C#** :
```csharp
using Microsoft.Win32;

var key = Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Run", true);
key.SetValue("ApplAudition", $"\"{exePath}\" --minimized");
```

##### Option 2 : Dossier Startup utilisateur

**Chemin** : `%APPDATA%\Microsoft\Windows\Start Menu\Programs\Startup\ApplAudition.lnk`

**Avantages** :
- Plus visible pour utilisateur (peut supprimer manuellement)
- Compatible toutes versions Windows

**Inconvénient** :
- Nécessite création de raccourci .lnk (via IWshRuntimeLibrary)

**Points d'ancrage** :
- `App.xaml.cs:ConfigureServices()` → Enregistrer `StartupManager` en singleton
- `MainViewModel.cs` → Ajouter commande "Démarrer avec Windows" (RelayCommand)
- `Views/MainWindow.xaml` → Ajouter checkbox dans zone paramètres (si UI dédiée)

**Impact** : 🟢 **FAIBLE** (service isolé, pas de couplage avec reste de l'app)

---

#### C) AppSettings étendu (Paramètres Tray)

**Fichier à modifier** : `Models/AppSettings.cs`

**Nouveaux champs** :
```csharp
public class AppSettings
{
    // Existant
    public bool ForceModeA { get; set; }
    public float? CalibrationConstantC { get; set; }

    // NOUVEAUX : Tray + Auto-démarrage
    public bool StartWithWindows { get; set; } = false;
    public bool StartMinimized { get; set; } = false;
    public bool MinimizeToTrayOnClose { get; set; } = true;
}
```

**Persistance** : Via `SettingsService.cs` (déjà existant)
**Emplacement** : `%LOCALAPPDATA%\ApplAudition\settings.json`

**Impact** : 🟢 **FAIBLE** (ajout de propriétés, pas de refactoring)

---

#### D) Argument ligne de commande `--minimized`

**Fichier à modifier** : `App.xaml.cs:OnStartup()`

**Logique** :
```csharp
private async void OnStartup(object sender, StartupEventArgs e)
{
    // ... (configuration DI existante)

    // Détecter argument --minimized
    bool startMinimized = e.Args.Contains("--minimized") ||
                          _settingsService.Settings.StartMinimized;

    // Initialiser services
    await InitializeServicesAsync();

    var mainWindow = _serviceProvider.GetRequiredService<MainWindow>();

    if (startMinimized)
    {
        // Ne PAS appeler mainWindow.Show()
        // Uniquement initialiser le tray
        var trayController = _serviceProvider.GetRequiredService<ITrayController>();
        trayController.Initialize(mainWindow);
        // Fenêtre créée mais invisible
    }
    else
    {
        mainWindow.Show();
    }
}
```

**Impact** : ⚠️ **MOYEN** (modifie logique démarrage principale)

---

#### E) MainWindow : Hook fermeture → Tray

**Fichier à modifier** : `Views/MainWindow.xaml.cs`

**Code** :
```csharp
public partial class MainWindow : Window
{
    private readonly ITrayController? _trayController;

    public MainWindow(MainViewModel viewModel, ITrayController trayController)
    {
        InitializeComponent();
        DataContext = viewModel;
        _trayController = trayController;

        // Hook événement fermeture
        Closing += OnWindowClosing;
    }

    private void OnWindowClosing(object? sender, CancelEventArgs e)
    {
        var settingsService = (DataContext as MainViewModel)?.SettingsService;

        if (settingsService?.Settings.MinimizeToTrayOnClose == true)
        {
            // Annuler fermeture, minimiser vers tray
            e.Cancel = true;
            Hide();
            _trayController?.ShowBalloonTip("Appli Audition",
                "Application toujours active en arrière-plan");
        }
        // Sinon : laisser fermeture normale (e.Cancel = false par défaut)
    }
}
```

**Impact** : 🟢 **FAIBLE** (ajout d'un event handler)

---

### Ressources nécessaires

#### Icône Tray : `Resources/Icons/tray-icon.ico`

**Spécifications** :
- Format : `.ico` multi-tailles (16x16, 32x32, 48x48 pixels)
- Fond : Transparent
- Design : Simple (picto casque ou niveau sonore)
- États possibles :
  - Icône verte (< 70 dB)
  - Icône orange (70-80 dB)
  - Icône rouge (> 80 dB)

**Intégration .csproj** :
```xml
<ItemGroup>
  <EmbeddedResource Include="Resources\Icons\tray-icon.ico" />
</ItemGroup>
```

**Alternative temporaire** : Utiliser icône système par défaut (`SystemIcons.Application`)

---

## 📋 Liste complète des fichiers à créer/modifier

### Nouveaux fichiers (9)

| Fichier | Description | Impact | Lignes estim. |
|---------|-------------|--------|---------------|
| `Services/ITrayController.cs` | Interface contrôleur tray | 🟢 Faible | ~30 |
| `Services/TrayController.cs` | Implémentation NotifyIcon + menu | ⚠️ Moyen | ~250 |
| `Services/IStartupManager.cs` | Interface gestionnaire auto-démarrage | 🟢 Faible | ~25 |
| `Services/StartupManager.cs` | Gestion registre HKCU Run | 🟢 Faible | ~120 |
| `Resources/Icons/tray-icon.ico` | Icône system tray multi-tailles | 🟢 Faible | N/A (binaire) |
| `README_DEV.md` | Documentation développeur (ce fichier) | 🟢 Faible | ~600 |

### Fichiers à modifier (5)

| Fichier | Modification | Impact | Lignes modif. |
|---------|--------------|--------|---------------|
| `ApplAudition.csproj` | Ajout référence System.Windows.Forms + icône embedded | 🟢 Faible | +5 |
| `App.xaml.cs` | Support arg `--minimized`, enregistrement services tray/startup | ⚠️ Moyen | ~40 |
| `Views/MainWindow.xaml.cs` | Hook `Closing` → minimiser vers tray | 🟢 Faible | ~25 |
| `Models/AppSettings.cs` | Ajout champs `StartWithWindows`, `StartMinimized`, `MinimizeToTrayOnClose` | 🟢 Faible | +6 |
| `ViewModels/MainViewModel.cs` | Ajout commandes UI (StartWithWindowsCommand) | 🟢 Faible | ~30 |

**Total lignes ajoutées/modifiées** : ~500 lignes (estimation)

**Impact global** : ⚠️ **MOYEN**
- Pas de refactoring majeur
- Changements localisés
- Compatibilité ascendante préservée
- Aucune dépendance externe lourde

---

## 🧪 Tests requis

### Tests unitaires à créer

- `StartupManagerTests.cs` :
  - `AddToStartup_ShouldCreateRegistryKey`
  - `RemoveFromStartup_ShouldDeleteRegistryKey`
  - `IsInStartup_ShouldReturnCorrectState`

- `TrayControllerTests.cs` :
  - `Initialize_ShouldCreateNotifyIcon`
  - `UpdateTooltip_ShouldReflectCurrentDbA`
  - `ShowWindow_ShouldRestoreMainWindow`

### Tests manuels

- [ ] Démarrage normal → fenêtre visible
- [ ] Démarrage avec `--minimized` → fenêtre cachée, icône tray visible
- [ ] Clic "Fermer" → minimise vers tray (si paramètre activé)
- [ ] Double-clic icône tray → restaure fenêtre
- [ ] Menu tray "Quitter" → ferme application
- [ ] Activation "Démarrer avec Windows" → clé registre créée
- [ ] Redémarrage Windows → app démarre automatiquement minimisée
- [ ] Désactivation auto-démarrage → clé registre supprimée

---

## 🔐 Considérations de sécurité

### Registre Windows

- **Portée** : `HKEY_CURRENT_USER` uniquement (pas HKEY_LOCAL_MACHINE)
- **Droits** : Pas de droits admin requis
- **Réversibilité** : L'utilisateur peut désactiver via Gestionnaire de tâches

### Notifications système

- Utiliser `NotifyIcon.ShowBalloonTip()` avec modération (pas de spam)
- Respecter préférences Windows (si notifications désactivées système)

### Lifecycle application

- Garantir `Dispose()` correct de `NotifyIcon` dans `App.OnExit()`
- Éviter fuites mémoire (désabonner événements WinForms)

---

## 📚 Références techniques

### APIs Windows utilisées

- **System.Windows.Forms.NotifyIcon** : Icône zone de notification
  - [Documentation Microsoft](https://learn.microsoft.com/dotnet/api/system.windows.forms.notifyicon)

- **Microsoft.Win32.Registry** : Manipulation registre Windows
  - [Documentation Microsoft](https://learn.microsoft.com/dotnet/api/microsoft.win32.registry)

- **System.Diagnostics.Process** : Récupération chemin exécutable
  - `Process.GetCurrentProcess().MainModule.FileName`

### Conventions de nommage projet

- **Interfaces** : Préfixe `I` (ex: `ITrayController`)
- **Services** : Suffix `Service` ou rôle métier (ex: `TrayController`)
- **Commandes MVVM** : Suffix `Command` (ex: `ToggleStartupCommand`)
- **Propriétés observables** : PascalCase, attribut `[ObservableProperty]` (CommunityToolkit.Mvvm)

---

## 🚀 Ordre d'implémentation recommandé

### Phase 1 : Fondations (TrayController)
1. Créer interfaces `ITrayController.cs`
2. Implémenter `TrayController.cs` (version basique)
3. Ajouter référence System.Windows.Forms au .csproj
4. Créer icône temporaire (ou utiliser `SystemIcons.Application`)
5. Modifier `App.xaml.cs` pour enregistrer TrayController
6. Modifier `MainWindow.xaml.cs` hook Closing

**Critère de validation** : Clic "Fermer" minimise vers tray, double-clic restaure fenêtre

### Phase 2 : Auto-démarrage (StartupManager)
1. Créer interfaces `IStartupManager.cs`
2. Implémenter `StartupManager.cs` (registre HKCU)
3. Étendre `AppSettings.cs` avec champs startup
4. Ajouter commandes dans `MainViewModel.cs`
5. Ajouter checkbox UI (optionnel, peut être dans menu tray)

**Critère de validation** : Checkbox "Démarrer avec Windows" crée/supprime clé registre

### Phase 3 : Mode minimisé au démarrage
1. Modifier `App.xaml.cs:OnStartup()` pour détecter `--minimized`
2. Tester : `ApplAudition.exe --minimized` → app démarre cachée

**Critère de validation** : Redémarrage Windows → app se lance automatiquement en tray

### Phase 4 : Polish & Tests
1. Améliorer icône tray (multi-états selon dB)
2. Tooltip dynamique avec niveau actuel
3. Notifications balloon tip (modération)
4. Tests unitaires StartupManager
5. Tests manuels complets

---

## 📝 Notes de développement

### Limitations connues

- **WPF + WinForms mixing** : `NotifyIcon` nécessite référence System.Windows.Forms (pas d'équivalent natif WPF)
- **Icône système** : Limité à 16x16 pixels dans tray (haute résolution = downscaling auto)
- **Menu contextuel** : `ContextMenuStrip` (WinForms) incompatible avec `ContextMenu` (WPF)

### Alternatives envisagées et rejetées

- **Hardcodet.NotifyIcon.Wpf** : Lib tierce pour tray natif WPF
  - **Rejeté** : Dépendance externe non nécessaire, System.Windows.Forms suffit

- **Windows Service** : App en tant que service Windows
  - **Rejeté** : Trop complexe, nécessite admin, non adapté pour app utilisateur

- **UWP Background Task** : Tâche en arrière-plan UWP
  - **Rejeté** : Projet est WPF classique, pas UWP

### Compatibilité

- **OS minimum** : Windows 10 version 1809+ (déjà requis par .NET 8)
- **Architecture** : x64 uniquement (NAudio WASAPI)
- **Droits** : Utilisateur standard (pas d'admin requis)

---

## 🎓 Ressources pour contributeurs

### Documentation officielle

- [WPF sur .NET 8](https://learn.microsoft.com/dotnet/desktop/wpf/)
- [System.Windows.Forms.NotifyIcon](https://learn.microsoft.com/dotnet/api/system.windows.forms.notifyicon)
- [Registre Windows et .NET](https://learn.microsoft.com/dotnet/api/microsoft.win32.registry)
- [CommunityToolkit.Mvvm](https://learn.microsoft.com/dotnet/communitytoolkit/mvvm/)

### Tutoriels pertinents

- [Creating a System Tray App in WPF](https://www.codeproject.com/Articles/36468/WPF-NotifyIcon)
- [Auto-start Windows Application](https://stackoverflow.com/questions/12352862/how-do-i-add-my-application-to-the-windows-startup)

---

## ✅ Critères d'acceptation finaux

- [x] README_DEV.md créé avec inventaire complet
- [x] Architecture et points d'ancrage documentés
- [x] Pipeline DSP expliqué (flux de calcul dB)
- [x] Stratégie TrayController documentée (System.Windows.Forms.NotifyIcon)
- [x] Stratégie StartupManager documentée (registre HKCU Run)
- [x] Liste fichiers à créer/modifier validée (9 nouveaux, 5 modifiés)
- [x] Impact évalué (MOYEN global, changements localisés)
- [x] Ordre d'implémentation défini (4 phases)
- [x] Options auto-démarrage comparées (registre vs Startup folder)
- [x] Tests requis listés (unitaires + manuels)
- [x] Branche `feature/tray-startup` créée

---

**Auteur** : Claude (Sonnet 4.5)
**Date** : 2025-10-22
**Version** : 1.0
**Statut** : ✅ Complet - Prêt pour implémentation
