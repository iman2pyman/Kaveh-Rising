# KAVEH RISING | قیام کاوه

**Current engine:** Godot 4.7.2 — Android  
**Active project:** `GodotProject/`  
**Backup project:** `UnityProject/` (preserved; not deleted)

## Current development
The Godot project is the active lightweight migration. Its first playable milestone includes a procedural 3D arena, Kaveh, a Zahhak guard with simple chase behavior, melee combat, health bars, Persian HUD, and touch controls. The original Unity assets and project remain in the repository as a backup while migration continues.

## Build an Android debug APK
1. Open **Actions** in this repository.
2. Select **Godot Android APK**.
3. Choose **Run workflow** on the `main` branch.
4. When the run succeeds, download the `KAVEH-RISING-Android-debug` artifact from that run.

The workflow downloads the official Godot 4.7.2 editor and matching export templates, installs Android build dependencies, validates the project, exports a debug APK, and uploads it for 14 days. No Godot account or license secret is required.

## Controls in the first prototype
- Android touch: directional buttons and «ضربه».
- Desktop test: WASD / arrow keys to move, Space to attack.

## Story direction
The game follows Kaveh's uprising against Zahhak. The planned story retains ten chapters and seventeen enemy waves, including the forge, the oppressed people, the mountain pass, Zahhak's prison, the prisoners' rescue, the people's uprising, and the Kaviani banner. Music and Persian voice acting remain deferred for the user to provide.

## Validation status
The Godot source and Android workflow have been added. An APK is **not yet verified**: the GitHub Actions workflow must complete successfully, and the resulting build still needs installation and gameplay testing on a real Android device. A signed release build is a later publishing step.
