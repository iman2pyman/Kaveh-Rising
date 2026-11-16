using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace KavehRising.Editor
{
    public static class CharacterPrefabBuilder
    {
        const string ModelPath = "Assets/Art/Characters/Barbarian_Large.fbx";
        const string TexturePath = "Assets/Art/Characters/barbarian_texture.png";
        static readonly string[] AnimationSources =
        {
            "Assets/Art/Animations/Rig_Large_MovementBasic.fbx"
        };
        const string ResourceRoot = "Assets/Resources";

        [MenuItem("Kaveh Rising/Build Rigged Character Prefabs")]
        public static void BuildPrefabs()
        {
            EnsureFolders();
            ConfigureModel();
            ConfigureAnimations();
            AssetDatabase.Refresh();
            var model = AssetDatabase.LoadAssetAtPath<GameObject>(ModelPath);
            if (!model) throw new System.Exception("Missing KayKit Barbarian_Large FBX model.");
            var texture = AssetDatabase.LoadAssetAtPath<Texture2D>(TexturePath);
            var heroMaterial = MakeMaterial("Kaveh - Leather and Iron", texture, new Color(1f,.82f,.66f));
            var bossMaterial = MakeMaterial("Dahhak - Cursed Iron", texture, new Color(.43f,.58f,.5f));
            CreatePrefab(model, heroMaterial, "KavehHumanoid", true);
            CreatePrefab(model, bossMaterial, "DahhakHumanoid", false);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("KAVEH RISING: rigged character prefabs and imported animation clips prepared.");
        }

        static void EnsureFolders()
        {
            Directory.CreateDirectory("Assets/Resources/Characters");
            Directory.CreateDirectory("Assets/Resources/Animations");
            Directory.CreateDirectory("Assets/Art/Materials");
        }
        static void ConfigureModel()
        {
            var importer = AssetImporter.GetAtPath(ModelPath) as ModelImporter;
            if (!importer) throw new System.Exception("Character FBX importer not found.");
            importer.importAnimation = false;
            importer.animationType = ModelImporterAnimationType.Generic;
            importer.optimizeGameObjects = false;
            importer.importBlendShapes = true;
            importer.importCameras = false;
            importer.importLights = false;
            importer.SaveAndReimport();
        }
        static void ConfigureAnimations()
        {
            foreach (var source in AnimationSources)
            {
                if (!File.Exists(source)) { Debug.LogWarning("Optional animation set not present: " + source); continue; }
                string destination = ResourceRoot + "/Animations/" + Path.GetFileName(source);
                if (!File.Exists(destination)) AssetDatabase.CopyAsset(source, destination);
                var importer = AssetImporter.GetAtPath(destination) as ModelImporter;
                if (!importer) continue;
                importer.importAnimation = true;
                importer.animationType = ModelImporterAnimationType.Legacy;
                importer.optimizeGameObjects = false;
                importer.importCameras = false;
                importer.importLights = false;
                importer.animationCompression = ModelImporterAnimationCompression.Off;
                importer.SaveAndReimport();
            }
            ImportGltfAnimationClips();
        }
        static void ImportGltfAnimationClips()
        {
            Directory.CreateDirectory(ResourceRoot + "/Animations/Imported");
            AssetDatabase.Refresh();
            var files=Directory.GetFiles(ResourceRoot + "/Animations","*.glb");
            foreach(var file in files)
            {
                string assetPath=file.Replace("\\","/");
                foreach(var clip in AssetDatabase.LoadAllAssetsAtPath(assetPath).OfType<AnimationClip>())
                {
                    if(!clip || clip.name.IndexOf("T-Pose",System.StringComparison.OrdinalIgnoreCase)>=0)continue;
                    string output=ResourceRoot + "/Animations/Imported/" + clip.name + ".anim";
                    if(AssetDatabase.LoadAssetAtPath<AnimationClip>(output))continue;
                    var copy=Object.Instantiate(clip);copy.name=clip.name;copy.legacy=true;copy.wrapMode=WrapMode.Once;
                    AssetDatabase.CreateAsset(copy,output);
                }
            }
        }
        static Material MakeMaterial(string name, Texture2D texture, Color tint)
        {
            var shader = Shader.Find("Standard");
            if (!shader) shader = Shader.Find("Universal Render Pipeline/Lit");
            var material = new Material(shader) { name = name, color = tint };
            if (texture) material.mainTexture = texture;
            material.SetFloat("_Metallic", .12f);
            material.SetFloat("_Glossiness", .28f);
            string path = "Assets/Art/Materials/" + name.Replace(" ", "_") + ".mat";
            var old = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (old) { old.CopyPropertiesFromMaterial(material); Object.DestroyImmediate(material); return old; }
            AssetDatabase.CreateAsset(material, path);
            return material;
        }
        static void CreatePrefab(GameObject source, Material material, string prefabName, bool hero)
        {
            var instance = PrefabUtility.InstantiatePrefab(source) as GameObject;
            if (!instance) throw new System.Exception("Could not instantiate rigged FBX: " + source.name);
            instance.name = prefabName;
            instance.transform.localScale = Vector3.one;
            foreach (var renderer in instance.GetComponentsInChildren<Renderer>(true))
            {
                var mats = renderer.sharedMaterials;
                for (int i = 0; i < mats.Length; i++) mats[i] = material;
                renderer.sharedMaterials = mats;
            }
            var animation = instance.GetComponent<Animation>();
            if (!animation) animation = instance.AddComponent<Animation>();
            animation.playAutomatically = false;
            animation.wrapMode = WrapMode.Loop;
            if (!instance.GetComponent<RiggedCharacterAnimation>()) instance.AddComponent<RiggedCharacterAnimation>();
            AddAccessories(instance.transform, hero);
            string path = "Assets/Resources/Characters/" + prefabName + ".prefab";
            PrefabUtility.SaveAsPrefabAsset(instance, path);
            Object.DestroyImmediate(instance);
        }
        static void AddAccessories(Transform root, bool hero)
        {
            Transform hand = FindBone(root, "hand_r");
            Transform head = FindBone(root, "head");
            if (hero && hand)
            {
                var handle = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                handle.name = "Kaveh forged hammer handle"; handle.transform.SetParent(hand, false);
                handle.transform.localPosition = new Vector3(0,.04f,.1f);
                handle.transform.localRotation = Quaternion.Euler(90,0,0);
                handle.transform.localScale = new Vector3(.045f,.32f,.045f);
                RemoveCollider(handle);
                var headPiece = GameObject.CreatePrimitive(PrimitiveType.Cube);
                headPiece.name = "Kaveh forged hammer head"; headPiece.transform.SetParent(handle.transform,false);
                headPiece.transform.localPosition = new Vector3(0,.95f,0);
                headPiece.transform.localScale = new Vector3(3.8f,.42f,1.25f);
                var iron = new Material(Shader.Find("Standard")); iron.color = new Color(.25f,.28f,.3f); iron.SetFloat("_Metallic",.75f);
                headPiece.GetComponent<Renderer>().sharedMaterial = iron; RemoveCollider(headPiece);
            }
            if (!hero && head)
            {
                var bronze = new Material(Shader.Find("Standard")); bronze.color = new Color(.18f,.27f,.2f); bronze.SetFloat("_Metallic",.6f);
                for(int side=-1;side<=1;side+=2)
                {
                    var horn=GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                    horn.name="Dahhak serpent crown horn";horn.transform.SetParent(head,false);
                    horn.transform.localPosition=new Vector3(side*.19f,.2f,0);
                    horn.transform.localRotation=Quaternion.Euler(0,0,-side*28);
                    horn.transform.localScale=new Vector3(.07f,.32f,.07f);
                    horn.GetComponent<Renderer>().sharedMaterial=bronze;RemoveCollider(horn);
                }
            }
        }
        static Transform FindBone(Transform root,string name)
        {
            foreach(var t in root.GetComponentsInChildren<Transform>(true))
                if(t.name.ToLowerInvariant()==name) return t;
            return null;
        }
        static void RemoveCollider(GameObject go)
        {
            var collider=go.GetComponent<Collider>(); if(collider)Object.DestroyImmediate(collider);
        }
    }
}