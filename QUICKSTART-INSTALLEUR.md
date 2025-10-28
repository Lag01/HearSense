# Guide de démarrage rapide - Création de l'installeur

**HearSense - Créé par Erwan GUEZINGAR**

## 📋 Ce dont vous avez besoin

### 1. Installer Inno Setup (une seule fois)

**Inno Setup n'est PAS installé sur votre machine.**

1. **Téléchargez Inno Setup 6** :
   - Allez sur : https://jrsoftware.org/isdl.php
   - Téléchargez : **Inno Setup 6.x.x** (Unicode version)
   - Taille : ~3 MB

2. **Installez-le** :
   - Double-cliquez sur le fichier téléchargé
   - Suivez l'assistant (installation par défaut)
   - Durée : ~1 minute

3. **C'est tout !** Vous n'aurez plus jamais à refaire cette étape.

---

## 🚀 Créer l'installeur

Une fois Inno Setup installé, c'est très simple :

### Méthode automatique (Recommandée)

1. Ouvrez **PowerShell** dans le dossier du projet
2. Exécutez :
   ```powershell
   .\Build-Installer.ps1
   ```
3. Attendez 2-3 minutes
4. L'installeur sera créé dans `Build\HearSense_1.6_Setup.exe`

### Méthode manuelle (si le script échoue)

1. **Build de l'application** :
   ```powershell
   dotnet publish HearSense\HearSense.csproj --configuration Release --runtime win-x64 --self-contained true --output Build\Release\HearSense
   ```

2. **Ouvrez Inno Setup** :
   - Lancez Inno Setup depuis le menu Démarrer
   - Ouvrez le fichier `HearSense-Installer.iss`
   - Cliquez sur "Build" > "Compile" (ou F9)

3. **Récupérez l'installeur** :
   - Il sera dans `Build\HearSense_1.6_Setup.exe`

---

## 📦 Distribuer l'installeur

Une fois `HearSense_1.6_Setup.exe` créé :

1. **Envoyez ce fichier** à vos amis (par email, OneDrive, WeTransfer, etc.)
2. **Ils double-cliquent dessus** pour installer
3. **C'est tout !** L'application s'installe automatiquement

---

## ✅ Test rapide (recommandé)

Avant d'envoyer l'installeur à vos amis, testez-le :

1. **Fermez HearSense** s'il est ouvert
2. **Lancez l'installeur** : `Build\HearSense_1.6_Setup.exe`
3. **Suivez l'assistant** d'installation
4. **Vérifiez** que l'application démarre depuis le menu Démarrer
5. **Désinstallez** via Paramètres > Applications (pour vérifier que ça fonctionne)

---

## ❓ Problèmes courants

### Le script Build-Installer.ps1 dit "Inno Setup introuvable"

- Vérifiez que vous avez bien installé Inno Setup
- Relancez PowerShell après l'installation
- Le script cherche dans : `C:\Program Files (x86)\Inno Setup 6\ISCC.exe`

### Erreur "dotnet introuvable"

- Installez .NET 8 SDK : https://dotnet.microsoft.com/download/dotnet/8.0

### L'installeur est très gros (> 100 MB)

- C'est normal ! L'installeur contient :
  - L'application (~20 MB)
  - Toutes les dépendances .NET (~40 MB)
  - Les bibliothèques (NAudio, LiveCharts, etc.)

---

## 📊 Ce que fait le script Build-Installer.ps1

1. ✅ Vérifie que dotnet et Inno Setup sont installés
2. ✅ Build l'application en mode Release self-contained
3. ✅ Nettoie les fichiers inutiles (.pdb, etc.)
4. ✅ Compile le script Inno Setup
5. ✅ Crée `HearSense_1.6_Setup.exe`

---

## 🔄 Mettre à jour la version

Si vous voulez créer une nouvelle version (1.7, 1.8, etc.) :

1. **Modifiez la version** dans `HearSense-Installer.iss` :
   ```
   #define MyAppVersion "1.7"  ← Changez ici
   ```

2. **Relancez le script** :
   ```powershell
   .\Build-Installer.ps1
   ```

3. **Nouveau fichier créé** : `HearSense_1.7_Setup.exe`

---

**Besoin d'aide ?** Consultez [INSTALLATION.md](INSTALLATION.md) pour plus de détails.
