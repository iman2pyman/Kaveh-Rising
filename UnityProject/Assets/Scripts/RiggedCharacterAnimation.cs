using UnityEngine;

namespace KavehRising
{
    [RequireComponent(typeof(Animation))]
    public sealed class RiggedCharacterAnimation : MonoBehaviour
    {
        Animation animationPlayer;
        Transform upperArmL, upperArmR, thighL, thighR, spine, calfL, calfR;
        string locomotion = "Idle";
        bool dead;
        void Awake()
        {
            animationPlayer = GetComponent<Animation>();
            upperArmL = FindBone("upperarm_l","leftarm","leftupperarm"); upperArmR = FindBone("upperarm_r","rightarm","rightupperarm");
            thighL = FindBone("thigh_l","leftupleg","leftupperleg"); thighR = FindBone("thigh_r","rightupleg","rightupperleg");
            calfL = FindBone("calf_l","leftleg","leftlowerleg"); calfR = FindBone("calf_r","rightleg","rightlowerleg");
            spine = FindBone("spine_02","spine2","chest","spine");
            LoadImportedAnimations();
            BuildClip("Idle", true, new Motion(spine,0,new[]{0f,1f,0f}));
            BuildClip("Walk", true, new Motion(thighL,0,new[]{-25f,25f,-25f}),new Motion(thighR,0,new[]{25f,-25f,25f}),new Motion(upperArmL,0,new[]{22f,-22f,22f}),new Motion(upperArmR,0,new[]{-22f,22f,-22f}));
            BuildClip("Run", true, new Motion(thighL,0,new[]{-42f,42f,-42f}),new Motion(thighR,0,new[]{42f,-42f,42f}),new Motion(upperArmL,0,new[]{35f,-35f,35f}),new Motion(upperArmR,0,new[]{-35f,35f,-35f}));
            BuildClip("Crouch", true, new Motion(spine,0,new[]{12f,14f,12f}),new Motion(calfL,0,new[]{-28f,-32f,-28f}),new Motion(calfR,0,new[]{-28f,-32f,-28f}));
            BuildClip("Attack", false, new Motion(upperArmR,0,new[]{0f,-105f,28f,0f}),new Motion(spine,1,new[]{0f,-12f,4f,0f}));
            BuildClip("HeavyAttack", false, new Motion(upperArmR,0,new[]{0f,-145f,38f,0f}),new Motion(spine,1,new[]{0f,-22f,8f,0f}));
            BuildClip("Dodge", false, new Motion(spine,2,new[]{0f,24f,-10f,0f}),new Motion(upperArmL,0,new[]{0f,-55f,0f}));
            BuildClip("Hit", false, new Motion(spine,0,new[]{0f,28f,-8f,0f}),new Motion(upperArmR,0,new[]{0f,35f,0f}));
            BuildClip("Death", false, new Motion(spine,0,new[]{0f,45f,85f,90f}),new Motion(thighL,0,new[]{0f,-20f,-55f,-70f}),new Motion(thighR,0,new[]{0f,20f,55f,70f}));
            Play("Idle", .05f);
        }
        void LoadImportedAnimations()
        {
            var clips=Resources.LoadAll<AnimationClip>("Animations").Concat(Resources.LoadAll<AnimationClip>("Animations/Imported")).ToArray();
            foreach(var clip in clips)
            {
                if(!clip || !clip.legacy || animationPlayer.GetClip(clip.name)!=null)continue;
                animationPlayer.AddClip(clip,clip.name);
            }
            RegisterExternal("Idle", "idle", "stand");
            RegisterExternal("Walk", "walk");
            RegisterExternal("Run", "run", "jog");
            RegisterExternal("Crouch", "crouch", "sneak");
            RegisterExternal("Dodge", "Dodge_Forward", "Dodge_Left", "dodge", "roll", "evade");
            RegisterExternal("Hit", "hit", "hurt", "damage");
            RegisterExternal("Death", "death", "die");
            RegisterExternal("Attack", "Melee_1H_Slash", "Melee_1H_Stab", "attack", "melee", "slash", "swing");
            RegisterExternal("HeavyAttack", "Melee_2H_Slam", "Melee_2H_Attack", "heavy", "combo", "strong");
        }
        void RegisterExternal(string state,params string[] words)
        {
            foreach(AnimationState s in animationPlayer)
            {
                string n=s.name.ToLowerInvariant();
                foreach(string word in words) if(n.Contains(word))
                {
                    external[state]=s.name;
                    s.wrapMode=(state=="Idle"||state=="Walk"||state=="Run"||state=="Crouch")?WrapMode.Loop:WrapMode.Once;
                    return;
                }
            }
        }
        readonly System.Collections.Generic.Dictionary<string,string> external=new System.Collections.Generic.Dictionary<string,string>();
        struct Motion
        {
            public Transform bone; public int axis; public float[] values;
            public Motion(Transform b,int a,float[] v){bone=b;axis=a;values=v;}
        }
        Transform FindBone(params string[] tokens)
        {
            foreach(var t in GetComponentsInChildren<Transform>(true))
                foreach(var token in tokens)
                    if(t.name.Replace(" ","").Replace(".","").Replace(":","").ToLowerInvariant()==token.Replace("_","").ToLowerInvariant())return t;
            return null;
        }
        string Path(Transform target)
        {
            if(!target)return null;string path=target.name;var p=target.parent;
            while(p&&p!=transform){path=p.name+"/"+path;p=p.parent;}
            return p==transform?path:null;
        }
        void BuildClip(string clipName,bool loop,params Motion[] motions)
        {
            if(external.ContainsKey(clipName))return;
            var clip=new AnimationClip();clip.name=clipName;clip.legacy=true;clip.frameRate=30f;clip.wrapMode=loop?WrapMode.Loop:WrapMode.Once;
            foreach(var motion in motions)
            {
                string path=Path(motion.bone);if(string.IsNullOrEmpty(path))continue;
                string axis=motion.axis==0?"x":motion.axis==1?"y":"z";
                var keys=new Keyframe[motion.values.Length];
                for(int i=0;i<keys.Length;i++)keys[i]=new Keyframe(i*.18f,motion.values[i]);
                clip.SetCurve(path,typeof(Transform),"localEulerAnglesRaw."+axis,new AnimationCurve(keys));
            }
            animationPlayer.AddClip(clip,clipName);
        }
        void Play(string state,float fade=.12f)
        {
            if(!animationPlayer||dead&&state!="Death")return;
            string clip=external.ContainsKey(state)?external[state]:state;
            if(animationPlayer[clip]==null)return;
            animationPlayer.CrossFade(clip,fade);locomotion=state;
        }
        public void SetLocomotion(float speed,bool crouching)
        {
            if(dead)return;string state=crouching?"Crouch":speed>.68f?"Run":speed>.08f?"Walk":"Idle";
            if(state!=locomotion)Play(state);
        }
        public void PlayAttack(bool heavy){if(!dead)Play(heavy?"HeavyAttack":"Attack",.04f);}
        public void PlayDodge(){if(!dead)Play("Dodge",.03f);}
        public void PlayHit(){if(!dead)Play("Hit",.04f);}
        public void PlayDeath(){if(dead)return;dead=true;Play("Death",.08f);}
    }
}