# KAVEH RISING | قیام کاوه

A lightweight, Persian-first Android action game inspired by the legend of Kaveh the Blacksmith and Zahhak.

## Complete game loop

- Six story chapters, from the forge to the raising of the Kaviani banner.
- Chapter-specific environments, opponents, enemy health, and short Persian story introductions.
- Five touch controls: move left/right, dodge, normal attack, and heavy attack.
- Timed enemy turns, telegraphed attacks, special strikes in later chapters, and temporary dodge invulnerability.
- Three player health points, one healing potion per attempt, chapter rewards, score, and a saved high score.
- Local save/resume for chapter progress and combat state.
- Native Android Canvas graphics; no large art/audio bundles and no extra runtime libraries.

## Project quality

Combat calculations live in a small pure-Kotlin rules module with JVM unit tests. GitHub Actions runs the unit tests and builds a Debug APK on pushes to `main`, pull requests, and manual workflow dispatch.

## Build locally

Open the project in Android Studio with Android SDK 35 and JDK 17. The project uses Android Gradle Plugin 8.7.3 and Kotlin 2.0.21. Run `testDebugUnitTest` and `assembleDebug` to test and build.

## Downloading a build

After the GitHub Actions workflow succeeds, open the run under the repository's Actions tab and download the `KAVEH-RISING-debug-apk` artifact.
