using UnityEngine;

namespace KavehRising
{
    public static class RuntimeWorld
    {
        static Transform environmentRoot;
        static Material Mat(Color c,float metallic=0f)
        {
            var shader=Shader.Find("Standard"); if(shader==null) shader=Shader.Find("Universal Render Pipeline/Lit");
            var m=new Material(shader); m.color=c; m.SetFloat("_Metallic",metallic); return m;
        }
        static GameObject Cube(string name,Vector3 pos,Vector3 scale,Material mat)
        {
            var o=GameObject.CreatePrimitive(PrimitiveType.Cube); o.name=name;
            o.transform.position=pos; o.transform.localScale=scale; o.GetComponent<Renderer>().material=mat;
            o.transform.SetParent(environmentRoot,true); return o;
        }

        static void Boundary(string name,Vector3 position,Vector3 size)
        {
            var wall=new GameObject(name); wall.transform.SetParent(environmentRoot,true); wall.transform.position=position;
            var box=wall.AddComponent<BoxCollider>(); box.size=size;
        }
        static void Pillar(float x,float z,Material stone,Material trim)
        {
            Cube("Carved column",new Vector3(x,1.7f,z),new Vector3(.85f,3.4f,.85f),stone);
            Cube("Column capital",new Vector3(x,3.45f,z),new Vector3(1.25f,.25f,1.25f),trim);
            Cube("Column base",new Vector3(x,.15f,z),new Vector3(1.2f,.3f,1.2f),trim);
        }
        public static void BuildEnvironment(ChapterData chapter)
        {
            if(environmentRoot!=null) Object.Destroy(environmentRoot.gameObject);
            environmentRoot=new GameObject("Chapter Environment").transform;
            RenderSettings.skybox=null; RenderSettings.fog=true; RenderSettings.fogColor=chapter.sky; RenderSettings.fogDensity=.012f;
            if(Camera.main==null) throw new System.Exception("Main Camera was not created by Bootstrap.");
            Camera.main.backgroundColor=chapter.sky; RenderSettings.ambientLight=new Color(.48f,.43f,.38f);
            var ground=GameObject.CreatePrimitive(PrimitiveType.Plane); ground.name="Ancient Persian ground";
            ground.transform.localScale=new Vector3(4,1,3); ground.GetComponent<Renderer>().material=Mat(chapter.ground);
            ground.transform.SetParent(environmentRoot,true);
            Boundary("Arena north boundary",new Vector3(0,2,14.5f),new Vector3(40,4,.5f));
            Boundary("Arena south boundary",new Vector3(0,2,-14.5f),new Vector3(40,4,.5f));
            Boundary("Arena west boundary",new Vector3(-19.5f,2,0),new Vector3(.5f,4,30));
            Boundary("Arena east boundary",new Vector3(19.5f,2,0),new Vector3(.5f,4,30));
            var stone=Mat(new Color(.34f,.27f,.22f)); var dark=Mat(new Color(.13f,.12f,.13f));
            var gold=Mat(new Color(.62f,.38f,.12f),.65f); var ember=Mat(new Color(.8f,.22f,.035f),.15f);
            Cube("Ancient stone pathway",new Vector3(0,.06f,0),new Vector3(24,.12f,3.2f),stone);
            for(int i=0;i<7;i++){Pillar(-12+i*4,8,stone,gold); Pillar(-12+i*4,-9,dark,stone);}
            Cube("Far fortress wall",new Vector3(0,2.1f,12),new Vector3(32,4.2f,1),dark);
            Cube("Broken foreground wall",new Vector3(0,1.1f,-11),new Vector3(28,2.2f,.8f),stone);
            // Chapter-specific landmarks, silhouettes and forge lights.
            switch(GameRoot.Instance!=null?GameRoot.Instance.Chapter:0)
            {
                case 1:
                    for(int i=0;i<4;i++){Cube("Oppressed villagers shelter",new Vector3(-11+i*7,.8f,5),new Vector3(2.2f,1.6f,2.4f),stone);Cube("Shelter roof",new Vector3(-11+i*7,1.8f,5),new Vector3(2.8f,.25f,2.8f),dark);}
                    break;
                case 0:
                    Cube("Forge hearth",new Vector3(-7,.7f,3),new Vector3(2.8f,1.4f,2),dark);
                    for(int i=0;i<3;i++) Cube("Glowing forge coal",new Vector3(-7+i*.55f,1.48f,3),new Vector3(.4f,.16f,.5f),ember);
                    break;
                case 2:
                case 8:
                    for(int i=0;i<6;i++){Cube("Mountain ridge left",new Vector3(-16+i*2,3+i%2,13),new Vector3(3,6+i%3,4),dark);Cube("Mountain ridge right",new Vector3(16-i*2,3+(i+1)%2,13),new Vector3(3,6+(i+1)%3,4),dark);}
                    break;
                case 3:
                case 6:
                    for(int i=0;i<5;i++) Cube("Dungeon cell",new Vector3(-10+i*5,1.8f,5),new Vector3(.25f,3.6f,2.5f),gold);
                    break;
                case 5:
                    Cube("Banner pole",new Vector3(0,3.2f,4),new Vector3(.18f,6.4f,.18f),gold);
                    Cube("Derafsh Kaviani cloth",new Vector3(1.25f,4.7f,4),new Vector3(2.5f,1.6f,.08f),ember);
                    break;
                case 6:
                    for(int i=0;i<4;i++){var chain=GameObject.CreatePrimitive(PrimitiveType.Cylinder);chain.name="Dungeon chains";chain.transform.SetParent(environmentRoot,true);chain.transform.position=new Vector3(-9+i*6,1.6f,4);chain.transform.localScale=new Vector3(.08f,1.5f,.08f);chain.GetComponent<Renderer>().material=gold;}
                    break;
                case 8:
                    Cube("Alborz beacon base",new Vector3(0,.7f,5),new Vector3(2,1.4f,2),stone);
                    var beaconLight=new GameObject("Alborz beacon light");beaconLight.transform.SetParent(environmentRoot,true);beaconLight.transform.position=new Vector3(0,2.5f,5);
                    var beacon=beaconLight.AddComponent<Light>();beacon.type=LightType.Point;beacon.color=new Color(1f,.48f,.12f);beacon.range=14;beacon.intensity=3.2f;
                    break;
                case 7:
                case 9:
                    for(int i=0;i<4;i++){var serpent=GameObject.CreatePrimitive(PrimitiveType.Sphere);serpent.name="Serpent idol";serpent.transform.SetParent(environmentRoot,true);serpent.transform.position=new Vector3(-9+i*6,2,7);serpent.transform.localScale=new Vector3(1.1f,3.5f,.7f);serpent.GetComponent<Renderer>().material=Mat(new Color(.12f,.28f,.18f),.45f);}
                    break;
            }
            for(int i=0;i<10;i++)
            {
                float x=Mathf.Sin(i*2.17f)*13f, z=Mathf.Cos(i*1.73f)*9f;
                Cube("Broken masonry",new Vector3(x,.2f,z),new Vector3(.55f+(i%3)*.2f,.4f,.65f),stone);
            }
            var lightObject=new GameObject("Warm directional light"); lightObject.transform.SetParent(environmentRoot,true);
            var light=lightObject.AddComponent<Light>(); light.type=LightType.Directional; light.intensity=1.25f;
            light.transform.rotation=Quaternion.Euler(48,-32,0);
            var fire=new GameObject("Forge glow"); fire.transform.SetParent(environmentRoot,true); fire.transform.position=new Vector3(-7,2,3);
            var glow=fire.AddComponent<Light>(); glow.type=LightType.Point; glow.color=new Color(1f,.32f,.08f); glow.range=9; glow.intensity=2.1f;
            var cam=Camera.main; cam.transform.position=new Vector3(0,7,-12); cam.transform.rotation=Quaternion.Euler(22,0,0);
            var follow=cam.GetComponent<FollowCamera>(); if(follow==null) follow=cam.gameObject.AddComponent<FollowCamera>(); follow.Target=null;
        }
        public static void SpawnActors(GameRoot root)
        {
            var prefab=Resources.Load<GameObject>("Characters/KavehHumanoid");
            var hero=new GameObject("Kaveh - Blacksmith Rebel"); hero.transform.position=prefab?new Vector3(-3,0,0):new Vector3(-3,1,0);
            var cc=hero.AddComponent<CharacterController>(); cc.height=1.9f; cc.radius=.38f; cc.center=new Vector3(0,.95f,0);
            if(prefab)
            {
                var model=Object.Instantiate(prefab,hero.transform); model.name="Kaveh rigged humanoid"; model.transform.localPosition=Vector3.zero; model.transform.localRotation=Quaternion.identity; model.transform.localScale=Vector3.one;
                hero.AddComponent<Animation>(); hero.AddComponent<RiggedCharacterAnimation>();
            }
            else { hero.AddComponent<PlayerMotor>(); var fallback=hero.AddComponent<CharacterVisuals>(); fallback.IsHero=true; }
            if(!hero.GetComponent<PlayerMotor>())hero.AddComponent<PlayerMotor>();
            var foe=new GameObject(root.Current.enemyName); foe.transform.position=prefab?new Vector3(3,0,0):new Vector3(3,1,0);
            if(prefab)
            {
                var model=Object.Instantiate(prefab,foe.transform); model.name="Enemy rigged humanoid"; model.transform.localPosition=Vector3.zero; model.transform.localRotation=Quaternion.identity; model.transform.localScale=Vector3.one*(root.Chapter==9?1.22f:1.08f);
                foreach(var renderer in model.GetComponentsInChildren<Renderer>()) { renderer.material=new Material(renderer.material); renderer.material.color=root.Chapter==9?new Color(.48f,.12f,.16f):new Color(.32f,.38f,.34f); }
                foe.AddComponent<Animation>(); foe.AddComponent<RiggedCharacterAnimation>();
            }
            var enemyCollider=foe.AddComponent<CapsuleCollider>(); enemyCollider.height=2.2f; enemyCollider.radius=.42f; enemyCollider.center=new Vector3(0,1.05f,0);
            var brain=foe.AddComponent<EnemyBrain>();
            if(!prefab){foe.transform.localScale=new Vector3(1.15f,1.25f,1.15f);var fallback=foe.AddComponent<CharacterVisuals>();fallback.IsBoss=true;}
            root.RegisterActors(hero.GetComponent<PlayerMotor>(),brain);
        }
    }
}
