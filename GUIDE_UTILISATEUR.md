# Guide Utilisateur - HearSense

> **Surveillez le niveau sonore de votre casque en temps réel pour protéger votre audition**

---

## Qu'est-ce qu'HearSense ?

HearSense est une application Windows qui mesure en temps réel le niveau sonore (en dB(A)) de l'audio envoyé à votre casque ou vos écouteurs. Elle vous aide à surveiller votre exposition sonore pour éviter les dommages auditifs irréversibles.

**Ce que fait l'application :**
- ✅ Mesure le niveau sonore en temps réel (mise à jour toutes les 125ms)
- ✅ Affiche une jauge colorée (vert/orange/rouge) selon le niveau de risque
- ✅ Enregistre un historique sur 3 minutes
- ✅ Vous alerte si vous dépassez les seuils de sécurité
- ✅ Fonctionne 100% en local, sans connexion internet

**Ce que l'application N'EST PAS :**
- ❌ Un outil médical certifié
- ❌ Un audiogramme (test d'audition)
- ❌ Une mesure parfaitement précise (marge d'erreur de ±5-8 dB)

---

## Installation

### Méthode 1 : Lancer l'exécutable (le plus simple)

Si vous avez déjà le dossier du projet :

1. Ouvrez l'Explorateur Windows
2. Naviguez vers : `C:\Users\lumin\Documents\Code\HearSense\HearSense\bin\Debug\net8.0-windows\`
3. Double-cliquez sur **HearSense.exe**

**Astuce** : Créez un raccourci sur le Bureau pour y accéder rapidement !

### Méthode 2 : Version portable (pour partager)

Si vous avez un fichier .zip :

1. Extrayez le contenu du .zip où vous voulez (Bureau, Documents, clé USB...)
2. Double-cliquez sur **HearSense.exe**
3. Aucune installation nécessaire !

### Méthode 3 : Ligne de commande

Si vous êtes développeur :

```powershell
cd "C:\Users\lumin\Documents\Code\HearSense"
dotnet run --project HearSense\HearSense.csproj
```

**Prérequis** : .NET 8 Desktop Runtime installé ([Télécharger ici](https://dotnet.microsoft.com/download/dotnet/8.0))

---

## Premier lancement

### Configuration audio

**Avant de lancer l'application**, configurez votre casque dans Windows :

1. Ouvrez **Paramètres Windows** → **Système** → **Son**
2. Dans "Périphérique de sortie", sélectionnez votre casque
3. Testez avec une vidéo YouTube pour vérifier

### Démarrer l'application

1. **Lancez HearSense.exe**
2. L'application démarre automatiquement la mesure
3. **Jouez de l'audio** (musique, vidéo, jeux...)
4. **Observez la jauge** bouger en temps réel

**C'est tout !** Vous surveillez maintenant votre exposition sonore.

---

## Interface principale

### Jauge de niveau sonore

```
┌─────────────────────────────────┐
│  🎚️ 72.5 dB(A)    🟢           │  ← Niveau actuel
│  ████████████░░░░░░░░░░         │  ← Barre de progression
│                                 │
│  📊 Leq 1 min : 68.3 dB(A)     │  ← Moyenne sur 1 minute
│  📍 Pic : 75.2 dB(A)            │  ← Maximum récent
│                                 │
│  [Graphe historique 3 minutes] │  ← Évolution temporelle
└─────────────────────────────────┘
```

### Code couleur

L'application utilise un système de feux de signalisation :

| Couleur | Plage | Signification | Durée max recommandée |
|---------|-------|---------------|----------------------|
| 🟢 **VERT** | < 70 dB(A) | Niveau sûr, écoute confortable | Illimitée |
| 🟠 **ORANGE** | 70-85 dB(A) | Niveau modéré, faire attention | 2-8 heures |
| 🔴 **ROUGE** | > 85 dB(A) | ⚠️ DANGER ! Réduire immédiatement | < 2 heures |

**Important** : Ces seuils sont basés sur les recommandations de l'Organisation Mondiale de la Santé (OMS).

### Graphe historique

- **Axe horizontal** : Temps (3 dernières minutes)
- **Axe vertical** : Niveau dB(A)
- **Survolez la courbe** : Tooltip avec valeur exacte

---

## Personnaliser les seuils d'alerte

L'application vous permet de définir vos propres niveaux d'alerte selon votre tolérance personnelle.

### Pourquoi personnaliser ?

- Vous êtes plus sensible au bruit → baissez les seuils
- Vous voulez suivre les recommandations OMS strictes → 85 dB(A) max
- Vous préférez des alertes plus conservatrices → réglez orange à 65 dB(A)

### Comment régler

1. Ouvrez les **Paramètres** de l'application
2. Utilisez les curseurs pour ajuster :
   - **Seuil Orange (Avertissement)** : Défaut 70 dB(A), plage 60-90 dB(A)
   - **Seuil Rouge (Danger)** : Défaut 85 dB(A), plage 75-100 dB(A)
3. Les changements sont appliqués **immédiatement** et automatiquement sauvegardés

### Exemple de personnalisation

| Profil utilisateur | Seuil Orange | Seuil Rouge | Objectif |
|-------------------|--------------|-------------|----------|
| **Très prudent** | 60 dB(A) | 70 dB(A) | Protection maximale |
| **Conservateur** (défaut) | 70 dB(A) | 85 dB(A) | Marge de sécurité |
| **OMS strict** | 80 dB(A) | 85 dB(A) | Recommandation OMS uniquement |

---

## Recommandations de sécurité auditive

### Règle des 60/60

**60% du volume maximum** pendant **60 minutes maximum**, puis faites une pause.

### Durées d'exposition selon l'OMS

| Niveau sonore | Durée maximale par jour |
|---------------|-------------------------|
| < 85 dB(A) | 8 heures (sûr) |
| 85-90 dB(A) | 2-4 heures |
| 90-95 dB(A) | 30 min - 1 heure |
| 95-100 dB(A) | 15 minutes |
| > 100 dB(A) | Éviter complètement |

### Signes d'alerte

Consultez un médecin ORL si vous ressentez :
- 🔔 Acouphènes (sifflements dans les oreilles)
- 📢 Difficulté à comprendre les conversations
- 🔇 Sensation d'oreilles bouchées après écoute
- 💢 Douleur ou inconfort auditif

---

## Dépannage rapide

### ❌ L'application ne démarre pas

**Vérifiez** :
1. .NET 8 Desktop Runtime installé ? → [Télécharger](https://dotnet.microsoft.com/download/dotnet/8.0)
2. Double-cliquez sur le bon fichier `.exe` (pas sur un `.dll`)

### ❌ La jauge reste à 0 ou ne bouge pas

**Solutions** :
1. **Jouez de l'audio** (musique, vidéo) - l'application mesure le son système
2. **Vérifiez le périphérique actif** dans Paramètres Windows → Son
3. **Montez le volume** (si très bas, la mesure peut être proche de 0)
4. **Redémarrez l'application** si vous avez changé de casque

### ❌ "Aucun périphérique audio détecté"

**Étapes** :
1. Allez dans **Paramètres Windows** → **Son**
2. Sélectionnez votre casque comme "Périphérique de sortie"
3. Testez le son avec une vidéo
4. Redémarrez l'application

### ❌ Les valeurs semblent incohérentes

**Explications** :
- L'application mesure le **signal numérique envoyé** au casque, pas la pression acoustique réelle dans votre oreille
- Marge d'erreur normale : **±5-8 dB**
- Variables non contrôlées : fit du casque, isolation, type d'écouteurs

**Solution** :
1. Fiez-vous aux **couleurs** (vert/orange/rouge) plutôt qu'aux valeurs exactes en dB
2. **Personnalisez les seuils** selon votre ressenti personnel dans les Paramètres

### ❌ L'application consomme trop de ressources

**Solutions** :
1. Fermez les autres applications audio gourmandes (DAW, streaming)
2. Vérifiez que votre PC répond aux prérequis (2 cores, 4 GB RAM minimum)
3. Consultez les logs : `%LOCALAPPDATA%\HearSense\logs\`

---

## Export des données

Vous pouvez exporter l'historique de vos mesures au format CSV pour analyse (Excel, LibreOffice...).

**Procédure** :
1. Cliquez sur **Export CSV** (en bas de l'interface)
2. Choisissez l'emplacement et le nom du fichier
3. Ouvrez le .csv dans votre tableur

**Colonnes exportées** :
- Timestamp (date et heure)
- dBFS (niveau numérique)
- dB(A) (niveau pondéré A)
- Leq_1min (moyenne sur 1 minute)
- Peak (pic)

---

## Mode sombre

Pour passer en mode sombre :
- Cliquez sur l'icône **🌙 Dark** en haut à droite

Le thème est sauvegardé automatiquement entre les sessions.

---

## Logs et données

### Emplacement des logs

Les logs sont enregistrés dans :
```
C:\Users\<VotreNom>\AppData\Local\HearSense\logs\
```

**Utilité** : Déboguer les problèmes, vérifier la détection du périphérique

### Emplacement des paramètres

Les paramètres (thème, seuils personnalisés) sont dans :
```
C:\Users\<VotreNom>\AppData\Local\HearSense\settings.json
```

---

## Désinstallation

**Version portable** :
1. Supprimez le dossier contenant HearSense.exe
2. (Optionnel) Supprimez `%LOCALAPPDATA%\HearSense\`

**Version installée (.msix)** :
1. **Paramètres Windows** → **Applications**
2. Recherchez "HearSense"
3. Cliquez sur **Désinstaller**

---

## Besoin d'aide ?

### Documentation complète

- **README.md** : Documentation technique détaillée
- **BUILD.md** : Guide pour compiler le projet
- **CLAUDE.md** : Architecture du logiciel (pour développeurs)

### Support

- **GitHub Issues** : Signaler un bug ou suggérer une fonctionnalité
- **Logs** : Consultez `%LOCALAPPDATA%\HearSense\logs\` pour diagnostiquer

---

## Avertissement final

⚠️ **Cette application est un outil indicatif, pas un dispositif médical certifié.**

- Elle ne remplace pas une consultation médicale en cas de symptômes auditifs
- Les développeurs déclinent toute responsabilité en cas de dommages auditifs
- L'utilisateur est seul responsable de la gestion de son exposition sonore

**Protégez votre audition. Elle est irremplaçable. 👂🎧**

---

**Version** : 1.0
**Dernière mise à jour** : Octobre 2025
**License** : MIT
