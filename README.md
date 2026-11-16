# KAVEH RISING | قیام کاوه

**Engine:** Unity 2022.3.62f1 — Android  
**Primary project:** `UnityProject/`

## Project contents
The Unity project contains the 3D Android game, rigged KayKit character assets, imported animation packs, ten story chapters, staged enemy encounters, touch controls, Persian HUD/story text, mission interactions, stealth and CC0 sound effects. The soundtrack and Persian voice acting are intentionally deferred.

## Build an Android APK
1. Open `UnityProject/` with Unity **2022.3.62f1** and install Android Build Support (SDK/NDK and OpenJDK).
2. In the Unity menu, run `Kaveh Rising > Build Android APK`.
3. The APK is written to `UnityProject/Build/KavehRising.apk`.

The same build is available through GitHub Actions: open **Actions > Unity Android APK > Run workflow**. The workflow builds the APK and publishes it as the `KAVEH-RISING-Android-APK` artifact for 14 days.

## GitHub Actions license setup
Before the cloud build can run, add a valid Unity license XML as the repository Actions secret named `UNITY_LICENSE`. If required by the selected Unity license/activation method, also configure `UNITY_EMAIL` and `UNITY_PASSWORD`. Never commit license files or credentials to the repository.

## Validation status
The repository includes a non-destructive preflight validator at `Kaveh Rising > Validate Project Before Device Build`. It checks required source assets, generated character prefabs, imported animation clips, enabled build scenes and Android support.

**Release status:** no successful Unity Android build or physical-device test has yet been verified from this environment. A successful GitHub Actions run and installation/gameplay test on a real Android device are still required before calling this a tested release.
