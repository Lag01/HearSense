# Appli Audition - Application d'estimation du niveau sonore au casque

> **Application Windows .NET 8 WPF** pour estimation du niveau dB(A) au casque **sans saisie utilisateur obligatoire**, avec profils heuristiques auto-sélectionnés et approche conservatrice.

---

## ⚠️ Changements majeurs (2025-10-09) - Approche simplifiée "Style Apple"

### 🔧 CORRECTION CRITIQUE : Intégration du volume système Windows

**Problème identifié** : L'application affichait des valeurs dB constantes car WASAPI Loopback capture le signal audio **AVANT** que le volume système Windows ne soit appliqué. Monter ou baisser le volume n'avait aucun effet visible.

**Solution implémentée** :
- Nouveau service `SystemVolumeService` qui récupère le niveau de volume Windows en temps réel via `NAudio.CoreAudioApi.AudioEndpointVolume`
- Formule corrigée : `SPL_est = dBFS + C + volume_système_dB`
- Le volume système (en dB) est maintenant intégré dans tous les calculs SPL
- Monitoring en temps réel des changements de volume (événements Windows)

### 🎨 Interface simplifiée - Approche minimaliste

**Philosophie** : Simplicité type Apple - l'application fonctionne immédiatement sans configuration

**Supprimé de l'interface** :
- ❌ Panneau "Calibration" (Expander complet)
- ❌ Panneau "Mode actif" et tous les badges techniques
- ❌ Panneau "Profil détecté" (information technique)
- ❌ Boutons "Démarrer" et "Arrêter" (démarrage automatique)
- ❌ Bouton "Forcer Mode A" (mode transparent)
- ❌ Informations techniques (dBFS, Leq, Pic) dans l'UI principale

**Conservé** :
- ✅ Jauge dB(A) avec code couleur (vert/orange/rouge)
- ✅ Graphe historique 3 minutes (LiveCharts2)
- ✅ Export CSV (bouton discret)
- ✅ Toggle thème Dark/Light (icône discrète)

### 🚀 Démarrage automatique

- L'application démarre l'analyse audio **automatiquement** dès l'ouverture
- Plus besoin de cliquer sur "Démarrer"
- L'utilisateur voit immédiatement le niveau sonore s'afficher
- Message de statut : "Analyse en cours..."

### 🔬 Mode d'estimation transparent

- Les modes A et B fonctionnent toujours en arrière-plan
- L'utilisateur ne voit qu'une seule valeur dB(A) estimée
- Les profils heuristiques sont appliqués automatiquement si détectés
- Constante C de base = 95 dB (estimation conservatrice pour casques grand public)
- L'utilisateur n'a pas besoin de comprendre la différence Mode A/Mode B

### 📐 Architecture technique mise à jour

#### Nouveaux services
- **SystemVolumeService** : Récupère le volume Windows en temps réel
  - `GetCurrentVolume()` : Volume scalar (0.0 à 1.0)
  - `GetVolumeDb()` : Volume en dB (-96 dB à 0 dB)
  - Événement `VolumeChanged` pour monitoring temps réel

#### Formule SPL mise à jour
```
Mode A : SPL_est = dBFS + volume_système_dB + 95
Mode B : SPL_est = dBFS + volume_système_dB + C_profil
```

Où :
- `dBFS` = niveau numérique du signal audio (après pondération A)
- `volume_système_dB` = niveau de volume Windows en dB (**NOUVEAU**)
- `95` = offset de base conservateur (Mode A)
- `C_profil` = constante du profil heuristique détecté (Mode B)

### 📝 Impact sur la documentation

**Sections obsolètes** (à ignorer ou mettre à jour) :
- "Modes d'estimation" → maintenant transparent pour l'utilisateur
- "Calibration optionnelle" → supprimée de l'UI
- Phase 7 "UI avancée" → simplifiée
- Phase 8 "Calibration" → non accessible dans l'UI

**Décision technique importante** :
> "Volume système inaccessible via WASAPI → calibration nécessaire" **est désormais FAUSSE**.
> Le volume système est maintenant accessible et intégré via `AudioEndpointVolume`.

---

## 📋 Table des matières

