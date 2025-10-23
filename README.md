# Appli Audition

> **Application Windows de surveillance du niveau sonore au casque en temps réel**

[![.NET](https://img.shields.io/badge/.NET-8.0-512BD4)](https://dotnet.microsoft.com/)
[![License](https://img.shields.io/badge/license-MIT-blue.svg)](LICENSE)
[![Platform](https://img.shields.io/badge/platform-Windows%2010%2F11-0078D6)](https://www.microsoft.com/windows)

---

## 📋 Vue d'ensemble

**Appli Audition** est une application Windows qui mesure en temps réel le niveau sonore (dB(A)) de l'audio envoyé à votre casque ou vos écouteurs. Elle vous aide à surveiller votre exposition sonore pour protéger votre audition contre les dommages irréversibles.

### Pourquoi cette application ?

L'exposition prolongée à des niveaux sonores élevés (> 85 dB(A)) peut causer des **dommages auditifs irréversibles** (acouphènes, perte auditive). La plupart des utilisateurs de casques n'ont aucune idée du niveau sonore réel auquel ils s'exposent.

### Solution

Cette application capture le signal audio système via WASAPI loopback, applique une pondération A (simulation de l'oreille humaine), et affiche en temps réel :
- 🎚️ **Niveau dB(A) actuel** avec code couleur (vert/orange/rouge)
- 📊 **Graphe historique** sur 3 minutes
- 📈 **Leq (niveau équivalent)** sur 1 minute
- 🔔 **Alertes visuelles** si seuils de sécurité dépassés

### Philosophie

- ✅ **Fonctionne immédiatement** sans configuration complexe
- ✅ **100% offline** - aucune connexion réseau requise
- ✅ **Open source** - code transparent et vérifiable
- ✅ **Conservateur** - préfère sur-estimer le risque que le sous-estimer

---

## ✨ Fonctionnalités

| Fonctionnalité | Description |
|----------------|-------------|
| **Mesure temps réel** | Niveau dB(A) mis à jour toutes les 125 ms |
| **Code couleur** | 🟢 Vert (sûr) / 🟠 Orange (modéré) / 🔴 Rouge (danger) |
| **Graphe historique** | Visualisation 3 minutes glissantes (LiveCharts2) |
| **Leq et Pic** | Niveau équivalent continu + pic sur 1 minute |
| **Seuils personnalisables** | Définissez vos propres niveaux d'alerte |
| **Export CSV** | Exportez vos données pour analyse externe |
| **Dark mode** | Thème sombre pour le confort visuel |
| **Offline** | Fonctionne sans connexion internet |

---

## 💻 Installation

### Prérequis

| Composant | Minimum | Recommandé |
|-----------|---------|------------|
| **Système** | Windows 10 (1809+) | Windows 11 |
| **CPU** | 2 cores, 2 GHz | 4 cores, 3 GHz |
| **RAM** | 4 GB | 8 GB |
| **Runtime** | .NET 8 Desktop Runtime | - |

**Installer .NET 8 Desktop Runtime** : [Télécharger ici](https://dotnet.microsoft.com/download/dotnet/8.0)

### Option 1 : Lancer l'exécutable (développement)

```bash
# Depuis le dossier du projet
cd "C:\Users\lumin\Documents\Code\Appli Audition\ApplAudition\bin\Debug\net8.0-windows"
# Double-cliquez sur ApplAudition.exe
```

### Option 2 : Version portable (distribution)

1. Téléchargez `ApplAudition_portable.zip` depuis les [Releases](https://github.com/votreRepo/ApplAudition/releases)
2. Extrayez le contenu où vous voulez (Bureau, Documents, clé USB...)
3. Double-cliquez sur `ApplAudition.exe`
4. Aucune installation nécessaire !

### Option 3 : Installer avec MSIX (Windows Store)

1. Téléchargez `ApplAudition.msix` depuis les [Releases](https://github.com/votreRepo/ApplAudition/releases)
2. Double-cliquez sur le fichier .msix
3. Cliquez sur "Installer"
4. Lancez depuis le menu Démarrer

---

## 🚀 Démarrage rapide

### Première utilisation

1. **Configurez votre casque dans Windows**
   - Paramètres → Son → Sélectionnez votre casque comme périphérique de sortie

2. **Lancez ApplAudition.exe**
   - L'application démarre automatiquement la mesure

3. **Jouez de l'audio**
   - Musique, vidéo, jeux... n'importe quelle source

4. **Observez la jauge**
   - 🟢 Vert : < 70 dB(A) → Niveau sûr
   - 🟠 Orange : 70-85 dB(A) → Faire attention
   - 🔴 Rouge : > 85 dB(A) → DANGER ! Réduire immédiatement

---

## 📊 Interface

### Jauge principale

L'interface affiche en temps réel :
- **Niveau dB(A) actuel** : Valeur numérique + barre de progression colorée
- **Leq 1 min** : Niveau équivalent moyen sur la dernière minute
- **Pic** : Niveau maximum atteint récemment
- **Catégorie** : Safe / Moderate / Hazardous

### Graphe historique

- **Axe X** : Temps (3 minutes glissantes)
- **Axe Y** : Niveau dB(A) (0-120)
- **Tooltip** : Survolez pour voir la valeur exacte à un instant T

### Paramètres

- 🌙 **Dark mode** : Basculer entre thème clair et sombre
- 🔔 **Seuils personnalisés** : Définir vos propres niveaux d'alerte
- 💾 **Export CSV** : Exporter l'historique (timestamp, dBFS, dB(A), Leq, Pic)

---

## 📏 Seuils et recommandations

### Code couleur par défaut

| Couleur | Plage | Durée max recommandée (OMS) |
|---------|-------|------------------------------|
| 🟢 **Vert** | < 70 dB(A) | Illimitée |
| 🟠 **Orange** | 70-85 dB(A) | 2-8 heures |
| 🔴 **Rouge** | > 85 dB(A) | < 2 heures |

**Note** : Ces seuils sont basés sur les recommandations de l'Organisation Mondiale de la Santé (OMS) avec un biais conservateur de -5 dB (sur-estimation pour sécurité).

### Durées d'exposition selon l'OMS

| Niveau sonore | Durée maximale par jour |
|---------------|-------------------------|
| < 85 dB(A) | 8 heures (sûr) |
| 85-90 dB(A) | 2-4 heures |
| 90-95 dB(A) | 30 min - 1 heure |
| 95-100 dB(A) | 15 minutes |
| > 100 dB(A) | Éviter complètement |

### Règle des 60/60

**60% du volume maximum** pendant **60 minutes maximum**, puis faites une pause de 10-15 minutes.

---

## ⚠️ Limites importantes

### Ce que l'application MESURE

✅ Le **signal numérique** envoyé au périphérique audio (dBFS)
✅ L'**estimation du niveau dB(A)** après pondération A
✅ Le **niveau équivalent continu** (Leq) sur 1 minute

### Ce que l'application NE MESURE PAS

❌ La **pression acoustique réelle** au conduit auditif
❌ Votre **audition personnelle** (seuil, sensibilité, acouphènes)
❌ Les **fuites** ou le **fit** du casque (±10 dB d'impact)
❌ Le **volume système Windows** (inaccessible via WASAPI loopback)
❌ Les **égaliseurs externes** (Dolby, SoundBlaster, etc.)

### Variables non contrôlées

| Variable | Impact sur mesure | Note |
|----------|-------------------|------|
| **Volume système** | ±20 dB | API Windows non accessible |
| **Fit du casque** | ±10 dB | Étanchéité, position, coussinets usés |
| **Égaliseurs externes** | ±6 dB | Modifications audio tierces |
| **Impédance de sortie** | ±3 dB | Dépend de la carte son / DAC |

### Marge d'erreur

L'estimation a une marge d'erreur typique de **±5-8 dB**.

**Conseil** : Fiez-vous aux **couleurs** (vert/orange/rouge) plutôt qu'aux valeurs exactes en dB.

---

## ❓ FAQ

### L'application affiche 0.0 dB(A), pourquoi ?

**Causes possibles** :
1. Aucun audio en cours de lecture → Lancez de la musique/vidéo
2. Périphérique audio non détecté → Vérifiez Paramètres Windows → Son
3. Volume trop bas → Montez le volume

### L'application consomme trop de CPU ?

**Solutions** :
1. Fermez les applications audio gourmandes (DAW, streaming)
2. Vérifiez la config minimale (2 cores, 4 GB RAM)
3. Consultez les logs : `%LOCALAPPDATA%\ApplAudition\logs\`

### Puis-je utiliser l'application avec des haut-parleurs ?

Oui, mais l'estimation sera **très imprécise** (distance, acoustique de la pièce). L'application est conçue pour les casques/écouteurs.

### L'application fonctionne-t-elle hors ligne ?

Oui, **100% offline**. Aucune connexion réseau, aucune donnée envoyée.

### Quelle est la différence entre dBFS et dB(A) ?

- **dBFS** : Échelle numérique (Full Scale digital) - 0 dBFS = signal maximal avant saturation
- **dB(A)** : Décibels pondérés A - simule la sensibilité de l'oreille humaine

L'application convertit dBFS → dB(A) via pondération A (filtre IEC 61672:2003).

---

## 🛠️ Architecture technique

### Stack

- **.NET 8** (C# 12, nullable reference types)
- **WPF** (Windows Presentation Foundation)
- **MVVM** (Model-View-ViewModel pattern)
- **NAudio** (WASAPI loopback, traitement audio)
- **LiveCharts2** (graphiques temps réel)
- **Serilog** (logging structuré)

### Pipeline DSP

1. **Capture WASAPI loopback** : 48 kHz, 32-bit float, stéréo
2. **Conversion mono** : (L+R)/2
3. **Filtre pondération A** : Biquad IIR cascade (IEC 61672:2003)
4. **Fenêtrage Hann** : Atténuation des bords
5. **Calcul RMS** : sqrt(Σ(x²)/N)
6. **Conversion dBFS** : 20·log10(RMS)
7. **Calcul Leq_1min** : 10·log10(mean(10^(Li/10)))
8. **Catégorisation** : Safe / Moderate / Hazardous

### Performance

- **CPU** : < 10% (mesure continue)
- **RAM** : ~50 MB
- **Latence** : 125 ms (8 mises à jour/sec)

---

## 🤝 Contributions

Les contributions sont les bienvenues !

### Comment contribuer

1. **Forker** le dépôt
2. **Créer une branche** : `git checkout -b feature/ma-fonctionnalite`
3. **Committer** : `git commit -m "Ajout de ma fonctionnalité"`
4. **Pusher** : `git push origin feature/ma-fonctionnalite`
5. **Ouvrir une Pull Request**

### Types de contributions

- 🐛 **Bug reports** : Issues avec logs et description détaillée
- 💡 **Suggestions** : Proposer de nouvelles fonctionnalités
- 📚 **Documentation** : Améliorer README, guides, FAQ
- 🧪 **Tests** : Ajouter tests unitaires ou d'intégration
- 🌐 **Traductions** : Support multi-langues

---

## 📚 Documentation

- **GUIDE_UTILISATEUR.md** : Guide détaillé pour l'utilisateur final (installation, utilisation, dépannage)
- **BUILD.md** : Instructions de compilation (portable, MSIX, Visual Studio)
- **CLAUDE.md** : Architecture complète et plan d'implémentation (développeurs)
- **ApplAudition.Tests/README.md** : Documentation des tests unitaires

---

## 📝 License

**MIT License** - Copyright (c) 2025 Appli Audition Contributors

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.

---

## ⚠️ Avertissement final

**Cette application est un outil indicatif, pas un dispositif médical certifié.**

- Ne remplace pas une consultation ORL en cas de symptômes auditifs (acouphènes, perte auditive, douleur)
- Ne garantit aucune protection contre les dommages auditifs
- L'utilisateur est seul responsable de la gestion de son exposition sonore
- Les développeurs déclinent toute responsabilité en cas de dommages

**En cas de symptômes auditifs, consultez immédiatement un professionnel ORL.**

---

**Protégez votre audition. Elle est irremplaçable. 👂🎧**

---

*Dernière mise à jour : Octobre 2025*
*Version : 1.0*
