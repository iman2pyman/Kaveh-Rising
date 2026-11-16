using System;
using System.Linq;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace KavehRising.Editor
{
    /// <summary>
    /// Non-destructive release preflight. Reports missing project assets and
    /// Android configuration before a developer starts a device build.
    /// This tool never starts or uploads an APK.
    /// </summary>
    public static class ProjectPreflightValidator
    {
        static readonly string[] RequiredAssets =
        {
            "Assets/Art/Characters/Barbarian.fbx",
            "Assets/Art/Characters/Mage.fbx",
            "Assets/Resources/Characters/KavehHumanoid.prefab",
            "Assets/Resources/Characters/DahhakHumanoid.prefab",
            "Assets/Resources/Animations/Rig_Medium_MovementBasic.glb",
            "Assets/Resources/Animations/Rig_Medium_MovementAdvanced.glb",
            "Assets/Resources/Animations/Rig_Medium_General.glb",
            "Assets/Resources/Animations/Rig_Medium_CombatMelee.glb",
            "Assets/Resources/Animations/Rig_Medium_Special.glb"
        };

        [MenuItem("Kaveh Rising/Validate Project Before Device Build")]
        public static void Validate()
        {
            int errors = 0;
            int warnings = 0;

            foreach (string path in RequiredAssets)
            {
                if (AssetDatabase.LoadMainAssetAtPath(path) == null)
                {
                    Debug.LogError("[KAVEH PREFLIGHT] Missing required asset: " + path);
                    errors++;
                }
            }

            string[] clips = AssetDatabase.FindAssets("t:AnimationClip", new[] { "Assets/Resources/Animations/Imported" });
            if (clips.Length < 20)
            {
                Debug.LogError("[KAVEH PREFLIGHT] Expected at least 20 imported animation clips; found " + clips.Length + ". Run 'Kaveh Rising/Build Rigged Character Prefabs' first.");
                errors++;
            }
            else
            {
                Debug.Log("[KAVEH PREFLIGHT] Imported animation clips: " + clips.Length);
            }

            var scenes = EditorBuildSettings.scenes.Where(scene => scene.enabled).ToArray();
            if (scenes.Length == 0)
            {
                Debug.LogError("[KAVEH PREFLIGHT] No enabled scenes are listed in Build Settings.");
                errors++;
            }
            else
            {
                foreach (var scene in scenes)
                {
                    if (AssetDatabase.LoadAssetAtPath<SceneAsset>(scene.path) == null)
                    {
                        Debug.LogError("[KAVEH PREFLIGHT] Build scene is missing: " + scene.path);
                        errors++;
                    }
                }
                Debug.Log("[KAVEH PREFLIGHT] Enabled build scenes: " + scenes.Length);
            }

            if (!BuildPipeline.IsBuildTargetSupported(BuildTargetGroup.Android, BuildTarget.Android))
            {
                Debug.LogError("[KAVEH PREFLIGHT] Android Build Support is not installed in this Unity Editor.");
                errors++;
            }

            if (string.IsNullOrWhiteSpace(PlayerSettings.applicationIdentifier))
            {
                Debug.LogError("[KAVEH PREFLIGHT] Android application identifier is empty.");
                errors++;
            }

            if (PlayerSettings.defaultInterfaceOrientation != UIOrientation.LandscapeLeft &&
                PlayerSettings.defaultInterfaceOrientation != UIOrientation.LandscapeRight &&
                PlayerSettings.defaultInterfaceOrientation != UIOrientation.AutoRotation)
            {
                Debug.LogWarning("[KAVEH PREFLIGHT] Orientation is not configured for landscape play.");
                warnings++;
            }

            Debug.Log("[KAVEH PREFLIGHT] Result: " + errors + " error(s), " + warnings + " warning(s). This check does not build an APK.");
            if (errors == 0)
                Debug.Log("[KAVEH PREFLIGHT] Static preflight passed. A real Android build and device test are still required before release.");
        }
    }
}
