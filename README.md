# TER MIASHS : Effet du Guidage en Dessin Spatial AR

Ce dépôt contient l'application d'AR développée dans le cadre de notre TER de L3 MIASHS.

## 🎯 Objectif de l'expérience
L'objectif de notre recherche est de mesurer et comparer l'effet de deux types de guidages sur l'apprentissage et la précision d'un geste en 3D (Spatial Drawing) :
- **Guidage Concomitant :** Feedback visuel en temps réel pendant le tracé.
- **Guidage Terminal :** Feedback visuel et score de précision affichés après l'exécution du tracé.

L'application permet d'enregistrer les coordonnées spatiales (X, Y, Z) du téléphone lors du tracé pour une analyse statistique ultérieure.

---

## 🛠️ Guide d'installation pour l'équipe (Important)

Pour que le projet compile correctement sur des téléphones Android récents (ex: Android 13+, Samsung S23+), **vous devez respecter ces versions** lors de l'ouverture du projet dans Unity :

1. **Unity Version :** `2019.4.41f2 LTS`
2. **Modules requis (via Unity Hub) :** `Android Build Support`, `OpenJDK`, et `Android SDK & NDK Tools`.
3. **Configuration du Build (Player Settings) :**
   - Minimum API Level : `Android 7.0 (API 24)`
   - Graphics APIs : Supprimer Vulkan (ne laisser que `OpenGLES3`).
   - Scripting Backend : `IL2CPP`.
   - Target Architectures : Cocher uniquement `ARM64` (décocher ARMv7).
4. **Mise à jour ARCore :** Dans le *Package Manager*, assurez-vous de mettre à jour `ARCore XR Plugin` et `AR Foundation`.
5. **Gradle :** Utilisez une version externe de Gradle (`5.6.4`) via *Preferences > External Tools*.

---
---

# 📚 Original Project Base : ARDraw by Dilmer Valecillos

*This project is built upon the excellent open-source foundation provided by Dilmer Valecillos.*

AR Drawing demos with AR Foundation, ARCore XR Plugin, and ARKit XR Plugin.
Results from various examples taught in YouTube:

***If you missed the YouTube video(s) about this project make sure to watch the first one <a href="https://www.youtube.com/watch?v=kcqcUxVQu0o">here</a> & subscribe as it would help me in bringing more open source projects THANK YOU !***

|Features||
|---|---|
|Drawing experience 1 </br><img src="https://github.com/dilmerv/ARDraw/blob/master/docs/images/demo_1.gif" width="300">|Drawing experience 2 </br><img src="https://github.com/dilmerv/ARDraw/blob/master/docs/images/demo_2.gif" width="300">|
|Reticle feature </br><img src="https://github.com/dilmerv/ARDraw/blob/master/docs/images/demo_3.gif" width="300">|Multi-Touch drawing support </br><img src="https://github.com/dilmerv/ARDraw/blob/master/docs/images/demo_4.gif" width="300">|