====================================
  APPLI AUDITION - VERSION PORTABLE
====================================

Version : 1.0.0
Date    : 2025-10-08
License : MIT


INSTALLATION
============

1. Extraire tout le contenu de l'archive .zip dans un dossier de votre choix
   (ex: C:\Programs\ApplAudition ou une clé USB)

2. Double-cliquer sur ApplAudition.exe pour lancer l'application

3. Aucune installation supplémentaire requise (.NET 8 est embarqué)


CONFIGURATION REQUISE
=====================

- Windows 10 (1809+) ou Windows 11
- 64-bit (x64)
- Périphérique audio compatible WASAPI
- ~150 MB d'espace disque


PREMIÈRE UTILISATION
====================

Au démarrage, l'application :

1. Détecte automatiquement votre périphérique audio actif
2. Tente de reconnaître le modèle (Mode B) ou utilise le Mode A par défaut
3. Affiche en temps réel le niveau sonore dB(A) avec code couleur :
   - VERT   : < 70 dB(A) (niveau sûr)
   - ORANGE : 70-80 dB(A) (exposition modérée, à limiter)
   - ROUGE  : > 80 dB(A) (exposition dangereuse, réduire immédiatement)

4. Jouer de l'audio (musique, vidéo, jeux) pour voir les valeurs s'afficher


FONCTIONNALITÉS
===============

- Estimation temps réel du niveau dB(A) au casque
- Graphe historique 3 minutes
- Calcul Leq (niveau équivalent continu sur 1 minute)
- Détection automatique des casques populaires (Sony, Bose, AirPods, etc.)
- Calibration optionnelle avec sonomètre
- Export CSV de l'historique
- Dark mode
- 100% offline (aucune connexion réseau)


MODES D'ESTIMATION
==================

MODE A : Zero-Input Conservateur (par défaut)
----------------------------------------------
- Affichage : dB(A) relatif (pas de SPL absolu)
- Activation : Toujours disponible, périphérique non reconnu
- Biais de sécurité : -5 dB (sur-estimation)
- Cas d'usage : Indication relative sans calibration

MODE B : Auto-profil Heuristique
---------------------------------
- Affichage : SPL estimé en dB(A) absolu
- Activation : Périphérique reconnu (patterns JSON)
- Précision : ±5-8 dB typique
- Cas d'usage : Casques populaires détectés automatiquement
- Avertissement : "Estimation heuristique, marge ±6 dB"


LIMITES IMPORTANTES
===================

⚠ CETTE APPLICATION EST UN OUTIL INDICATIF, PAS UN DISPOSITIF MÉDICAL CERTIFIÉ.

Ce que l'application MESURE :
✓ Le signal numérique envoyé au périphérique (dBFS)
✓ L'estimation du SPL basée sur profils heuristiques (Mode B)
✓ Le niveau équivalent continu (Leq) sur 1 minute

Ce que l'application NE MESURE PAS :
✗ La pression acoustique RÉELLE au conduit auditif
✗ Votre audition personnelle (seuil, sensibilité)
✗ Les fuites ou le fit du casque (±10 dB d'impact)
✗ Le volume système Windows (API non accessible)
✗ Les égaliseurs externes (Dolby, SoundBlaster, etc.)

Variables non contrôlées :
- Volume système   : ±20 dB d'impact
- Fit du casque    : ±10 dB (étanchéité, coussinets usés)
- EQ externes      : ±6 dB
- Impédance sortie : ±3 dB (carte son / DAC)

⚠ En cas de symptômes auditifs (acouphènes, perte auditive, douleur),
  consultez un professionnel ORL immédiatement.


RECOMMANDATIONS OMS
===================

Durée d'exposition maximale selon le niveau :

- < 85 dB(A)  : 8 heures / jour (sûr)
- 85-90 dB(A) : 2-4 heures / jour
- 90-95 dB(A) : 30 min - 1 heure / jour
- > 95 dB(A)  : < 15 minutes (éviter)


CALIBRATION OPTIONNELLE
=======================

Pour améliorer la précision (±2-3 dB) :

1. Matériel requis :
   - Sonomètre de référence (IEC 61672 classe 2 minimum)
   - Coupleur acoustique ou mesure in-situ

2. Procédure :
   - Jouer un signal de test (bruit rose, musique)
   - Mesurer le SPL réel avec le sonomètre
   - Dans l'application : "Calibration" → entrer valeur mesurée
   - L'application ajuste la constante C automatiquement

3. Limitations :
   - ⚠ Valide uniquement pour ce périphérique + volume système
   - ⚠ Changer le volume ou le casque invalide la calibration


FICHIERS DE L'APPLICATION
=========================

ApplAudition.exe    : Exécutable principal (self-contained)
README.txt          : Ce fichier

Logs (créés au runtime) :
%LOCALAPPDATA%\ApplAudition\logs\app-YYYYMMDD.log

Settings (créés au runtime) :
%LOCALAPPDATA%\ApplAudition\settings.json


DÉSINSTALLATION
===============

Supprimer simplement le dossier contenant ApplAudition.exe.

Optionnel : Supprimer les données utilisateur dans :
%LOCALAPPDATA%\ApplAudition\


SUPPORT
=======

- Documentation complète : Voir README.md (GitHub)
- Issues / Bugs         : https://github.com/votreRepo/ApplAudition/issues
- Discussions           : https://github.com/votreRepo/ApplAudition/discussions


FAQ RAPIDE
==========

Q : L'application affiche 0.0 dB(A), pourquoi ?
R : Assurez-vous qu'un audio est en cours de lecture (musique, vidéo).
    Vérifiez que le périphérique audio est actif dans Windows.

Q : Pourquoi le Mode B n'est pas activé ?
R : Votre casque n'est pas dans la base de profils embarquée.
    L'application reste en Mode A (zero-input conservateur).

Q : L'application consomme trop de CPU ?
R : Vérifiez qu'aucune autre application audio intensive ne tourne.
    Consultez les logs dans %LOCALAPPDATA%\ApplAudition\logs

Q : Puis-je utiliser l'application avec des haut-parleurs ?
R : Oui, mais l'estimation SPL sera très imprécise (distance, acoustique).
    L'application est conçue pour les casques/écouteurs.

Q : L'application fonctionne-t-elle hors ligne ?
R : Oui, 100% offline. Aucune connexion réseau, aucune donnée envoyée.


LICENSE
=======

MIT License - Copyright (c) 2025 Appli Audition Contributors

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


====================================
⚠ AVERTISSEMENT FINAL

Cette application est un outil indicatif, pas un dispositif médical certifié.

Protégez votre audition. Elle est irremplaçable.
====================================

Dernière mise à jour : 2025-10-08
Version : 1.0.0
