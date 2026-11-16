using UnityEditor;
using UnityEngine;

namespace KavehRising.Editor
{
    // Import defaults for externally sourced humanoid FBX assets placed under Assets/Art/Characters or Animations.
    public sealed class HumanoidAssetImporter : AssetPostprocessor
    {
        void OnPreprocessModel()
        {
            if (!(assetPath.Contains("/Art/Characters/") || assetPath.Contains("/Art/Animations/"))) return;
            var importer = (ModelImporter)assetImporter;
            importer.importAnimation = true;
            importer.animationType = ModelImporterAnimationType.Human;
            importer.avatarSetup = ModelImporterAvatarSetup.CreateFromThisModel;
            importer.optimizeGameObjects = false;
            importer.importBlendShapes = true;
            importer.importCameras = false;
            importer.importLights = false;
        }
    }
}
