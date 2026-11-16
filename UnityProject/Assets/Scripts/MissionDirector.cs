using UnityEngine;
namespace KavehRising
{
    public sealed class MissionDirector : MonoBehaviour
    {
        public static MissionDirector Instance { get; private set; }
        static readonly string[][] Tasks =
        {
            new[]{"روشن کردن آتشدان آهنگری"},
            new[]{"آزاد کردن اسیر مردم"},
            new[]{"برداشتن آذوقه کاروان"},
            new[]{"آزاد کردن زندانی اول","آزاد کردن زندانی دوم","آزاد کردن زندانی سوم"},
            new[]{"همراه کردن مردم","همراه کردن پیشه‌وران"},
            new[]{"برافراشتن درفش کاویانی"},
            new[]{"شکستن زنجیر اسیر اول","شکستن زنجیر اسیر دوم","شکستن زنجیر اسیر سوم"},
            new[]{"شکستن بت مار اول","شکستن بت مار دوم"},
            new[]{"روشن کردن آتش راهنما"},
            new[]{"از کار انداختن نشان مار اول","از کار انداختن نشان مار دوم","از کار انداختن نشان مار سوم"}
        };
        int chapter, done;
        Transform targetsRoot;
        Material bronze, stone, ember, cloth;
        GameObject civilianPrefab;
        public bool IsComplete => done >= Tasks[Mathf.Clamp(chapter,0,Tasks.Length-1)].Length;
        public string CurrentTask => IsComplete ? "هدف فرعی کامل شد" : Tasks[Mathf.Clamp(chapter,0,Tasks.Length-1)][Mathf.Clamp(done,0,Tasks[Mathf.Clamp(chapter,0,Tasks.Length-1)].Length-1)];

