using UnityEditor;

namespace KavehRising.Editor
{
    public sealed class HumanoidAssetImporter : AssetPostprocessor
    {
        void OnPreprocessModel()
        {
            if (assetPath.Contains("/Art/Characters/"))
            {
                var importer = (ModelImporter)assetImporter;
                importer.importAnimation = false;
                importer.animationType = ModelImporterAnimationType.Generic;
                importer.optimizeGameObjects = false;
                importer.importBlendShapes = true;
                importer.importCameras = false;
                importer.importLights = false;
            }
            else if (assetPath.Contains("/Art/Animations/"))
            {
                var importer = (ModelImporter)assetImporter;
                importer.importAnimation = true;
                importer.animationType = ModelImporterAnimationType.Legacy;
                importer.optimizeGameObjects = false;
                importer.importCameras = false;
                importer.importLights = false;
            }
        }
    }
}