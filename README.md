# Appli Audition

> **Application Windows d'estimation du niveau sonore au casque**
>
> Estimation en temps réel du niveau dB(A) à partir du signal audio système, sans configuration obligatoire.

[![.NET](https://img.shields.io/badge/.NET-8.0-512BD4)](https://dotnet.microsoft.com/)
[![License](https://img.shields.io/badge/license-MIT-blue.svg)](LICENSE)
[![Platform](https://img.shields.io/badge/platform-Windows%2010%2F11-0078D6)](https://www.microsoft.com/windows)

---

## 📋 Table des matières

- [Vue d'ensemble](#vue-densemble)
- [Concepts techniques](#concepts-techniques)
- [Modes d'estimation](#modes-destimation)
- [Profils heuristiques](#profils-heuristiques)
- [Calibration optionnelle](#calibration-optionnelle)
- [⚠️ Limites importantes](#️-limites-importantes)
- [Installation](#installation)
- [Utilisation](#utilisation)
- [Configuration requise](#configuration-requise)
- [FAQ](#faq)
- [Contributions](#contributions)
- [License](#license)

---

## Vue d'ensemble

### Problématique

L'exposition prolongée à des niveaux sonores élevés (> 85 dB(A)) peut causer des **dommages auditifs irréversibles**. Les utilisateurs de casques et écouteurs manquent souvent de retour sur le niveau sonore réel auquel ils sont exposés.

### Solution

**Appli Audition** est une application Windows qui estime en temps réel le niveau sonore dB(A) au casque à partir du signal audio système (via WASAPI loopback), **sans exiger de configuration par défaut**.

### Philosophie "Zero-Input Conservateur"

1. **Priorité 1** : Fonctionner immédiatement sans configuration
2. **Priorité 2** : Sur-estimer modérément pour la sécurité (biais conservateur +5 dB)
3. **Priorité 3** : Améliorer la précision progressivement (profils heuristiques, calibration optionnelle)

### Cas d'usage

#### 1. Utilisateur lambda
Lance l'application → obtient une indication visuelle (🟢 Vert / 🟠 Orange / 🔴 Rouge) du niveau d'exposition relatif.

#### 2. Utilisateur avec casque reconnu
L'application détecte automatiquement "Sony WH-1000XM4" → affiche une estimation SPL absolue (±6 dB).

#### 3. Utilisateur exigeant
Calibre avec un sonomètre de référence → précision optimale pour son setup spécifique.

### Ce que l'application N'EST PAS

- ❌ Un sonomètre médical certifié
- ❌ Une mesure de l'audition personnelle (audiogramme)
- ❌ Un remplacement des protections auditives professionnelles
- ❌ Une mesure de la pression acoustique réelle au conduit auditif

---

## Concepts techniques

### Qu'est-ce que le dB(A) ?

Le **dB(A)** (décibel pondéré A) est une unité de mesure du niveau sonore qui simule la sensibilité de l'oreille humaine :

- Atténue les basses fréquences (ex: 100 Hz ≈ -20 dB)
- Référence : 1 kHz = 0 dB de correction
- Atténue légèrement les hautes fréquences (ex: 10 kHz ≈ -4 dB)

### Qu'est-ce que le dBFS ?

Le **dBFS** (decibels Full Scale) est l'échelle numérique utilisée dans les systèmes audio :

- 0 dBFS = amplitude numérique maximale (100%)
- -6 dBFS ≈ 50% de l'amplitude maximale
- -∞ dBFS = silence complet

**Important** : dBFS ≠ dB(A) SPL. Le dBFS est une mesure **numérique**, tandis que le dB(A) SPL est une mesure **physique** (pression acoustique).

### Qu'est-ce que le Leq ?

Le **Leq** (Equivalent Continuous Level) est le niveau équivalent continu, une moyenne logarithmique de l'énergie sonore sur une période donnée :

- Leq_1min = niveau moyen sur 1 minute
- Utilisé par les normes de santé au travail (NIOSH, OMS)
- Plus représentatif que les pics instantanés

### Qu'est-ce que le SPL ?

Le **SPL** (Sound Pressure Level) est le niveau de pression acoustique réel, mesuré en dB(A) :

- Référence : 20 µPa (seuil d'audition humaine)
- Mesuré avec un sonomètre étalonné
- Dépend du casque, du volume système, et du fit

---

## Modes d'estimation

L'application propose **deux modes d'estimation** selon le contexte :

### Mode A : Zero-Input Conservateur (par défaut)

**Activation** : Par défaut, toujours disponible, aucun périphérique reconnu.

**Affichage** : dB(A) **relatif** (pas de SPL absolu).

**Catégorisation** :
- 🟢 **Vert** : < 70 dB(A) relatif (écoute modérée, sans risque prolongé)
- 🟠 **Orange** : 70-80 dB(A) relatif (prolongée à limiter, 2-8h max)
- 🔴 **Rouge** : > 80 dB(A) relatif (potentiellement nocive, < 2h)

**Avertissement UI** : "Estimation conservatrice du signal numérique"

**Biais de sécurité** : Les seuils sont décalés de **-5 dB** (sur-estimation) pour la sécurité.

**Cas d'usage** : Indication relative sans calibration, fallback universel.

---

### Mode B : Auto-profil Heuristique

**Activation** : Automatique si le périphérique audio est reconnu (patterns JSON).

**Affichage** : SPL estimé en dB(A) **absolu** (avec marge d'erreur).

**Catégorisation** :
- 🟢 **Vert** : < 70 dB(A) SPL (sûr)
- 🟠 **Orange** : 70-80 dB(A) SPL (exposition modérée)
- 🔴 **Rouge** : > 80 dB(A) SPL (exposition dangereuse)

**Avertissement UI** : "Estimation heuristique, marge ±6 dB"

**Précision typique** : ±5-8 dB (selon profil, volume système, fit du casque).

**Cas d'usage** : Casques populaires reconnus automatiquement (Sony WH-1000XM4, AirPods Pro, Bose QC35, etc.).

**Override manuel** : Bouton "Forcer Mode A" disponible pour ignorer le profil.

---

### Comparaison Mode A vs Mode B

| Aspect | Mode A (Zero-Input) | Mode B (Auto-profil) |
|--------|---------------------|----------------------|
| **Activation** | Par défaut | Si périphérique reconnu |
| **Affichage** | dB(A) relatif | SPL estimé (dB(A) absolu) |
| **Catégories** | Seuils relatifs | Seuils absolus (normes OMS) |
| **Calibration** | C = 0 (référence numérique) | C = profil heuristique (ex: -15 dB) |
| **Avertissement** | "Estimation conservatrice" | "Marge ±6 dB" |
| **Précision** | N/A (valeur relative) | ±5-8 dB typique |

---

## Profils heuristiques

### Fonctionnement

L'application embarque une **base de profils JSON** pour les casques populaires :

1. **Détection automatique** : Nom du périphérique audio actif (ex: "Sony WH-1000XM4")
2. **Matching par patterns** : Regex pour reconnaître le modèle
3. **Constante C** : Offset de conversion dBFS → SPL (ex: -15 dB pour over-ear ANC)
4. **Marge d'erreur** : Indiquée dans l'UI (ex: ±6 dB)

### Profils inclus

| Type | Exemples | Constante C | Marge |
|------|----------|-------------|-------|
| **Over-ear ANC** (fermés) | Sony WH-1000XM3/4/5, Bose QC35/700 | -15 dB | ±6 dB |
| **On-ear** | Beats Solo, Sennheiser Momentum On-Ear | -12 dB | ±7 dB |
| **IEM** (intra-auriculaires) | AirPods Pro, Galaxy Buds, IEM génériques | -8 dB | ±8 dB |
| **Bluetooth générique** | Tout périphérique Bluetooth non reconnu | -12 dB | ±8 dB |

### Fallback

Si aucun profil ne correspond → **Mode A** (zero-input conservateur).

---

## Calibration optionnelle

### Pourquoi calibrer ?

- **Précision absolue** : Réduire la marge d'erreur à ±2-3 dB
- **Setup spécifique** : Prendre en compte votre casque exact + volume système habituel

### Procédure

1. **Matériel requis** :
   - Sonomètre de référence (IEC 61672 classe 2 minimum)
   - Coupleur acoustique ou mesure in-situ (oreille factice)

2. **Étapes** :
   - Jouer un signal de test (ex: bruit rose, musique connue)
   - Mesurer le SPL réel avec le sonomètre
   - Dans l'application : section "Calibration" → entrer la valeur mesurée
   - L'application ajuste la constante C automatiquement

3. **Validation** :
   - Badge "Calibré" affiché dans l'UI
   - Avertissement : "Valide uniquement pour ce périphérique + volume système"

### Limitations de la calibration

- ⚠️ **Volume système inaccessible** : Windows ne permet pas de lire le volume système via WASAPI loopback
- ⚠️ **Valide pour un volume fixe** : Si vous changez le volume, la calibration devient invalide
- ⚠️ **Périphérique spécifique** : Changer de casque invalide la calibration
- ⚠️ **Fit du casque** : Une mauvaise étanchéité peut varier de ±10 dB

---

## ⚠️ Limites importantes

### Ce que l'application mesure

✅ Le **signal numérique** envoyé au périphérique audio (dBFS)
✅ L'**estimation du SPL** basée sur des profils heuristiques (Mode B)
✅ Le **niveau équivalent continu** (Leq) sur 1 minute

### Ce que l'application NE mesure PAS

❌ La **pression acoustique réelle** au conduit auditif
❌ Votre **audition personnelle** (seuil, sensibilité, acouphènes)
❌ Les **fuites** ou le **fit** du casque
❌ Le **volume système** Windows (API non accessible)
❌ Les **EQ externes** ou effets audio (Dolby, SoundBlaster, etc.)

### Variables non contrôlées

| Variable | Impact sur SPL réel | Note |
|----------|---------------------|------|
| **Volume système** | ±20 dB | Inaccessible via WASAPI loopback |
| **Fit du casque** | ±10 dB | Étanchéité, position, usure des coussinets |
| **EQ externes** | ±6 dB | Égaliseurs Windows, logiciels tiers |
| **Impédance de sortie** | ±3 dB | Varie selon la carte son / DAC |
| **Sensibilité réelle** | ±5 dB | Tolérances fabricant, vieillissement |

### Biais conservateur

L'application applique un **biais de sécurité de -5 dB** sur les seuils de catégorisation :

- Objectif : **Sur-estimer** le risque plutôt que le sous-estimer
- Conséquence : Les alertes orange/rouge peuvent apparaître à des niveaux légèrement inférieurs aux normes OMS (85 dB(A))

### Avertissements légaux

⚠️ **Cette application est un outil indicatif, pas un dispositif médical certifié.**

- Ne remplace pas une consultation ORL en cas de symptômes (acouphènes, perte auditive, hyperacousie)
- Ne garantit aucune protection contre les dommages auditifs
- L'utilisateur est seul responsable de la gestion de son exposition sonore
- Les développeurs déclinent toute responsabilité en cas de dommages auditifs

⚠️ **Respectez les recommandations de l'OMS** :

- **< 85 dB(A)** : Exposition sûre jusqu'à 8h/jour
- **85-90 dB(A)** : Limiter à 2-4h/jour
- **90-95 dB(A)** : Limiter à 30 min - 1h/jour
- **> 95 dB(A)** : Éviter toute exposition prolongée (< 15 min)

---

## Installation

### Option 1 : Installer MSIX (recommandé)

1. **Télécharger** : `ApplAudition_1.0.0.msix` depuis [Releases](https://github.com/votreRepo/ApplAudition/releases)
2. **Double-cliquer** sur le fichier .msix
3. **Installer** : Windows va demander confirmation (cliquer "Installer")
4. **Lancer** : Via le menu Démarrer → "Appli Audition"

**Prérequis** : Windows 10 (1809+) ou Windows 11

**Certificat** : Vous devrez peut-être installer le certificat de signature la première fois (auto-signé en version dev).

---

### Option 2 : Version portable (.zip)

1. **Télécharger** : `ApplAudition_1.0.0_portable.zip` depuis [Releases](https://github.com/votreRepo/ApplAudition/releases)
2. **Extraire** : Décompresser le .zip dans un dossier de votre choix
3. **Lancer** : Double-cliquer sur `ApplAudition.exe`

**Avantages** :
- ✅ Pas d'installation requise
- ✅ Portable (USB, etc.)
- ✅ Aucune dépendance (.NET 8 embarqué)

**Inconvénients** :
- ❌ Taille plus importante (~100 MB vs ~10 MB pour MSIX)
- ❌ Pas de mise à jour automatique

---

## Utilisation

### Démarrage rapide

1. **Lancer l'application** : Via le menu Démarrer ou ApplAudition.exe
2. **L'interface affiche** :
   - 🎧 Mode actif (A ou B)
   - 🎚️ Jauge dB(A) en temps réel (vert/orange/rouge)
   - 📊 Graphe historique 3 minutes
   - 📈 Leq_1min (niveau équivalent) et Pic

3. **Jouer de l'audio** : Musique, vidéo, jeux, etc.
4. **Observer** :
   - Jauge change de couleur selon le niveau
   - Graphe montre l'historique récent
   - Leq donne le niveau moyen

### Interface

#### Jauge principale
- **Valeur numérique** : dB(A) actuel (relatif en Mode A, SPL estimé en Mode B)
- **Code couleur** :
  - 🟢 Vert : Niveau sûr (< 70 dB(A))
  - 🟠 Orange : Niveau modéré (70-80 dB(A)), limiter l'exposition
  - 🔴 Rouge : Niveau élevé (> 80 dB(A)), réduire le volume ou la durée

#### Graphe historique
- **Axe X** : Temps (3 minutes glissantes)
- **Axe Y** : dB(A)
- **Tooltip** : Valeur exacte au survol

#### Panneau "Mode actif"
- **Mode A** : "Zero-Input Conservateur" (badge bleu)
- **Mode B** : "Auto-profil Heuristique" (badge vert) + nom du profil détecté
- **Badge "Conservateur"** : Toujours visible, rappelle le biais de sécurité

#### Panneau "Profil détecté" (Mode B uniquement)
- Nom du profil (ex: "Over-ear ANC (fermés)")
- Constante C utilisée (ex: -15 dB)
- Marge d'erreur (ex: ±6 dB)
- Avertissement : "Estimation du signal envoyé, pas de votre audition"

#### Calibration (optionnelle)
- Section collapsible "Calibration"
- Instructions pour utiliser un sonomètre
- Champ : SPL mesuré (dB(A))
- Bouton "Calibrer" → ajuste la constante C
- Badge "Calibré" affiché si calibration active
- Bouton "Reset" pour revenir au profil heuristique

#### Paramètres
- 🌙 **Dark mode** : Toggle clair/sombre
- 💾 **Export CSV** : Exporter l'historique (timestamp, dBFS, dB(A), Leq, mode, profil)
- 🔄 **Forcer Mode A** : Ignorer le profil détecté et revenir en Mode A

### Logs

Les logs sont enregistrés dans :
```
%LOCALAPPDATA%\ApplAudition\logs\app-YYYYMMDD.log
```

Utiles pour :
- Déboguer les problèmes de détection de périphérique
- Vérifier quel profil a été sélectionné
- Analyser les erreurs de capture audio

---

## Configuration requise

| Composant | Minimum | Recommandé |
|-----------|---------|------------|
| **Système** | Windows 10 (1809+) | Windows 11 |
| **CPU** | 2 cores, 2 GHz | 4 cores, 3 GHz |
| **RAM** | 4 GB | 8 GB |
| **Audio** | WASAPI compatible | - |
| **Runtime** | .NET 8 Desktop Runtime (inclus si portable) | - |

---

## FAQ

### L'application affiche 0.0 dB(A), que faire ?

**Causes possibles** :
1. Aucun audio en cours de lecture → Jouer de la musique/vidéo
2. Périphérique audio non détecté → Vérifier les paramètres Windows
3. Erreur WASAPI loopback → Consulter les logs (%LOCALAPPDATA%\ApplAudition\logs)

### Pourquoi le Mode B n'est pas activé ?

Le Mode B nécessite que votre périphérique audio soit reconnu par les profils JSON embarqués. Si votre casque n'est pas dans la base :
- L'application reste en Mode A (zero-input conservateur)
- Vous pouvez proposer d'ajouter votre casque via une issue GitHub

### Comment savoir si ma calibration est correcte ?

Une calibration correcte doit donner une marge d'erreur de ±2-3 dB par rapport au sonomètre de référence. Testez avec plusieurs morceaux de musique et vérifiez la cohérence.

### L'application consomme trop de CPU (> 10%), pourquoi ?

**Solutions** :
1. Réduire la fréquence de rafraîchissement du graphe (paramètres)
2. Désactiver le graphe si non utilisé
3. Vérifier qu'aucune autre application audio intensive ne tourne en parallèle
4. Consulter les logs pour détecter des erreurs répétées

### Puis-je utiliser l'application avec des haut-parleurs ?

Oui, mais l'estimation SPL sera **très imprécise** (distance, acoustique de la pièce, etc.). L'application est conçue pour les casques/écouteurs, où le signal est relativement contrôlé.

### L'application fonctionne-t-elle hors ligne ?

Oui, **100% offline**. Aucune connexion réseau requise, aucune donnée envoyée.

### Quelle est la différence entre dBFS et dB(A) SPL ?

- **dBFS** : Échelle numérique (0 dBFS = signal maximal avant clipping digital)
- **dB(A) SPL** : Niveau de pression acoustique physique, mesuré en décibels pondérés A

L'application convertit dBFS → dB(A) SPL via une constante C (profil heuristique ou calibration).

### Pourquoi un biais conservateur de -5 dB ?

Pour **sur-estimer le risque** plutôt que le sous-estimer. Il vaut mieux afficher une alerte orange/rouge trop tôt que trop tard.

### Puis-je contribuer au projet ?

Oui ! Voir section [Contributions](#contributions).

---

## Contributions

### Comment contribuer

1. **Forker** le dépôt GitHub
2. **Créer une branche** : `git checkout -b feature/nouvelle-fonctionnalite`
3. **Committer** : `git commit -m "Ajout de nouvelle fonctionnalité"`
4. **Pusher** : `git push origin feature/nouvelle-fonctionnalite`
5. **Ouvrir une Pull Request**

### Types de contributions

- 🐛 **Bug reports** : Ouvrir une issue avec logs et description
- 💡 **Suggestions** : Proposer de nouvelles fonctionnalités
- 📚 **Documentation** : Améliorer le README, la FAQ, etc.
- 🎧 **Profils** : Ajouter de nouveaux profils de casques (avec données techniques)
- 🧪 **Tests** : Ajouter des tests unitaires ou d'intégration

### Proposer un nouveau profil

Pour ajouter un casque à la base de profils :

1. **Specs requises** :
   - Nom exact du périphérique (Windows)
   - Sensibilité (dB/mW ou dB SPL/V)
   - Impédance (Ω)
   - Type (over-ear, on-ear, IEM)

2. **Calibration recommandée** :
   - Mesurer le SPL avec un sonomètre de référence
   - Noter la constante C calculée
   - Indiquer la marge d'erreur observée

3. **Créer une issue** avec le template "Nouveau profil"

---

## Architecture technique

### Stack

- **.NET 8** (C# 12)
- **WPF** (Windows Presentation Foundation)
- **MVVM** (Model-View-ViewModel)
- **NAudio** (WASAPI loopback, DSP)
- **LiveCharts2** (graphe temps réel)
- **Serilog** (logging structuré)

### Pipeline DSP

1. **Capture WASAPI loopback** : 48 kHz, 32-bit float, stéréo
2. **Conversion mono** : (L+R)/2
3. **Filtre pondération A** : Biquad IIR cascade (IEC 61672:2003)
4. **Fenêtrage Hann** : w[n] = 0.5·(1 - cos(2πn/(N-1)))
5. **Calcul RMS** : sqrt(Σ(x²)/N)
6. **Conversion dBFS** : 20·log10(RMS)
7. **Calcul Leq_1min** : 10·log10(mean(10^(Li/10)))
8. **Estimation SPL** (Mode B) : dBFS + C
9. **Catégorisation** : Safe/Moderate/Hazardous

### Performance

- **CPU** : < 10% (mesure continue)
- **RAM** : ~50 MB
- **Latence** : 125 ms (8 updates/sec)

---

## Roadmap

### Version 1.0 (actuelle)

- ✅ Mode A (zero-input conservateur)
- ✅ Mode B (auto-profil heuristique)
- ✅ Profils embarqués (over-ear, on-ear, IEM)
- ✅ Calibration optionnelle
- ✅ Export CSV
- ✅ Dark mode
- ✅ Graphe historique 3 min

### Version 1.1 (prévu)

- 🔄 Profils cloud (mise à jour automatique)
- 🔄 Dose d'exposition quotidienne (cumul 24h)
- 🔄 Notifications système (alerte seuil dépassé)
- 🔄 Support multi-langues (EN, FR, ES, DE)

### Version 2.0 (futur)

- 🔮 Analyse spectrale (visualisation fréquences)
- 🔮 Détection automatique du volume système (si API disponible)
- 🔮 Mode "blind test" (calibration sans sonomètre)
- 🔮 Intégration Spotify/Apple Music (métadonnées)

---

## License

**MIT License**

Copyright (c) 2025 Appli Audition Contributors

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

## Contact & Support

- **Issues** : [GitHub Issues](https://github.com/votreRepo/ApplAudition/issues)
- **Discussions** : [GitHub Discussions](https://github.com/votreRepo/ApplAudition/discussions)
- **Documentation technique** : Voir `CLAUDE.md`

---

**⚠️ AVERTISSEMENT FINAL** : Cette application est un outil indicatif. En cas de symptômes auditifs (acouphènes, perte auditive, douleur), consultez un professionnel ORL immédiatement.

**Protégez votre audition. Elle est irremplaçable.**

---

*Dernière mise à jour : 2025-10-08*
*Version : 1.0.0*
