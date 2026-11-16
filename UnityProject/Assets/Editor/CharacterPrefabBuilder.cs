using System.Linq;
using UnityEditor;
using UnityEngine;

namespace KavehRising.Editor
{
    public static class CharacterPrefabBuilder
    {
        const string HeroModelPath = "Assets/Art/Characters/Barbarian.fbx";
        const string BossModelPath = "Assets/Art/Characters/Mage.fbx";
        const string HeroTexturePath = "Assets/Art/Characters/barbarian_texture.png";
        const string BossTexturePath = "Assets/Art/Characters/mage_texture.png";
        static readonly string[] AnimationPaths =
        {
            "Assets/Resources/Animations/Rig_Medium_MovementBasic.glb",
            "Assets/Resources/Animations/Rig_Medium_MovementAdvanced.glb",
            "Assets/Resources/Animations/Rig_Medium_General.glb",
            "Assets/Resources/Animations/Rig_Medium_CombatMelee.glb",
            "Assets/Resources/Animations/Rig_Medium_Special.glb"
        };

        [MenuItem("Kaveh Rising/Build Rigged Character Prefabs")]
        public static void BuildPrefabs()
        {
            ConfigureModel(HeroModelPath);
            ConfigureModel(BossModelPath);
            int clipCount = ImportAndCountAnimations();
            var hero = AssetDatabase.LoadAssetAtPath<GameObject>(HeroModelPath);
            var boss = AssetDatabase.LoadAssetAtPath<GameObject>(BossModelPath);
            if (!hero || !boss) throw new System.Exception("KayKit Barbarian/Mage model FBX is missing.");
            var heroMat = MakeMaterial("Kaveh - Forged Leather", AssetDatabase.LoadAssetAtPath<Texture2D>(HeroTexturePath), new Color(1f,.9f,.78f));
            var bossMat = MakeMaterial("Dahhak - Cursed Jade", AssetDatabase.LoadAssetAtPath<Texture2D>(BossTexturePath), new Color(.72f,.88f,.78f));
            CreatePrefab(hero, heroMat, "KavehHumanoid", true);
            CreatePrefab(boss, bossMat, "DahhakHumanoid", false);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("KAVEH RISING: two rigged medium characters prepared with " + clipCount + " imported animation clips.");
        }
        static void ConfigureModel(string path)
        {
            var importer = AssetImporter.GetAtPath(path) as ModelImporter;
            if (!importer) throw new System.Exception("Model importer missing: " + path);
            importer.importAnimation = false;
            importer.animationType = ModelImporterAnimationType.Generic;
            importer.optimizeGameObjects = false;
            importer.importBlendShapes = true;
            importer.importCameras = false;
            importer.importLights = false;
            importer.SaveAndReimport();
        }
        static int ImportAndCountAnimations()
        {
            AssetDatabase.Refresh();
            int total = 0;
            foreach (var path in AnimationPaths)
            {
                AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);
                var clips = AssetDatabase.LoadAllAssetsAtPath(path).OfType<AnimationClip>().ToArray();
                if (clips.Length == 0) throw new System.Exception("glTFast imported no animation clips from " + path);
                total += clips.Length;
            }
            if (total < 20) throw new System.Exception("Animation import incomplete: only " + total + " clips found.");
            return total;
        }
        static Material MakeMaterial(string name, Texture2D texture, Color tint)
        {
            var shader = Shader.Find("Standard");
            if (!shader) shader = Shader.Find("Universal Render Pipeline/Lit");
            var material = new Material(shader) { name = name, color = tint };
            if (texture) material.mainTexture = texture;
            material.SetFloat("_Metallic", .18f); material.SetFloat("_Glossiness", .32f);
            string path = "Assets/Art/Materials/" + name.Replace(" ", "_") + ".mat";
            var old = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (old) { old.CopyPropertiesFromMaterial(material); Object.DestroyImmediate(material); return old; }
            AssetDatabase.CreateAsset(material, path); return material;
        }
        static void CreatePrefab(GameObject source, Material material, string prefabName, bool hero)
        {
            var instance = PrefabUtility.InstantiatePrefab(source) as GameObject;
            if (!instance) throw new System.Exception("Could not instantiate rigged model: " + source.name);
            instance.name = prefabName; instance.transform.localScale = Vector3.one;
            foreach (var renderer in instance.GetComponentsInChildren<Renderer>(true))
            {
                var mats = renderer.sharedMaterials;
                for (int i = 0; i < mats.Length; i++) mats[i] = material;
                renderer.sharedMaterials = mats;
            }
            var animation = instance.GetComponent<Animation>();
            if (!animation) animation = instance.AddComponent<Animation>();
            animation.playAutomatically = false; animation.wrapMode = WrapMode.Loop;
            if (!instance.GetComponent<RiggedCharacterAnimation>()) instance.AddComponent<RiggedCharacterAnimation>();
            AddAccessories(instance.transform, hero);
            PrefabUtility.SaveAsPrefabAsset(instance, "Assets/Resources/Characters/" + prefabName + ".prefab");
            Object.DestroyImmediate(instance);
        }
        static void AddAccessories(Transform root, bool hero)
        {
            Transform hand = FindBone(root, "hand_r", "righthand", "right_hand");
            Transform head = FindBone(root, "head");
            if (hero && hand)
            {
                var handle = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                handle.name = "Kaveh forged hammer handle"; handle.transform.SetParent(hand, false);
                handle.transform.localPosition = new Vector3(0,.04f,.1f);
                handle.transform.localRotation = Quaternion.Euler(90,0,0);
                handle.transform.localScale = new Vector3(.045f,.32f,.045f); RemoveCollider(handle);
                var hammerHead = GameObject.CreatePrimitive(PrimitiveType.Cube);
                hammerHead.name = "Kaveh forged hammer head"; hammerHead.transform.SetParent(handle.transform,false);
                hammerHead.transform.localPosition = new Vector3(0,.95f,0); hammerHead.transform.localScale = new Vector3(3.8f,.42f,1.25f);
                var iron = new Material(Shader.Find("Standard")); iron.color = new Color(.25f,.28f,.3f); iron.SetFloat("_Metallic",.8f);
                hammerHead.GetComponent<Renderer>().sharedMaterial = iron; RemoveCollider(hammerHead);
            }
            if (!hero && head)
            {
                var bronze = new Material(Shader.Find("Standard")); bronze.color = new Color(.12f,.25f,.18f); bronze.SetFloat("_Metallic",.65f);
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
        static Transform FindBone(Transform root,params string[] names)
        {
            foreach(var t in root.GetComponentsInChildren<Transform>(true))
                foreach(var name in names)
                    if(t.name.Replace(" ","").Replace(".","").Replace(":","").ToLowerInvariant()==name.Replace("_","").ToLowerInvariant()) return t;
            return null;
        }
        static void RemoveCollider(GameObject go){var collider=go.GetComponent<Collider>();if(collider)Object.DestroyImmediate(collider);}
    }
}