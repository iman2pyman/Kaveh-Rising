using System;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEditor.Build.Reporting;

namespace KavehRising.Editor
{
    /// <summary>
    /// Single Android build entry point for both the Unity menu and CI.
    /// Creates the bootstrap scene, prepares character prefabs, validates
    /// Android settings, and fails the batch process when the build fails.
    /// </summary>
    public static class BuildAndroid
    {
        const string ScenePath = "Assets/Scenes/Main.unity";
        const string DefaultOutputPath = "Build/KavehRising.apk";

        [MenuItem("Kaveh Rising/Build Android APK")]
        public static void BuildFromMenu()
        {
            BuildToPath(DefaultOutputPath);
        }

        // Configure GitHub Actions / Unity batch mode with:
        // -executeMethod KavehRising.Editor.BuildAndroid.BuildFromCommandLine
        public static void BuildFromCommandLine()
        {
            string outputPath = GetArgument("-buildOutput");
            if (string.IsNullOrWhiteSpace(outputPath))
                outputPath = DefaultOutputPath;

            try
            {
                BuildToPath(outputPath);
                EditorApplication.Exit(0);
            }
            catch (Exception exception)
            {
                Debug.LogException(exception);
                EditorApplication.Exit(1);
                throw;
            }
        }

        static string GetArgument(string key)
        {
            string[] args = Environment.GetCommandLineArgs();
            for (int i = 0; i < args.Length - 1; i++)
                if (string.Equals(args[i], key, StringComparison.OrdinalIgnoreCase))
                    return args[i + 1];
            return null;
        }

        static void BuildToPath(string outputPath)
        {
            if (!BuildPipeline.IsBuildTargetSupported(BuildTargetGroup.Android, BuildTarget.Android))
                throw new InvalidOperationException("Android Build Support is not installed in this Unity Editor.");

            EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTargetGroup.Android, BuildTarget.Android);

            // Generate real rigged character prefabs and imported animation clips.
            CharacterPrefabBuilder.BuildPrefabs();

            Directory.CreateDirectory("Assets/Scenes");
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            var bootstrap = new GameObject("Bootstrap");
            bootstrap.AddComponent<Bootstrap>();
            EditorSceneManager.SaveScene(scene, ScenePath);

            PlayerSettings.companyName = "Kaveh Rising Studio";
            PlayerSettings.productName = "KAVEH RISING";
            PlayerSettings.SetApplicationIdentifier(BuildTargetGroup.Android, "com.kavehrising.game");
            PlayerSettings.defaultInterfaceOrientation = UIOrientation.LandscapeLeft;
            PlayerSettings.Android.minSdkVersion = AndroidSdkVersions.AndroidApiLevel23;
            PlayerSettings.Android.targetSdkVersion = AndroidSdkVersions.AndroidApiLevel35;
            PlayerSettings.bundleVersion = "1.0.0";
            PlayerSettings.Android.bundleVersionCode = 1;
            EditorBuildSettings.scenes = new[] { new EditorBuildSettingsScene(ScenePath, true) };

            // Do not produce an APK from an incomplete or misconfigured project.
            ProjectPreflightValidator.ValidateOrThrow();

            string fullOutputPath = Path.GetFullPath(outputPath);
            Directory.CreateDirectory(Path.GetDirectoryName(fullOutputPath));

            var options = new BuildPlayerOptions
            {
                scenes = new[] { ScenePath },
                locationPathName = fullOutputPath,
                target = BuildTarget.Android,
                options = BuildOptions.None
            };

            Debug.Log("[KAVEH BUILD] Starting Android APK build: " + fullOutputPath);
            BuildReport report = BuildPipeline.BuildPlayer(options);
            if (report.summary.result != BuildResult.Succeeded)
                throw new BuildFailedException("Android build failed: " + report.summary.result +
                    " | errors: " + report.summary.totalErrors +
                    " | warnings: " + report.summary.totalWarnings);

            if (!File.Exists(fullOutputPath))
                throw new BuildFailedException("Unity reported success but the APK file was not found: " + fullOutputPath);

            Debug.Log("[KAVEH BUILD] SUCCESS | APK bytes: " + report.summary.totalSize +
                " | output: " + fullOutputPath);
        }
    }
}