        void Awake()
        {
            if(Instance && Instance!=this){Destroy(this);return;}
            Instance=this;
            bronze=MakeMaterial(new Color(.52f,.34f,.14f),.65f);
            stone=MakeMaterial(new Color(.25f,.26f,.28f),.12f);
            ember=MakeMaterial(new Color(.95f,.24f,.035f),.1f);
            cloth=MakeMaterial(new Color(.55f,.055f,.04f),.08f);
            civilianPrefab=Resources.Load<GameObject>("Characters/KavehHumanoid");
        }
        Material MakeMaterial(Color color,float metallic)
        {
            var shader=Shader.Find("Standard");if(!shader)shader=Shader.Find("Universal Render Pipeline/Lit");
            var material=new Material(shader);material.color=color;material.SetFloat("_Metallic",metallic);return material;
        }
        public void BeginChapter(int index)
        {
            chapter=Mathf.Clamp(index,0,Tasks.Length-1);done=0;
            if(targetsRoot)Destroy(targetsRoot.gameObject);
            targetsRoot=new GameObject("Chapter Mission Objectives").transform;
            var labels=Tasks[chapter];
            for(int i=0;i<labels.Length;i++)
            {
                float x=labels.Length==1?0f:-7f+i*(14f/(labels.Length-1));
                float z=chapter==3||chapter==6?4f:chapter==1||chapter==4?5f:chapter==5||chapter==8?5.5f:chapter==7||chapter==9?4.5f:2.5f;
                CreateTarget(labels[i],new Vector3(x,0,z),i);
            }
        }
        void CreateTarget(string label,Vector3 position,int index)
        {
            var target=new GameObject("Objective - "+label);target.transform.SetParent(targetsRoot);target.transform.position=position;
            var trigger=target.AddComponent<SphereCollider>();trigger.isTrigger=true;trigger.radius=1.35f;
            if(chapter==1||chapter==3||chapter==4||chapter==6)
                CreatePerson(target.transform,chapter==3||chapter==6);
            else
                CreateProp(target.transform,index);
            target.AddComponent<ObjectiveTarget>().Init(label);
        }
        void CreatePerson(Transform parent,bool prisoner)
        {
            if(civilianPrefab)
            {
                var person=Instantiate(civilianPrefab,parent);person.name=prisoner?"Freed prisoner":"Iranian villager";
                person.transform.localPosition=Vector3.zero;person.transform.localRotation=Quaternion.identity;person.transform.localScale=Vector3.one*.78f;
                var rig=person.GetComponent<RiggedCharacterAnimation>();if(rig)rig.SetLocomotion(0,prisoner);
            }
            else
            {
                Part(parent,"Villager body",PrimitiveType.Capsule,new Vector3(0,1,0),new Vector3(.55f,.8f,.4f),cloth);
                Part(parent,"Villager head",PrimitiveType.Sphere,new Vector3(0,2,0),new Vector3(.38f,.38f,.38f),bronze);
            }
            if(prisoner)
            {
                Part(parent,"Prisoner wrist chain",PrimitiveType.Cylinder,new Vector3(-.55f,1.15f,.05f),new Vector3(.045f,.35f,.045f),bronze,Quaternion.Euler(0,0,70));
                Part(parent,"Prisoner wrist chain",PrimitiveType.Cylinder,new Vector3(.55f,1.15f,.05f),new Vector3(.045f,.35f,.045f),bronze,Quaternion.Euler(0,0,-70));
            }
        }
        void CreateProp(Transform parent,int index)
        {
            switch(chapter)
            {
                case 0:
                    Part(parent,"Forge stone base",PrimitiveType.Cylinder,new Vector3(0,.35f,0),new Vector3(1.2f,.35f,1.2f),stone);
                    Part(parent,"Forge flame",PrimitiveType.Sphere,new Vector3(0,1.25f,0),new Vector3(.48f,.85f,.48f),ember);
                    Part(parent,"Forge iron rim",PrimitiveType.Cylinder,new Vector3(0,.72f,0),new Vector3(.85f,.12f,.85f),bronze);
                    break;
                case 2:
                    Part(parent,"Supply crate",PrimitiveType.Cube,new Vector3(0,.65f,0),new Vector3(1.25f,1.1f,1.1f),stone);
                    Part(parent,"Crate iron band",PrimitiveType.Cube,new Vector3(0,.65f,-.56f),new Vector3(.12f,1.12f,.05f),bronze);
                    break;
                case 5:
                    Part(parent,"Banner pole",PrimitiveType.Cylinder,new Vector3(0,2f,0),new Vector3(.09f,2f,.09f),bronze);
                    Part(parent,"Derafsh Kaviani cloth",PrimitiveType.Cube,new Vector3(.85f,2.6f,0),new Vector3(1.7f,1.1f,.08f),cloth);
                    Part(parent,"Banner golden mark",PrimitiveType.Sphere,new Vector3(.85f,2.6f,-.08f),new Vector3(.28f,.28f,.08f),bronze);
                    break;
                case 7:
                case 9:
                    Part(parent,"Serpent idol base",PrimitiveType.Cube,new Vector3(0,.35f,0),new Vector3(1.4f,.7f,1.1f),stone);
                    Part(parent,"Serpent idol body",PrimitiveType.Cylinder,new Vector3(0,1.35f,0),new Vector3(.38f,.85f,.38f),bronze);
                    Part(parent,"Serpent idol head",PrimitiveType.Sphere,new Vector3(0,2.25f,0),new Vector3(.5f,.42f,.45f),ember);
                    Part(parent,"Serpent horn left",PrimitiveType.Cylinder,new Vector3(-.32f,2.55f,0),new Vector3(.08f,.38f,.08f),bronze,Quaternion.Euler(0,0,25));
                    Part(parent,"Serpent horn right",PrimitiveType.Cylinder,new Vector3(.32f,2.55f,0),new Vector3(.08f,.38f,.08f),bronze,Quaternion.Euler(0,0,-25));
                    break;
                case 8:
                    Part(parent,"Alborz beacon base",PrimitiveType.Cylinder,new Vector3(0,.5f,0),new Vector3(.8f,.5f,.8f),stone);
                    Part(parent,"Alborz beacon flame",PrimitiveType.Sphere,new Vector3(0,1.45f,0),new Vector3(.4f,.7f,.4f),ember);
                    var lightObject=new GameObject("Beacon glow");lightObject.transform.SetParent(parent,false);lightObject.transform.localPosition=new Vector3(0,1.4f,0);
                    var light=lightObject.AddComponent<Light>();light.type=LightType.Point;light.color=new Color(1f,.42f,.12f);light.range=9;light.intensity=2.2f;
                    break;
                default:
                    Part(parent,"Mission relic",PrimitiveType.Cube,new Vector3(0,.8f,0),new Vector3(1f,1.4f,.6f),bronze);
                    break;
            }
        }
        void Part(Transform parent,string name,PrimitiveType type,Vector3 position,Vector3 scale,Material material,Quaternion rotation=default)
        {
            var part=GameObject.CreatePrimitive(type);part.name=name;part.transform.SetParent(parent,false);
            part.transform.localPosition=position;part.transform.localScale=scale;part.transform.localRotation=rotation==default?Quaternion.identity:rotation;
            var renderer=part.GetComponent<Renderer>();if(renderer)renderer.sharedMaterial=material;
            var collider=part.GetComponent<Collider>();if(collider)Destroy(collider);
        }
        public bool TryInteract(Transform actor)
        {
            if(IsComplete||!targetsRoot)return false;
            ObjectiveTarget candidate=null;float nearest=2.8f;
            foreach(var t in targetsRoot.GetComponentsInChildren<ObjectiveTarget>())
            {
                float distance=Vector3.Distance(actor.position,t.transform.position);
                if(!t.Done&&distance<nearest){nearest=distance;candidate=t;}
            }
            if(!candidate)return false;
            string label=candidate.Label;candidate.Complete();done++;
            var audio=FindObjectOfType<GameAudio>();
            if(audio){if(chapter==0)audio.PlayForge();else if(chapter==3)audio.PlayDoor();else audio.PlayObjective();}
            if(GameRoot.Instance){GameRoot.Instance.SetNotice(label+" — انجام شد");GameRoot.Instance.CheckChapterCompletion();}
            return true;
        }
        void OnDestroy(){if(Instance==this)Instance=null;}
    }
    public sealed class ObjectiveTarget:MonoBehaviour
    {
        public string Label{get;private set;} public bool Done{get;private set;}
        public void Init(string label){Label=label;}
        public void Complete()
        {
            if(Done)return;Done=true;
            foreach(var renderer in GetComponentsInChildren<Renderer>(true))renderer.material.color=new Color(.18f,.65f,.28f);
            foreach(var collider in GetComponentsInChildren<Collider>(true))collider.enabled=false;
            Destroy(gameObject,1.2f);
        }
    }
}