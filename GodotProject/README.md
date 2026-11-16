# KAVEH RISING — Godot edition

This is the new lightweight Godot 4.7.2 project. The original Unity project remains intact in `../UnityProject` as a backup.

## First playable milestone
- Procedural low-poly 3D arena (no external downloads or heavyweight starter assets).
- Kaveh player capsule and a Zahhak guard enemy.
- Third-person follow camera, basic chase AI, health and melee attack.
- Persian HUD and on-screen touch buttons; keyboard controls are included for desktop testing.
- First chapter placeholder: «آتش آهنگری».

## Android
The repository workflow **Godot Android APK** downloads the official Godot 4.7.2 editor and matching export templates, installs Android SDK packages, validates the project, exports a debug APK and uploads it as a workflow artifact.

No Godot account or license secret is needed. A debug APK is for testing; a separately signed release build and real-device QA are still required before publishing.

## Controls
- Touch: directional buttons and «ضربه».
- Keyboard: WASD / arrow keys to move, Space to attack.
