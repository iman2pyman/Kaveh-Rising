using System.Linq;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;
namespace KavehRising.Editor
{
 public static class HumanoidPrefabBuilder
 {
  const string AnimationPath="Assets/Art/Animations/Rig_Large_MovementBasic.fbx";
  const string ControllerPath="Assets/Resources/Controllers/KavehHumanoid.controller";
  [MenuItem("Kaveh Rising/Build Humanoid Prefabs")]
  public static void BuildAll()
  {
   var clips=AssetDatabase.LoadAllAssetsAtPath(AnimationPath).OfType<AnimationClip>().Where(c=>c&&!c.name.StartsWith("__preview__",System.StringComparison.OrdinalIgnoreCase)).ToArray();
   if(clips.Length==0)throw new System.Exception("No imported humanoid animation clips found: "+AnimationPath);
   var controller=BuildController(clips);
   BuildPrefab("Assets/Art/Characters/Superhero_Male_FullBody.fbx","Assets/Resources/Characters/KavehHero.prefab",controller);
   BuildPrefab("Assets/Art/Characters/Barbarian_Large.fbx","Assets/Resources/Characters/DahhakWarrior.prefab",controller);
   AssetDatabase.SaveAssets();AssetDatabase.Refresh();
   Debug.Log("KAVEH RISING: built two rigged prefabs using "+clips.Length+" animation clips.");
  }
  static AnimationClip Pick(AnimationClip[] clips,params string[] words)
  {
   foreach(var word in words){var c=clips.FirstOrDefault(x=>x.name.IndexOf(word,System.StringComparison.OrdinalIgnoreCase)>=0);if(c)return c;}
   return clips[0];
  }
  static AnimatorController BuildController(AnimationClip[] clips)
  {
   if(AssetDatabase.LoadAssetAtPath<AnimatorController>(ControllerPath))AssetDatabase.DeleteAsset(ControllerPath);
   System.IO.Directory.CreateDirectory("Assets/Resources/Controllers");
   var controller=AnimatorController.CreateAnimatorControllerAtPath(ControllerPath);
   controller.AddParameter("Speed",AnimatorControllerParameterType.Float);
   controller.AddParameter("Crouch",AnimatorControllerParameterType.Bool);
   foreach(var p in new[]{"Attack","HeavyAttack","Dodge","Hit","Die"})controller.AddParameter(p,AnimatorControllerParameterType.Trigger);
   controller.AddParameter("Grounded",AnimatorControllerParameterType.Bool);
   var machine=controller.layers[0].stateMachine;
   var tree=new BlendTree{name="Locomotion",blendType=BlendTreeType.Simple1D,blendParameter="Speed",useAutomaticThresholds=false};
   tree.AddChild(Pick(clips,"Idle","Stand"),0f);tree.AddChild(Pick(clips,"Walk","Jog"),.45f);tree.AddChild(Pick(clips,"Run","Sprint"),1f);
   AssetDatabase.AddObjectToAsset(tree,controller);
   var locomotion=machine.AddState("Locomotion");locomotion.motion=tree;machine.defaultState=locomotion;
   var crouch=machine.AddState("Crouch");crouch.motion=Pick(clips,"Crouch");
   var t=locomotion.AddTransition(crouch);t.hasExitTime=false;t.duration=.12f;t.AddCondition(AnimatorConditionMode.If,0,"Crouch");
   t=crouch.AddTransition(locomotion);t.hasExitTime=false;t.duration=.12f;t.AddCondition(AnimatorConditionMode.IfNot,0,"Crouch");
   Action(machine,locomotion,"Attack",Pick(clips,"Attack","Punch","Swing"));
   Action(machine,locomotion,"HeavyAttack",Pick(clips,"Heavy","Power","Attack"));
   Action(machine,locomotion,"Dodge",Pick(clips,"Dodge","Roll","Evade"));
   Action(machine,locomotion,"Hit",Pick(clips,"Hit","Hurt","Damage"));
   Action(machine,locomotion,"Die",Pick(clips,"Death","Die","Fall"));
   return controller;
  }
  static void Action(AnimatorStateMachine machine,AnimatorState home,string parameter,AnimationClip clip)
  {
   var state=machine.AddState(parameter);state.motion=clip;
   var any=machine.AddAnyStateTransition(state);any.hasExitTime=false;any.duration=.05f;any.canTransitionToSelf=false;any.AddCondition(AnimatorConditionMode.If,0,parameter);
   if(parameter=="Die")return;
   var back=state.AddTransition(home);back.hasExitTime=true;back.exitTime=.92f;back.duration=.12f;
  }
  static void BuildPrefab(string modelPath,string prefabPath,AnimatorController controller)
  {
   var source=AssetDatabase.LoadAssetAtPath<GameObject>(modelPath);if(!source)throw new System.Exception("Missing character FBX: "+modelPath);
   var instance=PrefabUtility.InstantiatePrefab(source) as GameObject;if(!instance)instance=Object.Instantiate(source);
   instance.name=System.IO.Path.GetFileNameWithoutExtension(prefabPath);
   var animator=instance.GetComponent<Animator>();if(!animator)animator=instance.AddComponent<Animator>();
   var avatar=AssetDatabase.LoadAllAssetsAtPath(modelPath).OfType<Avatar>().FirstOrDefault();if(avatar)animator.avatar=avatar;
   animator.runtimeAnimatorController=controller;animator.applyRootMotion=false;
   if(!instance.GetComponent<HumanoidAnimationDriver>())instance.AddComponent<HumanoidAnimationDriver>();
   System.IO.Directory.CreateDirectory(System.IO.Path.GetDirectoryName(prefabPath));
   PrefabUtility.SaveAsPrefabAsset(instance,prefabPath);Object.DestroyImmediate(instance);
  }
 }
}