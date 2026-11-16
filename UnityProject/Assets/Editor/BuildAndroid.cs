using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.IO;

namespace KavehRising.Editor
{
    public static class BuildAndroid
    {
        [MenuItem("Kaveh Rising/Build Android APK")]
        public static void Build()
        {
            HumanoidPrefabBuilder.BuildAll();
            Directory.CreateDirectory("Assets/Scenes");
            var scene=EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Single);
            var go=new GameObject("Bootstrap");
            go.AddComponent<Bootstrap>();
            EditorSceneManager.SaveScene(scene,"Assets/Scenes/Main.unity");
            PlayerSettings.companyName="Kaveh Rising Studio";
            PlayerSettings.productName="KAVEH RISING";
            PlayerSettings.SetApplicationIdentifier(BuildTargetGroup.Android,"com.kavehrising.game");
            PlayerSettings.defaultInterfaceOrientation=UIOrientation.LandscapeLeft;
            PlayerSettings.Android.minSdkVersion=AndroidSdkVersions.AndroidApiLevel23;
            PlayerSettings.Android.targetSdkVersion=AndroidSdkVersions.AndroidApiLevel35;
            PlayerSettings.bundleVersion="1.0.0";
            PlayerSettings.Android.bundleVersionCode=1;
            EditorBuildSettings.scenes=new[]{new EditorBuildSettingsScene("Assets/Scenes/Main.unity",true)};
            var report=BuildPipeline.BuildPlayer(new BuildPlayerOptions{
                scenes=new[]{"Assets/Scenes/Main.unity"},
                locationPathName="Build/KavehRising.apk",
                target=BuildTarget.Android,
                options=BuildOptions.None
            });
            if(report.summary.result!=UnityEditor.Build.Reporting.BuildResult.Succeeded)
                throw new System.Exception("Android build failed: "+report.summary.result);
            Debug.Log("KAVEH RISING APK built: "+report.summary.totalSize+" bytes");
        }
    }
}
