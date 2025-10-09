# 🎧 Guide de démarrage - Appli Audition

> **Application Windows pour surveiller le niveau sonore de votre casque en temps réel**

---

## 🚀 Démarrage rapide (5 minutes)

**Vous êtes pressé ? Suivez ces étapes :**

1. **Vérifiez .NET Desktop Runtime** → Vous devez avoir .NET 8 ou supérieur ([Télécharger si besoin](https://dotnet.microsoft.com/download/dotnet/8.0))
2. **Lancez l'application** → Voir les [3 méthodes ci-dessous](#-comment-lancer-lapplication)
3. **Connectez votre casque** → Définissez-le comme périphérique de sortie par défaut Windows
4. **Démarrez la capture** → Cliquez sur "▶ Démarrer la capture"
5. **Jouez de la musique** → Observez la jauge :
   - 🟢 **Vert** = Niveau sûr
   - 🟠 **Orange** = Modéré (limiter la durée)
   - 🔴 **Rouge** = Attention danger !

**C'est tout !** Continuez la lecture pour les détails et fonctionnalités avancées.

---

## 📋 Table des matières

1. [Prérequis système](#-prérequis-système)
2. [Comment lancer l'application](#-comment-lancer-lapplication)
3. [Première utilisation](#-première-utilisation)
4. [Comprendre l'interface](#-comprendre-linterface)
5. [Modes d'estimation](#-modes-destimation)
6. [Fonctionnalités avancées](#-fonctionnalités-avancées)
7. [Dépannage](#-dépannage)

---

## 💻 Prérequis système

### Configuration minimale

| Composant | Requis |
|-----------|--------|
| **Système** | Windows 10 (1809+) ou Windows 11 |
| **Processeur** | 2 cores, 2 GHz minimum |
| **Mémoire** | 4 GB RAM |
| **Runtime** | .NET 8 Desktop Runtime |

### Installer .NET 8 Desktop Runtime

> ⚠️ **Important** : L'application ne fonctionnera pas sans ce composant !

**Étapes d'installation :**

1. Rendez-vous sur [dotnet.microsoft.com/download/dotnet/8.0](https://dotnet.microsoft.com/download/dotnet/8.0)
2. Sélectionnez **"Desktop Runtime"** (pas le SDK)
3. Choisissez **x64** (architecture standard)
4. Installez et **redémarrez votre PC**

**Vérifier l'installation :**
```powershell
dotnet --list-runtimes
```
Vous devriez voir : `Microsoft.WindowsDesktop.App 8.x.x`

---

## 📦 Comment lancer l'application

> **Important** : Vous n'avez PAS besoin de Visual Studio pour utiliser l'application ! Visual Studio n'est nécessaire que si vous voulez modifier le code source.

### 🎯 Méthode 1 : Lancer l'exe directement (le plus simple)

Si vous avez déjà compilé le projet au moins une fois, l'exécutable existe déjà :

**Chemin complet :**
```
C:\Users\lumin\Documents\Code\Appli Audition\ApplAudition\bin\Debug\net8.0-windows\ApplAudition.exe
```

**Comment y accéder :**
1. Ouvrez l'Explorateur Windows
2. Naviguez vers le dossier du projet
3. Allez dans `ApplAudition\bin\Debug\net8.0-windows\`
4. Double-cliquez sur **`ApplAudition.exe`**

✅ **Astuce** : Créez un raccourci sur le Bureau pour un accès rapide !

---

### 🎯 Méthode 2 : Ligne de commande (dotnet run)

Ouvrez PowerShell ou le Terminal Windows dans le dossier du projet :

```powershell
# Depuis le dossier racine
dotnet run --project ApplAudition\ApplAudition.csproj
```

Cette méthode compile automatiquement si nécessaire, puis lance l'app.

---

### 🎯 Méthode 3 : Version portable (recommandée pour partager)

**Qu'est-ce que la version portable ?**

Un dossier `.zip` contenant l'exe + toutes les dépendances, que vous pouvez :
- Copier sur une clé USB
- Partager avec quelqu'un d'autre
- Installer sur un autre PC sans recompiler

**Générer la version portable :**

```powershell
# Depuis le dossier racine du projet
.\Build-Portable.ps1
```

Ce script :
1. Compile le projet en mode Release
2. Copie tous les fichiers nécessaires
3. Crée un `.zip` dans `Build\Portable\`

**Utilisation :**
1. Extrayez le `.zip` où vous voulez (Bureau, Documents, clé USB...)
2. Double-cliquez sur **`ApplAudition.exe`**
3. ✅ **Aucune installation nécessaire sur le PC cible** (tant qu'il a .NET Desktop Runtime)

---

### 🛠️ Pour les développeurs : Visual Studio

Si vous voulez **modifier le code source**, vous pouvez utiliser Visual Studio :

1. Installez [Visual Studio 2022 Community](https://visualstudio.microsoft.com/fr/downloads/) (gratuit)
2. Durant l'installation, sélectionnez la charge de travail **"Développement .NET Desktop"**
3. Ouvrez `ApplAudition.sln`
4. Appuyez sur **F5** pour compiler et lancer en mode debug

> **Note** : Visual Studio Code (VS Code) ≠ Visual Studio. VS Code est un éditeur de texte, pas un IDE .NET complet.

---

## 🎯 Première utilisation

### Étape 1️⃣ : Configuration audio Windows

**Avant de lancer l'app, configurez votre casque :**

1. Ouvrez **Paramètres Windows** → **Son**
2. Dans "Périphérique de sortie", sélectionnez votre casque
3. Testez le son avec une vidéo YouTube

> 💡 **Astuce** : L'application capture automatiquement le périphérique par défaut. Si vous changez de casque, redémarrez l'app.

---

### Étape 2️⃣ : Lancer l'application

Au premier démarrage, l'application :

1. ✅ Détecte automatiquement votre casque
2. ✅ Sélectionne le mode d'estimation optimal :
   - **Mode B** (précis) si le casque est reconnu (ex: Sony WH-1000XM4)
   - **Mode A** (conservateur) sinon
3. ✅ Affiche l'interface principale

**État initial :**
- La capture audio est **arrêtée** par défaut
- Vous devez cliquer sur **"▶ Démarrer la capture"** pour commencer

---

### Étape 3️⃣ : Démarrer la mesure

1. Cliquez sur **"▶ Démarrer la capture"**
2. Lancez votre musique (Spotify, YouTube, etc.)
3. Réglez le volume à votre niveau habituel
4. Observez la jauge dB(A) bouger en temps réel

🎉 **Félicitations !** Vous surveillez maintenant votre exposition sonore.

---

## 📊 Comprendre l'interface

### Panneau principal

```
┌─────────────────────────────────────────┐
│  🎚️ 72.5 dB(A)    [████████░░] 🟢     │  ← Jauge temps réel
│                                         │
│  📈 Leq 1 min: 68.3 dB(A)              │  ← Niveau moyen
│  📍 Pic: 75.2 dB(A)                     │  ← Maximum
│  🏷️ Catégorie: Safe                    │  ← Évaluation
│                                         │
│  📊 [Graphe historique 3 minutes]      │  ← Historique
│                                         │
└─────────────────────────────────────────┘
```

### Indicateurs expliqués

| Indicateur | Signification | Mise à jour |
|-----------|---------------|-------------|
| **dB(A) actuel** | Niveau instantané pondéré A (simule l'oreille humaine) | Toutes les 125 ms |
| **Leq 1 min** | Niveau équivalent moyen sur la dernière minute | Temps réel |
| **Pic** | Niveau maximum atteint dans la minute | Temps réel |
| **Catégorie** | Safe / Moderate / Hazardous (selon normes OMS) | Temps réel |

### Code couleur des seuils

| Couleur | Plage | Interprétation | Durée max recommandée |
|---------|-------|----------------|----------------------|
| 🟢 **Vert** | < 70 dB(A) | Niveau sûr, écoute confortable | Illimitée |
| 🟠 **Orange** | 70-80 dB(A) | Niveau modéré, attention à la durée | 2-8 heures |
| 🔴 **Rouge** | > 80 dB(A) | ⚠️ Potentiellement dangereux | < 2 heures |

> ⚠️ **Important** : Ces seuils sont basés sur les recommandations OMS avec un biais de sécurité de +5 dB (l'app sur-estime légèrement pour protéger vos oreilles).

---

### Graphe historique

- **Axe horizontal** : Temps (3 minutes glissantes)
- **Axe vertical** : Niveau dB(A) (0-120 dB)
- **Ligne bleue** : Évolution du niveau sonore
- **Tooltip** : Survolez la courbe pour voir la valeur exacte à un instant T

---

## 🔧 Modes d'estimation

L'application propose **2 modes** pour estimer le niveau sonore :

### 🔵 Mode A : Zero-Input Conservateur

**Quand s'active-t-il ?**
- Casque non reconnu par l'application
- Utilisateur force manuellement le Mode A
- Périphérique de type "Haut-parleurs"

**Caractéristiques :**

| Aspect | Détail |
|--------|--------|
| **Type** | Estimation **relative** (pas de SPL absolu) |
| **Précision** | N/A (valeurs relatives uniquement) |
| **Biais** | +5 dB (conservateur, sur-estime le risque) |
| **Calibration** | Non disponible |
| **Usage** | Surveillance générale de l'exposition |

**Badge affiché :**
```
🔵 Mode A : Zero-Input Conservateur
🟠 ⚠ Conservateur (+5 dB)
```

> 💡 **À retenir** : En Mode A, fiez-vous aux **couleurs** (vert/orange/rouge), pas aux valeurs dB(A) absolues.

---

### 🟢 Mode B : Auto-profil Heuristique

**Quand s'active-t-il ?**
- Casque reconnu dans la base de profils (ex: Sony WH-1000XM4, AirPods Pro, Bose QC35)
- Périphérique Bluetooth générique (profil conservateur appliqué)

**Caractéristiques :**

| Aspect | Détail |
|--------|--------|
| **Type** | Estimation **absolue** (SPL en dB(A)) |
| **Précision** | ±5-8 dB selon le casque |
| **Profil** | Détecté automatiquement (sensibilité, impédance) |
| **Calibration** | Disponible pour précision optimale |
| **Usage** | Surveillance précise avec valeurs comparables à un sonomètre |

**Badge affiché :**
```
🟢 Mode B : Auto-profil Heuristique
📋 Profil : Over-ear ANC (fermés)
⚠️ Marge d'erreur : ±6 dB
```

**Panneau "Profil détecté" visible avec :**
- Nom du profil (ex: "Over-ear ANC (fermés)")
- Constante C (ex: -15.0 dB)
- Marge d'erreur estimée

> ⚠️ **Limite** : L'application estime le signal **envoyé** au casque, pas la pression acoustique réelle dans votre oreille. Les valeurs peuvent varier selon le fit du casque, l'isolation, et le volume système.

---

### Forcer le Mode A manuellement

Si vous êtes en Mode B et préférez une estimation conservative :

1. Cliquez sur **"Forcer Mode A"**
2. L'estimation devient relative
3. Cliquez sur **"Mode automatique"** pour revenir en Mode B

---

## 🎛️ Fonctionnalités avancées

### 🎯 Calibration avec sonomètre

**Objectif :** Ajuster la constante C pour obtenir une précision maximale avec **votre** setup (casque + volume Windows).

#### Prérequis

- ✅ Être en **Mode B** (profil détecté)
- ✅ Avoir un **sonomètre étalonné** (classe 2 minimum)
  - Sonomètre smartphone (ex: app "Decibel X" calibrée)
  - Appareil professionnel (Extech, PCE Instruments, etc.)
- ✅ Environnement calme

#### Procédure pas à pas

**1. Ouvrir le panneau Calibration**
   - Cliquez sur **"🎯 Calibration (optionnelle)"**

**2. Préparer la mesure**
   - Portez votre casque normalement
   - Réglez le volume Windows à votre niveau d'écoute habituel
   - Lancez une musique ou un **bruit rose** (signal de test stable)

**3. Mesurer avec le sonomètre**
   - Placez le micro du sonomètre **à l'intérieur du casque**, près de votre oreille
   - Attendez que la valeur se stabilise (~10 secondes)
   - Notez le **SPL mesuré** (ex: `78.5 dB(A)`)

**4. Saisir les valeurs**
   - Dans l'app, entrez le SPL mesuré : `78.5`
   - Le SPL estimé actuel s'affiche automatiquement (ex: `72.1`)

**5. Calibrer**
   - Cliquez sur **"Calibrer"**
   - L'app calcule la nouvelle constante C :
     ```
     C_new = C_old + (SPL_mesuré - SPL_estimé)
     C_new = -15.0 + (78.5 - 72.1) = -8.6 dB
     ```

**6. Vérifier**
   - Un badge **"✓ Calibré"** apparaît
   - Les valeurs affichées sont maintenant calibrées pour **votre configuration exacte**

> ⚠️ **ATTENTION** : La calibration n'est valide que pour :
> - Ce casque précis
> - Ce volume Windows précis
>
> Si vous changez le volume ou de casque, **recalibrez !**

#### Réinitialiser la calibration

Pour revenir au profil heuristique par défaut :
- Cliquez sur **"Réinitialiser"**

---

### 📊 Export CSV

**Exporter vos données pour analyse externe (Excel, Python, etc.)**

#### Procédure

1. Lancez une session de mesure (quelques minutes à plusieurs heures)
2. Cliquez sur **"📊 Export CSV"** (en bas de l'interface)
3. Choisissez l'emplacement et le nom du fichier (ex: `mesure_2025-10-09.csv`)
4. Ouvrez le fichier dans Excel, LibreOffice, ou Google Sheets

#### Format CSV

```csv
Timestamp,dBFS,dB(A),Leq_1min,Peak,Mode,Profile
2025-10-09 14:30:00,-18.5,72.3,68.1,75.2,ModeB,over-ear-anc
2025-10-09 14:30:01,-19.2,71.6,68.3,75.2,ModeB,over-ear-anc
2025-10-09 14:30:02,-17.8,73.0,68.5,75.2,ModeB,over-ear-anc
```

**Colonnes :**
- `Timestamp` : Date et heure de la mesure
- `dBFS` : Niveau numérique (Full Scale)
- `dB(A)` : Niveau pondéré A (estimation SPL)
- `Leq_1min` : Niveau équivalent 1 minute
- `Peak` : Pic sur 1 minute
- `Mode` : ModeA ou ModeB
- `Profile` : Profil détecté (ou "none")

---

### 🌙 Dark Mode

**Basculer entre thème clair et sombre :**

- Cliquez sur **"🌙 Dark"** (en haut à droite)
- Ou cliquez sur **"☀️ Light"** pour revenir en mode clair

Le thème est **sauvegardé automatiquement** entre les sessions.

---

### 🔍 Consulter les logs

Les logs sont enregistrés automatiquement dans :

```
%LOCALAPPDATA%\ApplAudition\logs\
```

**Chemin complet :**
```
C:\Users\<VotreNom>\AppData\Local\ApplAudition\logs\
```

**Format :**
- Fichier : `app-YYYY-MM-DD.log` (un par jour)
- Rétention : 10 jours (suppression automatique)
- Taille max : 10 MB par fichier

**Utilité :**
- Debugging en cas de problème
- Vérifier quel profil a été détecté
- Tracer les événements de calibration
- Diagnostiquer les erreurs de capture audio

---

## 🔧 Dépannage

### ❌ L'application ne démarre pas

**Symptôme :** Double-clic sur `.exe` → rien ne se passe

**Solutions :**

| Étape | Action |
|-------|--------|
| **1** | Vérifier que .NET 8 Desktop Runtime est installé : `dotnet --list-runtimes` |
| **2** | Réinstaller .NET 8 depuis [dotnet.microsoft.com](https://dotnet.microsoft.com/download/dotnet/8.0) |
| **3** | Consulter les logs : `%LOCALAPPDATA%\ApplAudition\logs\` |

---

### ❌ "Aucun périphérique audio détecté"

**Symptôme :** Message d'erreur au démarrage

**Solutions :**

1. **Vérifier le périphérique de sortie**
   - `Paramètres Windows` → `Son` → `Périphérique de sortie`
   - Sélectionnez votre casque

2. **Tester le son**
   - Lancez une vidéo YouTube pour vérifier

3. **Redémarrer l'application**

---

### ❌ La jauge ne bouge pas / reste à 0

**Symptôme :** Capture démarrée, mais aucune valeur affichée

**Solutions :**

1. **Vérifier que l'audio joue**
   - Lancez une musique
   - Montez le volume

2. **Vérifier le périphérique actif**
   - L'app capture le périphérique **par défaut** Windows
   - Si vous changez de casque → **redémarrez l'app**

3. **Consulter les logs**
   - Cherchez les erreurs dans les logs

---

### ❌ Valeurs dB(A) incohérentes

**Symptôme :** L'app affiche 120 dB(A) alors que vous écoutez à faible volume (ou l'inverse)

**Solutions :**

| Mode | Explication |
|------|-------------|
| **Mode A** | ✅ **Normal** : Les valeurs sont **relatives**. Fiez-vous aux **couleurs**, pas aux chiffres absolus. |
| **Mode B** | ⚠️ Marge d'erreur ±6 dB normale. Si l'écart est > 10 dB : **calibrez** avec un sonomètre. |

**Note importante :**
> L'application ne peut **pas** mesurer le volume système Windows (limitation WASAPI loopback). Si vous changez le volume → **recalibrez**.

---

### ❌ Application freeze / CPU élevé

**Symptôme :** Interface qui rame ou CPU > 50%

**Solutions :**

1. Fermez les applications audio gourmandes (DAW, streaming)
2. Vérifiez la config système (minimum : 2 cores, 2 GHz)
3. Redémarrez l'application

---

### ❌ Export CSV échoue

**Symptôme :** Erreur lors de l'export

**Solutions :**

1. Vérifiez les permissions d'écriture (essayez `Documents` ou `Bureau`)
2. Vérifiez qu'il y a des données à exporter (lancez la capture quelques secondes)
3. Consultez les logs pour voir l'erreur détaillée

---

### ❌ Profil non détecté

**Symptôme :** Mode A actif alors que vous avez un casque reconnu (ex: Sony WH-1000XM4)

**Solutions :**

1. **Vérifier le nom du périphérique Windows**
   - `Paramètres` → `Son` → Noter le nom exact
   - Exemple : "WH-1000XM4" vs "Sony Wireless"

2. **Ajouter un pattern dans profiles.json**
   - Ouvrir `ApplAudition\Resources\profiles.json`
   - Ajouter votre pattern dans la section `"patterns"`
   - Recompiler l'app

3. **Consulter les logs**
   - Voir quel profil (ou non) a été détecté au démarrage

---

### ⚠️ Calibration grisée / ne fonctionne pas

**Symptôme :** Bouton "Calibrer" désactivé

**Solutions :**

1. **Vérifier que vous êtes en Mode B**
   - La calibration n'est disponible **qu'en Mode B**
   - Si Mode A : l'app ne peut pas calculer de SPL absolu

2. **Vérifier qu'un profil est détecté**
   - Le panneau "Profil détecté" doit être visible

---

## 📞 Besoin d'aide ?

### Ressources

- 📖 **Documentation complète** : `README.md` (racine du projet)
- 📝 **Concepts techniques** : `CLAUDE.md` (glossaire DSP, architecture)
- 🐛 **Signaler un bug** : Créer une issue GitHub (si projet publié)
- 💬 **FAQ** : Voir `README.md`

---

## 🎉 Profitez de l'application !

**Appli Audition** vous aide à surveiller votre exposition sonore, mais n'oubliez pas :

| ⚠️ Avertissements importants |
|-------------------------------|
| ✅ C'est un **outil indicatif**, pas un instrument médical certifié |
| ✅ En cas de doute, consultez un **audioprothésiste** |
| ✅ Faites des **pauses régulières** lors de l'écoute prolongée |
| ✅ Respectez les seuils OMS : **< 85 dB(A)** pour 8h d'exposition |

**Prenez soin de vos oreilles !** 🎧👂

---

**Dernière mise à jour** : 2025-10-09
