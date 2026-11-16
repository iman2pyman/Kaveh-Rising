using UnityEngine;
namespace KavehRising
{
    // Runtime keyframed animation for the imported CC0 skinned humanoid. Uses the rig's real bone transforms.
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
            upperArmL = FindBone("upperarm_l"); upperArmR = FindBone("upperarm_r");
            thighL = FindBone("thigh_l"); thighR = FindBone("thigh_r");
            calfL = FindBone("calf_l"); calfR = FindBone("calf_r");
            spine = FindBone("spine_02");
            BuildClip("Idle", true, new Motion(spine, 0, new[]{0f,1f,0f}));
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
        struct Motion
        {
            public Transform bone; public int axis; public float[] values;
            public Motion(Transform b,int a,float[] v){bone=b;axis=a;values=v;}
        }
        Transform FindBone(string token)
        {
            foreach(var t in GetComponentsInChildren<Transform>(true))
                if(t.name.ToLowerInvariant()==token) return t;
            return null;
        }
        string Path(Transform target)
        {
            if(!target)return null;string path=target.name;var p=target.parent;
            while(p&&p!=transform){path=p.name+"/"+path;p=p.parent;}return p==transform?path:null;
        }
        void BuildClip(string clipName,bool loop,params Motion[] motions)
        {
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
            if(!animationPlayer||dead&&state!="Death"||animationPlayer[state]==null)return;
            animationPlayer.CrossFade(state,fade);locomotion=state;
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