1. [Vue d'ensemble & Contexte](#vue-densemble--contexte)
2. [Glossaire technique](#glossaire-technique)
3. [Architecture système](#architecture-système)
4. [Stack technique](#stack-technique)
5. [Pipeline DSP](#pipeline-dsp)
6. [Conventions](#conventions)
7. [Plan d'implémentation (21 tâches)](#plan-dimplémentation)

---

## Vue d'ensemble & Contexte

### Problématique
L'exposition prolongée à des niveaux sonores élevés (> 85 dB(A)) peut causer des dommages auditifs irréversibles. Les utilisateurs de casques/écouteurs manquent souvent de retour sur le niveau sonore réel auquel ils sont exposés.

### Objectif du projet
Créer une application Windows qui **estime en temps réel** le niveau sonore dB(A) au casque à partir du signal audio système (WASAPI loopback), **sans exiger de saisie utilisateur par défaut**.

### Philosophie "Simple et Conservateur"
- **Priorité 1**: Fonctionner immédiatement sans configuration
- **Priorité 2**: Sur-estimer modérément pour la sécurité (biais conservateur)
- **Priorité 3**: Permettre la personnalisation des seuils de notification

### Cas d'usage
1. **Utilisateur lambda** : Lance l'app, obtient une indication visuelle (vert/orange/rouge) du niveau d'exposition en dB(A)
2. **Utilisateur avancé** : Peut personnaliser les seuils de notification selon ses préférences

### Ce que l'app N'EST PAS
- ❌ Un sonomètre médical certifié
- ❌ Une mesure de l'audition (audiogramme)
- ❌ Un remplacement des protections auditives professionnelles

---

## Glossaire technique

### Audio & DSP

| Terme | Définition |
|-------|-----------|
| **WASAPI Loopback** | API Windows (Windows Audio Session API) permettant de capturer le flux audio système avant qu'il n'atteigne les haut-parleurs/casque. Mode "loopback" = capture de la sortie, pas du micro. |
| **dBFS** | Decibels Full Scale. Échelle numérique où 0 dBFS = amplitude maximale possible avant écrêtage. Valeurs négatives (ex: -18 dBFS = 12.5% de l'amplitude max). |
| **SPL (Sound Pressure Level)** | Niveau de pression acoustique en dB (échelle physique, mesurée avec sonomètre). Référence : 20 µPa (seuil d'audition). |
| **dB(A)** | Décibels pondérés A. Filtre qui atténue les basses et hautes fréquences pour simuler la sensibilité de l'oreille humaine (1 kHz = référence, 100 Hz ≈ -20 dB). |
| **RMS (Root Mean Square)** | Valeur quadratique moyenne d'un signal. Représente l'énergie moyenne : RMS = sqrt(Σ(x²)/N). Utilisé pour mesurer le niveau sonore. |
| **Leq (Equivalent Continuous Level)** | Niveau équivalent continu. Moyenne logarithmique de l'énergie sur une période : Leq = 10·log10(mean(10^(Li/10))). Norme pour exposition sonore. |
| **Fenêtre de Hann** | Fonction de pondération pour réduire les artefacts spectraux : w[n] = 0.5·(1 - cos(2πn/(N-1))). Appliquée avant calcul RMS. |
| **Biquad** | Filtre IIR (Infinite Impulse Response) du 2ème ordre. Forme : y[n] = b0·x[n] + b1·x[n-1] + b2·x[n-2] - a1·y[n-1] - a2·y[n-2]. |

### Normes & Santé

| Norme | Seuil | Description |
|-------|-------|-------------|
| **OMS (WHO)** | 85 dB(A) | Exposition max 8h/jour sans risque selon l'Organisation Mondiale de la Santé |
| **NIOSH** | 85 dB(A) | Idem (National Institute for Occupational Safety and Health, USA) |
| **IEC 61672:2003** | - | Norme internationale définissant les filtres de pondération A, B, C pour sonomètres |

---

## Architecture système

### Vue d'ensemble

```
┌─────────────────────────────────────────────────────────────────┐
│                         WASAPI Loopback                         │
│                  (Capture audio système 48kHz)                  │
└────────────────────────────┬────────────────────────────────────┘
                             │ float[] buffer (125ms)
                             ▼
┌─────────────────────────────────────────────────────────────────┐
│                         DSP Pipeline                            │
│  1. Pondération A (Biquad IIR) → buffer pondéré                │
│  2. Fenêtrage Hann → buffer fenêtré                            │
│  3. Calcul RMS → float rms_weighted                            │
│  4. Conversion dBFS = 20·log10(rms)                            │
└────────────────────────────┬────────────────────────────────────┘
                             │ dBFS + Leq + Pic
                             ▼
┌─────────────────────────────────────────────────────────────────┐
│                    Estimation Mode Manager                      │
│                                                                 │
│  ┌─────────────────┐              ┌─────────────────┐         │
│  │    Mode A       │              │    Mode B       │         │
│  │  Zero-Input     │              │  Auto-profil    │         │
│  │  Conservateur   │              │  Heuristique    │         │
│  ├─────────────────┤              ├─────────────────┤         │
│  │ dB(A) relatif   │◄─profil?────►│ SPL_est = dBFS+C│         │
│  │ Catégories      │     NON      │ Avertissement   │         │
│  │ (Vert/Orange/   │              │ (marge ±6dB)    │         │
│  │  Rouge)         │     OUI      │                 │         │
│  └─────────────────┘              └─────────────────┘         │
└────────────────────────────┬────────────────────────────────────┘
                             │ Valeurs finales
                             ▼
┌─────────────────────────────────────────────────────────────────┐
│                         UI (WPF MVVM)                           │
│  - Jauge dB(A) avec code couleur (vert/orange/rouge)           │
│  - Graphe historique 3 min (LiveCharts2)                       │
│  - Badges "Mode actif", "Profil détecté", "Conservateur"       │
│  - Panneau Calibration (optionnel)                             │
│  - Export CSV                                                   │
└─────────────────────────────────────────────────────────────────┘
```

### Composants principaux

#### Services (Singletons, injectés via DI)
- **AudioCaptureService** : Gère WASAPI loopback, émet événements `DataAvailable`
- **AudioDeviceService** : Détecte périphérique de sortie actif (nom, type)
- **DspEngine** : Pipeline DSP (RMS, dBFS, pondération A)
- **AWeightingFilter** : Filtre biquad IEC 61672:2003
- **LeqCalculator** : Calcul Leq_1min, Pic (buffer circulaire)
- **ProfileMatcher** : Match nom périphérique → profil JSON
- **EstimationModeManager** : Logique Mode A ↔ Mode B, calcul SPL
- **SettingsService** : Persistance JSON (theme, C_calibrated, forceModeA)
- **LoggingService** : Serilog/NLog, fichiers rolling

#### ViewModels (MVVM)
- **MainViewModel** : Propriétés observables (CurrentDbA, Leq1Min, ExposureCategory, etc.)
- **CalibrationViewModel** : Logique UI calibration
- **SettingsViewModel** : Dark mode, export CSV, etc.

#### Models
- **Profile** : Représente un profil JSON (id, name, patterns, constant_c, margin_db)
- **ProfileDatabase** : Collection de profils chargés au démarrage
- **ExposureCategory** : Enum { Safe, Moderate, Hazardous }
- **EstimationMode** : Enum { ModeA, ModeB }

---

## Stack technique

### Framework & Runtime
- **.NET 8** (LTS, version minimale : 8.0.0)
- **WPF** (Windows Presentation Foundation)
- **C# 12** (nullable reference types activés)

### NuGet Packages

| Package | Version cible | Usage |
|---------|---------------|-------|
| `CommunityToolkit.Mvvm` | ≥ 8.2.2 | MVVM helpers (ObservableObject, RelayCommand) |
| `NAudio` | ≥ 2.2.1 | WASAPI loopback, traitement audio |
| `LiveCharts2.Wpf` | ≥ 2.0.0-rc2 | Graphe temps réel |
| `Serilog` + `Serilog.Sinks.File` | ≥ 3.1.1 | Logging structuré |
| `System.Text.Json` | Inclus .NET 8 | Sérialisation JSON (profiles, settings) |

### Configuration système requise

| Composant | Minimum | Recommandé |
|-----------|---------|------------|
| OS | Windows 10 (1809+) | Windows 11 |
| CPU | 2 cores, 2 GHz | 4 cores, 3 GHz |
| RAM | 4 GB | 8 GB |
| Audio | WASAPI compatible | - |
| .NET Runtime | .NET 8 Desktop Runtime | - |

---

## Pipeline DSP

**Étapes** (125 ms = 6000 samples @ 48kHz):
1. Stéréo → mono: `(L+R)/2`
2. Filtre pondération A (biquad IIR cascade)
3. Fenêtrage Hann: `w[n] = 0.5·(1 - cos(2πn/(N-1)))`
4. RMS: `sqrt(Σ(x²)/N)`
5. dBFS: `20·log10(RMS)` (clamp à -120 si silence)
6. Leq_1min: buffer circulaire 480 échantillons, `Leq = 10·log10(mean(10^(Li/10)))`
7. Pic: max(buffer circulaire)
8. Catégories: Safe < 70, Moderate 70-80, Hazardous > 80 dB(A)

**Formules clés**:
- **RMS**: `sqrt(Σ(x²)/N)`
- **dBFS**: `20·log10(RMS)`
- **Leq**: `10·log10(mean(10^(Li/10)))`
- **Fenêtre Hann**: `0.5·(1 - cos(2πn/(N-1)))`
- **Pondération A**: IEC 61672:2003 (1kHz = 0dB, 100Hz ≈ -20dB, 10kHz ≈ -4dB)

**Seuils d'exposition** (biais conservateur -5dB appliqué):

| Catégorie | Seuil UI | Couleur | Durée max |
|-----------|----------|---------|-----------|
| Safe | < 70 dB(A) | 🟢 Vert | Illimitée |
| Moderate | 70-80 dB(A) | 🟠 Orange | 2-8h |
| Hazardous | > 80 dB(A) | 🔴 Rouge | < 2h |

---

## Conventions

**Nommage**: Classes/Méthodes = PascalCase, Champs privés = _camelCase, Constantes = UPPER_SNAKE_CASE

**Architecture**:
- MVVM strict (DI via Microsoft.Extensions.DependencyInjection)
- Singletons: AudioCaptureService, DspEngine, SettingsService, etc.
- Transients: ViewModels
- Async/Await pour tous les I/O
- IDisposable pour ressources non managées

**Tests**: Couverture 80% (Services/DSP), 50% (ViewModels), tolérance ±0.5 dB

**Limitations**:
- ✅ Mesure: Signal numérique envoyé (dBFS → SPL estimé)
- ❌ Ne mesure PAS: Pression acoustique réelle au conduit auditif
- Variables non contrôlées: Fit casque (±10dB), volume système (±20dB), EQ externes (±6dB)
- Biais conservateur: -5dB sur seuils (sur-estimer pour sécurité)

**Avertissements UI obligatoires**:
- Mode A: "Estimation conservatrice, valeur relative"
- Mode B: "Estimation heuristique, marge ±6dB"
- Calibré: "Valide uniquement pour ce périphérique + volume"

---

# Plan d'implémentation

## Phase 1: Infrastructure de base

### Tâche 1: Setup projet .NET 8 WPF + MVVM
**Objectif**: Créer la structure de base du projet

**À faire**:
- Créer solution .NET 8 WPF
- Structure MVVM (dossiers: Models, Views, ViewModels, Services)
- NuGet: CommunityToolkit.Mvvm, NAudio, LiveCharts2
- Configuration App.xaml + MainWindow.xaml
- Créer BaseViewModel avec INotifyPropertyChanged

**Dépendances**: Aucune

**Critères de validation**:
- Solution compile sans erreur
- MainWindow s'affiche
- MVVM configuré et fonctionnel

---

### Tâche 2: Intégration NAudio pour WASAPI loopback
**Objectif**: Capturer le flux audio système en temps réel

**À faire**:
- Créer `AudioCaptureService` (singleton)
- Configurer WASAPI loopback (WasapiLoopbackCapture)
- Format: 48 kHz, 32-bit float
- Exposer événement `DataAvailable` avec buffer float[]
- Gérer lifecycle (Start/Stop/Dispose)
- Thread-safety pour éviter freezes UI

**Points techniques**:
```csharp
// WasapiLoopbackCapture
// Format: WaveFormat(48000, 32, 2) // 48kHz, 32-bit, stéréo
// Convertir en mono si besoin (moyenne L+R)
```

**Dépendances**: Tâche 1

**Critères de validation**:
- Capture audio système sans freeze
- Buffer float[] accessible
- CPU stable < 5%

---

## Phase 2: DSP Core

### Tâche 3: Implémentation calcul RMS et dBFS
**Objectif**: Calculer le niveau RMS et convertir en dBFS

**À faire**:
- Créer `DspEngine` service
- Méthode `CalculateRMS(float[] buffer)` → float
- Formule: RMS = sqrt(sum(samples²) / N)
- Méthode `RmsToDbfs(float rms)` → float
- Formule: dBFS = 20 * log10(RMS)
- Fenêtrage Hann (window size = 125ms à 48kHz = 6000 samples)
- Gérer cas RMS = 0 → -∞ dB (clamp à -120 dBFS)

**Points techniques**:
```csharp
// Fenêtre Hann: w[n] = 0.5 * (1 - cos(2π*n/(N-1)))
// RMS fenêtré: sqrt(sum((samples[n] * w[n])²) / N)
```

**Dépendances**: Tâche 2

**Critères de validation**:
- RMS correct (test avec signal sinusoïdal connu)
- dBFS cohérent
- Tests unitaires passent

---

### Tâche 4: Implémentation filtre pondération A (biquad)
**Objectif**: Appliquer pondération A au signal avant calcul RMS

**À faire**:
- Créer classe `AWeightingFilter`
- Implémenter filtre biquad standard pondération A
- Coefficients selon norme IEC 61672:2003
- Appliquer au buffer avant calcul RMS
- État du filtre (z⁻¹, z⁻²) persistant entre buffers

**Points techniques**:
```csharp
// Biquad A-weighting à 48kHz
// Cascade de filtres IIR (typiquement 2-3 stages)
// Coefficients: b0, b1, b2, a1, a2
// y[n] = b0*x[n] + b1*x[n-1] + b2*x[n-2] - a1*y[n-1] - a2*y[n-2]
```

**Dépendances**: Tâche 3

**Critères de validation**:
- Courbe de réponse en fréquence conforme (±2 dB)
- Tests avec signaux 1 kHz, 100 Hz, 10 kHz
- Différence A-weighted vs non-weighted cohérente

---

### Tâche 5: Calcul Leq_1min (moyenne énergétique glissante)
**Objectif**: Calculer niveau équivalent continu sur 1 minute

**À faire**:
- Créer `LeqCalculator` service
- Buffer circulaire 1 min (histoire des RMS)
- Calcul: Leq = 10 * log10(mean(10^(Li/10)))
- Où Li = chaque mesure RMS en dB(A)
- Mise à jour temps réel (toutes les 125 ms)
- Exposer valeurs: Leq_1min, Pic (max sur 1 min)

**Points techniques**:
```csharp
// Buffer circulaire de ~480 échantillons (60s / 0.125s)
// Moyenne énergétique logarithmique
```

**Dépendances**: Tâche 4

**Critères de validation**:
- Leq stable sur signal constant
- Tests unitaires avec signaux variables
- Pic correct

---

## Phase 3: Mode A - Zero-Input Conservateur

### Tâche 6: Système de catégorisation (Vert/Orange/Rouge)
**Objectif**: Classer le signal en catégories d'exposition

**À faire**:
- Créer enum `ExposureCategory { Safe, Moderate, Hazardous }`
- Méthode `CategorizeExposure(float dbA)` → ExposureCategory
- Seuils (relatifs, sans SPL absolu):
  - Vert: < 75 dB(A) relatif (écoute modérée)
  - Orange: 75-85 dB(A) relatif (prolongée à limiter)
  - Rouge: > 85 dB(A) relatif (potentiellement nocive)
- Affichage: texte + couleur

**Dépendances**: Tâche 5

**Critères de validation**:
- Catégories affichées correctement
- Changements fluides

---

### Tâche 7: Biais de sécurité (+3 à +6 dB)
**Objectif**: Sur-estimer modérément pour la sécurité

**À faire**:
- Constante `SAFETY_BIAS = 5` (dB)
- Appliquer aux seuils de catégorisation (décalage vers le bas)
  - Vert: < 70 dB(A) (au lieu de 75)
  - Orange: 70-80 dB(A)
  - Rouge: > 80 dB(A)
- Badge "Conservateur" dans UI
- Documentation claire du biais

**Dépendances**: Tâche 6

**Critères de validation**:
- Sur-estimation visible
- Badge affiché
- Documentation explicite

---

## Phase 4: UI de base

### Tâche 13: Vue principale + MVVM binding
**Objectif**: Structure UI principale

**À faire**:
- Créer `MainViewModel`
- Properties observables:
  - CurrentDbA (float)
  - CurrentDbfs (float)
  - Leq1Min (float)
  - Peak (float)
  - ExposureCategory (enum)
  - CurrentMode (string)
  - DetectedProfile (string?)
- Bindings WPF vers View
- Timer UI refresh (60 Hz ou 30 Hz)

**Dépendances**: Tâche 12

**Critères de validation**:
- Bindings fonctionnels
- UI réactive
- Pas de freeze

---

### Tâche 14: Jauge dB(A) avec code couleur
**Objectif**: Affichage visuel principal du niveau

**À faire**:
- Control WPF custom ou ProgressBar stylisée
- Affichage: valeur dB(A) (relative ou absolue)
- Code couleur dynamique:
  - Vert: < 70 dB(A)
  - Orange: 70-80 dB(A)
  - Rouge: > 80 dB(A)
- Animation fluide (interpolation)
- Texte: "dB(A) relatif" ou "dB(A) estimé" selon mode

**Dépendances**: Tâche 13

**Critères de validation**:
- Jauge visuellement claire
- Couleurs changent correctement
- Responsive

---

### Tâche 15: Graphe historique (LiveCharts2)
**Objectif**: Historique 2-3 minutes du niveau

**À faire**:
- Intégrer LiveCharts2.Wpf
- Série temporelle (ObservableCollection)
- Axe X: temps (2-3 min glissants)
- Axe Y: dB(A) (0-120 ou auto-scale)
- Buffer circulaire (~1440 points pour 3 min à 125 ms)
- Couleur ligne: dégradé ou unique
- Tooltip avec valeur exacte

**Points techniques**:
```csharp
// LiveCharts2: CartesianChart
// Series: LineSeries<DataPoint>
// Update toutes les 125 ms, throttle si nécessaire
```

**Dépendances**: Tâche 13

**Critères de validation**:
- Graphe fluide
- Historique correct
- Performance stable

---

## Phase 5: UI avancée

### Tâche 16: Panneau "Mode actif" et badges
**Objectif**: Indiquer mode d'estimation actif

**À faire**:
- Section UI "Mode actif"
- Badge: "Mode A: Zero-Input Conservateur" ou "Mode B: Auto-profil Heuristique"
- Badge "Conservateur" (toujours visible)
- Icônes/couleurs pour différencier
- Possibilité de forcer Mode A (bouton "Ignorer profil")

**Dépendances**: Tâche 13

**Critères de validation**:
- Badges clairs
- Switch manuel fonctionne
- UI cohérente

---

### Tâche 17: Panneau "Profil détecté"
**Objectif**: Afficher info profil (si Mode B)

**À faire**:
- Section "Profil détecté" (visible seulement en Mode B)
- Afficher: nom profil, type (over-ear/on-ear/IEM)
- Constante C utilisée (±X dB)
- Marge d'erreur (±Y dB)
- Avertissement: "Estimation du signal envoyé, pas de votre audition"
- Possibilité d'ouvrir détails (modal?)

**Dépendances**: Tâche 16

**Critères de validation**:
- Info profil exacte
- Avertissement visible
- Conditionnel (seulement Mode B)

---

### Tâche 18: Dark mode + persistance settings
**Objectif**: Thème sombre et sauvegarde préférences

**À faire**:
- Implémenter dark mode (ResourceDictionary ou lib)
- Toggle UI dark/light
- Persistance via Settings.json (ApplicationData local):
  - Theme (dark/light)
  - ForceModeA (bool)
  - CalibrationConstantC (float?)
- Charger au démarrage, sauver à chaque changement
- Créer `SettingsService`

**Dépendances**: Tâche 17

**Critères de validation**:
- Dark mode fonctionnel
- Settings persistent entre sessions
- Pas de perte de données

---

## Phase 6: Export & Logging

### Tâche 21: Export CSV (timestamp, dBFS, dB(A), Leq, mode)
**Objectif**: Exporter historique vers CSV

**À faire**:
- Bouton "Export CSV"
- SaveFileDialog (choisir chemin)
- Format CSV:
```csv
Timestamp,dBFS,dB(A),Leq_1min,Peak,Mode,Profile
2025-10-07 14:30:00,-18.5,72.3,68.1,75.2,ModeB,over-ear-anc
```
- Encoder UTF-8 BOM
- Notifier succès/échec

**Dépendances**: Tâche 15

**Critères de validation**:
- Export fonctionnel
- CSV bien formé
- Données correctes

---

### Tâche 22: Système de logging
**Objectif**: Logs pour debugging

**À faire**:
- Intégrer Serilog ou NLog
- Logs fichier (rolling, max 10 MB)
- Niveaux: Debug, Info, Warning, Error
- Logger:
  - Détection périphérique
  - Selection profil
  - Erreurs capture audio
  - Calibration
- Emplacement: %LOCALAPPDATA%\ApplAudition\logs

**Dépendances**: Tâche 1

**Critères de validation**:
- Logs écrits correctement
- Rotation fonctionne
- Pas de spam

---

## Phase 7: Tests & Qualité

### Tâche 23: Tests unitaires DSP (RMS, dBFS, biquad A, Leq)
**Objectif**: Valider calculs DSP

**À faire**:
- Créer projet de tests xUnit ou NUnit
- Tests RMS:
  - Signal sinusoïdal 1 kHz, amplitude connue → RMS attendu
  - Signal nul → RMS = 0
- Tests dBFS:
  - RMS = 1.0 → 0 dBFS
  - RMS = 0.5 → -6.02 dBFS
- Tests pondération A:
  - Signal 1 kHz (référence, ≈0 dB correction)
  - Signal 100 Hz (forte atténuation ≈-20 dB)
  - Signal 10 kHz (atténuation ≈-4 dB)
- Tests Leq:
  - Signal constant → Leq = niveau constant
  - Signal variable → moyenne logarithmique

**Dépendances**: Tâche 5

**Critères de validation**:
- Tous tests passent (±0.5 dB tolérance)
- Couverture ≥ 80%

---

### Tâche 24: Tests système de profils
**Objectif**: Valider matching et modes

**À faire**:
- Tests ProfileMatcher:
  - "Sony WH-1000XM4" → profil "over-ear-anc"
  - "AirPods Pro" → profil "iem"
  - "Unknown Device" → null
- Tests EstimationModeManager:
  - Profil détecté → Mode B
  - Aucun profil → Mode A
  - Switch manuel → Mode A forcé

**Dépendances**: Tâche 12

**Critères de validation**:
- Matching correct
- Modes changent correctement
- Edge cases gérés

---

### Tâche 25: Tests performance CPU
**Objectif**: Garantir CPU < 10%

**À faire**:
- Mesurer CPU usage en conditions réelles
- Task Manager ou PerfMon
- Scénarios:
  - Lecture audio continue 30 min
  - Graphe + jauge actifs
  - Export CSV simultané
- Optimisations si nécessaire:
  - Throttle UI refresh
  - Async/await pour DSP
  - Buffer pooling
- Target: < 10% CPU sur machine typique (4 cores)

**Dépendances**: Tâche 15

**Critères de validation**:
- CPU < 10% stable
- Pas de freeze UI
- Responsive

---

## Phase 8: Packaging & Documentation

### Tâche 26: Configuration MSIX
**Objectif**: Créer installer Windows

**À faire**:
- Ajouter projet MSIX Packaging à solution
- Configurer Package.appxmanifest:
  - Identity, Publisher
  - Capabilities: microphone (pas nécessaire pour loopback mais déclaratif)
  - Logo, splash screen
- Certificat auto-signé (dev) ou vrai certificat (prod)
- Build MSIX bundle
- Test installation sur machine vierge

**Points techniques**:
```xml
<!-- Package.appxmanifest -->
<Identity Name="ApplAudition" Publisher="CN=..." Version="1.0.0.0" />
<Capabilities>
  <Capability Name="internetClient" />
</Capabilities>
```

**Dépendances**: Tâche 25

**Critères de validation**:
- MSIX build sans erreur
- Installation fonctionne
- App démarre correctement

---

### Tâche 27: Build portable .zip
**Objectif**: Version portable sans installation

**À faire**:
- Configuration build "Release" self-contained
- PublishSingleFile=false (avec dépendances séparées)
- ou PublishSingleFile=true (gros exe unique)
- Inclure profiles.json
- Créer .zip avec:
  - ApplAudition.exe
  - Dépendances (.dll)
  - README.txt (instructions)
- Test extraction + run sur machine clean

**Points techniques**:
```xml
<PropertyGroup>
  <SelfContained>true</SelfContained>
  <RuntimeIdentifier>win-x64</RuntimeIdentifier>
  <PublishSingleFile>true</PublishSingleFile>
</PropertyGroup>
```

**Dépendances**: Tâche 26

**Critères de validation**:
- Zip fonctionnel
- Aucune dépendance externe requise
- Portable

---

### Tâche 28: README complet (principes, limites, etc.)
**Objectif**: Documentation utilisateur finale

**À faire**:
- Créer README.md racine (GitHub) et README.txt (app)
- Sections:
  1. **Présentation**: objectif, offline, pas de données envoyées
  2. **Concepts**:
     - dBFS (Full Scale digital)
     - Pondération A (fréquences audibles)
     - Leq (niveau équivalent continu)
  3. **Modes d'estimation**:
     - Mode A (zero-input conservateur)
     - Mode B (auto-profil heuristique)
  4. **Profils heuristiques**: comment ça marche, marges d'erreur
  5. **Calibration optionnelle**: procédure avec sonomètre
  6. **Limites**:
     - Estimation du signal, pas de l'audition réelle
     - Variabilité casques (fit, usure, fuites)
     - Pas un instrument étalonné
     - Biais conservateur (sur-estimation)
  7. **Installation**: MSIX vs portable
  8. **Utilisation**: captures d'écran, FAQ
  9. **Contributions**: open-source, issues GitHub
  10. **License**: MIT ou autre

**Dépendances**: Tâche 27

**Critères de validation**:
- README exhaustif
- Limites clairement expliquées
- FAQ utile

---

## Récapitulatif des dépendances

```
Phase 1 (Infrastructure) → Phase 2 (DSP) → Phase 3 (Mode A) → Phase 4 (Profils) → Phase 5 (Mode B)
                                                                ↓
Phase 6 (UI base) → Phase 7 (UI avancée) → Phase 8 (Calibration) → Phase 9 (Export)
                                                                           ↓
Phase 10 (Tests) → Phase 11 (Packaging & Doc)
```

---

## Notes importantes

1. **Tests**: Écrire tests au fur et à mesure, pas seulement en Phase 10
2. **Commits**: Commit après chaque tâche complétée
3. **Performance**: Surveiller CPU/RAM dès Phase 2
4. **UX**: Toujours afficher les limites/avertissements
5. **Sécurité**: Sur-estimer modérément, jamais sous-estimer
6. **Offline**: Aucune connexion réseau, tout embarqué

---

## Progression

- [x] Phase 1: Infrastructure (2/2) ✅ **COMPLÉTÉE** - 2025-10-07
  - [x] Tâche 1: Setup projet .NET 8 WPF + MVVM
  - [x] Tâche 2: Intégration NAudio WASAPI loopback
- [x] Phase 2: DSP Core (3/3) ✅ **COMPLÉTÉE** - 2025-10-08
  - [x] Tâche 3: Implémentation calcul RMS et dBFS
  - [x] Tâche 4: Implémentation filtre pondération A (biquad)
  - [x] Tâche 5: Calcul Leq_1min (moyenne énergétique glissante)
- [x] Phase 3: Catégorisation (2/2) ✅ **COMPLÉTÉE** - 2025-10-08
  - [x] Tâche 6: Système de catégorisation (Vert/Orange/Rouge)
  - [x] Tâche 7: Biais de sécurité (+3 à +6 dB)
- [x] Phase 4: UI base (3/3) ✅ **COMPLÉTÉE** - 2025-10-08
  - [x] Tâche 13: Vue principale + MVVM binding
  - [x] Tâche 14: Jauge dB(A) avec code couleur
  - [x] Tâche 15: Graphe historique (LiveCharts2)
- [x] Phase 5: UI avancée (3/3) ✅ **COMPLÉTÉE** - 2025-10-08
  - [x] Tâche 16: Interface utilisateur avancée
  - [x] Tâche 17: Seuils personnalisables
  - [x] Tâche 18: Dark mode + persistance settings
- [x] Phase 6: Export & Logging (2/2) ✅ **COMPLÉTÉE** - 2025-10-08
  - [x] Tâche 21: Export CSV (ExportService + commande UI)
  - [x] Tâche 22: Système de logging (Serilog configuré)
- [x] Phase 7: Tests & Qualité (3/3) ✅ **COMPLÉTÉE** - 2025-10-08
  - [x] Tâche 23: Tests unitaires DSP (DspEngine, AWeighting, Leq)
  - [x] Tâche 24: Tests système
  - [x] Tâche 25: Tests performance CPU (PerformanceTests)
- [x] Phase 8: Packaging & Documentation (3/3) ✅ **COMPLÉTÉE** - 2025-10-08
  - [x] Tâche 26: Configuration MSIX (templates et documentation)
  - [x] Tâche 27: Build portable .zip (script PowerShell + README)
  - [x] Tâche 28: README complet (documentation utilisateur exhaustive)

**Total**: 21/21 tâches complétées (100%) 🎉

---

## Notes

**Principes directeurs**:
1. Zero-Input First (fonctionner sans saisie utilisateur)
2. Conservateur (sur-estimer le risque, biais +5dB)
3. Transparence (afficher limites/marges)
4. Performance (CPU < 10%, pas de freeze)
5. Offline (aucune connexion réseau)

**Décisions techniques**:
- Buffer 125ms: compromis réactivité/stabilité (8 updates/sec)
- Pondération A: simule oreille humaine (vs C = linéaire)
- Leq_1min: plus réactif que Leq_8h pour écoute musicale
- Volume système intégré via AudioEndpointVolume (NAudio)

**Structure projet**: Models/ ViewModels/ Views/ Services/ Resources/ Converters/ Controls/ + Tests/

---

**Dernière mise à jour** : 2025-10-08
- Je ne veux pas de systeme de profil pour l'utilisateur ou qu'il est la possibilité de faire ces propres tests avec un sonomètre, je ne veux que notre estimation