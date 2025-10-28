# Guide d'installation et de distribution - HearSense

**Créé par Erwan GUEZINGAR**

Ce guide explique comment créer un installeur pour HearSense et comment le distribuer à vos amis.

---

## ⚠️ Avertissement

**Les valeurs affichées par HearSense sont des ESTIMATIONS INDICATIVES uniquement.**

Cette application NE REMPLACE PAS un sonomètre professionnel certifié. Les mesures peuvent varier selon le casque, le volume système, etc. En cas de doute sur votre audition, consultez un professionnel de la santé auditive.

---

## 📦 Pour le développeur : Créer l'installeur

### Prérequis

1. **Inno Setup 6.x** (gratuit)
   - Téléchargez depuis : https://jrsoftware.org/isdl.php
   - Installez la version recommandée (Unicode version)
   - Durée d'installation : ~2 minutes

2. **.NET 8 SDK** (déjà installé si vous développez l'application)
   - Si nécessaire : https://dotnet.microsoft.com/download/dotnet/8.0

### Étapes de création de l'installeur

#### Option 1 : Script automatisé (recommandé)

1. Ouvrez PowerShell dans le dossier du projet
2. Exécutez le script :
   ```powershell
   .\Build-Installer.ps1
   ```
3. Attendez la fin du build (~2-3 minutes selon votre machine)
4. L'installeur sera créé dans `Build\HearSense_1.6_Setup.exe`

**Le script fait automatiquement :**
- ✅ Build de l'application en mode Release
- ✅ Nettoyage des fichiers inutiles
- ✅ Compilation du script Inno Setup
- ✅ Génération de l'installeur .exe

#### Option 2 : Manuel (si le script échoue)

1. **Build de l'application** :
   ```powershell
   dotnet publish HearSense\HearSense.csproj --configuration Release --runtime win-x64 --self-contained true --output Build\Release\HearSense
   ```

2. **Compilation de l'installeur** :
   - Ouvrez `HearSense-Installer.iss` avec Inno Setup
   - Cliquez sur "Build" > "Compile" (ou appuyez sur F9)
   - L'installeur sera créé dans le dossier `Build\`

### Résultat

Vous obtiendrez un fichier nommé : **`HearSense_1.6_Setup.exe`** (~60-80 MB)

Ce fichier est prêt à être distribué !

---

## 👥 Pour vos amis : Installer HearSense

### Installation

1. **Téléchargez** le fichier `HearSense_1.6_Setup.exe`

2. **Double-cliquez** sur le fichier pour lancer l'installation

3. **Suivez l'assistant d'installation** :
   - Acceptez l'emplacement d'installation (ou choisissez-en un autre)
   - Choisissez si vous voulez un raccourci sur le Bureau
   - Cliquez sur "Installer"

4. **Si .NET 8 Runtime n'est pas installé** :
   - L'installeur le détectera automatiquement
   - Il proposera de télécharger et installer .NET 8 Desktop Runtime
   - Suivez les instructions à l'écran (~50 MB supplémentaires)
   - Relancez l'installeur après l'installation de .NET

5. **Terminé !**
   - L'application apparaît dans le menu Démarrer
   - Vous pouvez la lancer immédiatement

### Première utilisation

1. Lancez **HearSense** depuis le menu Démarrer
2. L'application démarre automatiquement la surveillance audio
3. La jauge affiche le niveau sonore en temps réel
4. Code couleur :
   - 🟢 **Vert** : Niveau sûr (< 70 dB(A))
   - 🟠 **Orange** : Niveau modéré (70-80 dB(A))
   - 🔴 **Rouge** : Niveau élevé (> 80 dB(A))

### Désinstallation

**Méthode 1 : Paramètres Windows**
1. Ouvrez **Paramètres Windows** > **Applications** > **Applications installées**
2. Recherchez **HearSense**
3. Cliquez sur les trois points > **Désinstaller**
4. Confirmez la désinstallation

**Méthode 2 : Panneau de configuration (Windows 10)**
1. Ouvrez le **Panneau de configuration**
2. Allez dans **Programmes** > **Désinstaller un programme**
3. Sélectionnez **HearSense**
4. Cliquez sur **Désinstaller**

**Méthode 3 : Menu Démarrer**
1. Ouvrez le menu Démarrer
2. Cherchez **HearSense**
3. Cliquez sur **Désinstaller HearSense**

⚠️ **Note** : La désinstallation supprime l'application mais conserve vos paramètres personnalisés dans `%LOCALAPPDATA%\HearSense` si vous souhaitez les récupérer plus tard.

---

## 🔧 Dépannage

### L'installeur ne démarre pas

**Problème** : Double-clic sans effet ou message d'erreur

**Solutions** :
1. Vérifiez que vous avez les droits administrateur
2. Désactivez temporairement l'antivirus (il peut bloquer les installeurs non signés)
3. Faites clic droit > "Exécuter en tant qu'administrateur"

### Message "Windows a protégé votre PC"

**Problème** : Windows SmartScreen bloque l'installation

**Solution** :
1. Cliquez sur "Informations complémentaires"
2. Cliquez sur "Exécuter quand même"
3. C'est normal pour les applications non signées numériquement

### L'application ne démarre pas après installation

**Problème** : Clic sur l'icône sans effet

**Solutions** :
1. Vérifiez que .NET 8 Desktop Runtime est installé :
   - Ouvrez PowerShell et tapez : `dotnet --list-runtimes`
   - Vous devriez voir : `Microsoft.WindowsDesktop.App 8.0.x`
2. Si absent, téléchargez : https://aka.ms/dotnet/8.0/windowsdesktop-runtime-win-x64.exe

### L'application se ferme immédiatement

**Problème** : L'application démarre puis se ferme

**Solutions** :
1. Vérifiez les logs dans : `%LOCALAPPDATA%\HearSense\logs`
2. Assurez-vous qu'un périphérique audio est actif (casque/haut-parleurs)

---

## 📊 Tailles des fichiers

| Élément | Taille approximative |
|---------|---------------------|
| Installeur (`HearSense_1.6_Setup.exe`) | 60-80 MB |
| .NET 8 Desktop Runtime (si nécessaire) | ~50 MB |
| Application installée sur le disque | ~150 MB |

---

## 🔐 Sécurité et confidentialité

- ✅ **100% offline** : Aucune connexion réseau requise
- ✅ **Pas de télémétrie** : Aucune donnée envoyée à des serveurs
- ✅ **Open source** : Code source vérifiable
- ✅ **Logs locaux** : Toutes les données restent sur votre machine

---

## ❓ Questions fréquentes (FAQ)

### Puis-je installer HearSense sur plusieurs ordinateurs ?

Oui, vous pouvez installer l'application sur autant d'ordinateurs que vous voulez. L'installeur peut être partagé librement.

### L'application fonctionne-t-elle avec tous les casques ?

Oui, HearSense mesure le signal audio envoyé par Windows, indépendamment du casque utilisé. Elle fonctionne avec :
- Casques filaires (jack 3.5mm, USB)
- Casques Bluetooth
- Écouteurs sans fil
- Haut-parleurs intégrés

### L'application ralentit-elle mon ordinateur ?

Non, HearSense utilise moins de 5% du CPU et environ 100 MB de RAM. Elle fonctionne en arrière-plan sans impact notable sur les performances.

### Puis-je désactiver les notifications ?

Oui, dans les paramètres de l'application, vous pouvez personnaliser les seuils d'alerte ou les désactiver complètement.

### L'application est-elle compatible avec les jeux ?

Oui, HearSense fonctionne en arrière-plan et mesure tous les sons système, y compris ceux des jeux, de la musique, des vidéos, etc.

---

## 📞 Support

Si vous rencontrez des problèmes :
1. Consultez la section **Dépannage** ci-dessus
2. Vérifiez les logs dans `%LOCALAPPDATA%\HearSense\logs`
3. Ouvrez une issue sur le dépôt GitHub du projet

---

## 📜 License

HearSense est distribué sous licence MIT. Voir le fichier `LICENSE` pour plus de détails.

---

**Version du guide** : 1.6
**Dernière mise à jour** : 28 octobre 2025
