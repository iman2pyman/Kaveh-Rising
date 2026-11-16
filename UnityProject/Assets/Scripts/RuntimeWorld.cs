using UnityEngine;

namespace KavehRising
{
    public static class RuntimeWorld
    {
        static Transform environmentRoot;
        static Material Mat(Color color,float metallic=0f)
        {
            var shader=Shader.Find("Standard");
            if(shader==null)shader=Shader.Find("Universal Render Pipeline/Lit");
            var material=new Material(shader);material.color=color;material.SetFloat("_Metallic",metallic);return material;
        }
        static GameObject Cube(string name,Vector3 position,Vector3 scale,Material material)
        {
            var item=GameObject.CreatePrimitive(PrimitiveType.Cube);item.name=name;
            item.transform.position=position;item.transform.localScale=scale;
            item.GetComponent<Renderer>().sharedMaterial=material;item.transform.SetParent(environmentRoot,true);return item;
        }
        static void Boundary(string name,Vector3 position,Vector3 size)
        {
            var wall=new GameObject(name);wall.transform.SetParent(environmentRoot,true);wall.transform.position=position;
            wall.AddComponent<BoxCollider>().size=size;
        }
        static void Pillar(float x,float z,Material stone,Material trim)
        {
            Cube("Carved column",new Vector3(x,1.7f,z),new Vector3(.85f,3.4f,.85f),stone);
            Cube("Column capital",new Vector3(x,3.45f,z),new Vector3(1.25f,.25f,1.25f),trim);
            Cube("Column base",new Vector3(x,.15f,z),new Vector3(1.2f,.3f,1.2f),trim);
        }
        public static void BuildEnvironment(ChapterData chapter)
        {
            if(environmentRoot)Object.Destroy(environmentRoot.gameObject);
            environmentRoot=new GameObject("Chapter Environment").transform;
            RenderSettings.skybox=null;RenderSettings.fog=true;RenderSettings.fogColor=chapter.sky;RenderSettings.fogDensity=.012f;
            if(Camera.main==null)throw new System.Exception("Main Camera was not created by Bootstrap.");
            Camera.main.backgroundColor=chapter.sky;RenderSettings.ambientLight=new Color(.48f,.43f,.38f);
            var ground=GameObject.CreatePrimitive(PrimitiveType.Plane);ground.name="Ancient Persian ground";
            ground.transform.localScale=new Vector3(4,1,3);ground.GetComponent<Renderer>().sharedMaterial=Mat(chapter.ground);ground.transform.SetParent(environmentRoot,true);
            Boundary("Arena north boundary",new Vector3(0,2,14.5f),new Vector3(40,4,.5f));
            Boundary("Arena south boundary",new Vector3(0,2,-14.5f),new Vector3(40,4,.5f));
            Boundary("Arena west boundary",new Vector3(-19.5f,2,0),new Vector3(.5f,4,30));
            Boundary("Arena east boundary",new Vector3(19.5f,2,0),new Vector3(.5f,4,30));
            var stone=Mat(new Color(.34f,.27f,.22f));var dark=Mat(new Color(.13f,.12f,.13f));
            var gold=Mat(new Color(.62f,.38f,.12f),.65f);var ember=Mat(new Color(.8f,.22f,.035f),.15f);
            Cube("Ancient stone pathway",new Vector3(0,.06f,0),new Vector3(24,.12f,3.2f),stone);
            for(int i=0;i<7;i++){Pillar(-12+i*4,8,stone,gold);Pillar(-12+i*4,-9,dark,stone);}
            Cube("Far fortress wall",new Vector3(0,2.1f,12),new Vector3(32,4.2f,1),dark);
            Cube("Broken foreground wall",new Vector3(0,1.1f,-11),new Vector3(28,2.2f,.8f),stone);
            switch(GameRoot.Instance!=null?GameRoot.Instance.Chapter:0)
            {
                case 0:
                    Cube("Forge hearth",new Vector3(-7,.7f,3),new Vector3(2.8f,1.4f,2),dark);
                    for(int i=0;i<3;i++)Cube("Glowing forge coal",new Vector3(-7+i*.55f,1.48f,3),new Vector3(.4f,.16f,.5f),ember);
                    break;
                case 1:
                    for(int i=0;i<4;i++){Cube("Oppressed villagers shelter",new Vector3(-11+i*7,.8f,5),new Vector3(2.2f,1.6f,2.4f),stone);Cube("Shelter roof",new Vector3(-11+i*7,1.8f,5),new Vector3(2.8f,.25f,2.8f),dark);}
                    break;
                case 2:
                case 8:
                    for(int i=0;i<6;i++){Cube("Mountain ridge left",new Vector3(-16+i*2,3+i%2,13),new Vector3(3,6+i%3,4),dark);Cube("Mountain ridge right",new Vector3(16-i*2,3+(i+1)%2,13),new Vector3(3,6+(i+1)%3,4),dark);}
                    break;
                case 3:
                    for(int i=0;i<5;i++)Cube("Dungeon cell",new Vector3(-10+i*5,1.8f,5),new Vector3(.25f,3.6f,2.5f),gold);
                    break;
                case 5:
                    Cube("Banner pole",new Vector3(0,3.2f,4),new Vector3(.18f,6.4f,.18f),gold);
                    Cube("Derafsh Kaviani cloth",new Vector3(1.25f,4.7f,4),new Vector3(2.5f,1.6f,.08f),ember);
                    break;
                case 6:
                    for(int i=0;i<4;i++){var chain=GameObject.CreatePrimitive(PrimitiveType.Cylinder);chain.name="Dungeon chains";chain.transform.SetParent(environmentRoot,true);chain.transform.position=new Vector3(-9+i*6,1.6f,4);chain.transform.localScale=new Vector3(.08f,1.5f,.08f);chain.GetComponent<Renderer>().sharedMaterial=gold;}
                    break;
                case 7:
                case 9:
                    for(int i=0;i<4;i++){var idol=GameObject.CreatePrimitive(PrimitiveType.Sphere);idol.name="Serpent idol";idol.transform.SetParent(environmentRoot,true);idol.transform.position=new Vector3(-9+i*6,2,7);idol.transform.localScale=new Vector3(1.1f,3.5f,.7f);idol.GetComponent<Renderer>().sharedMaterial=Mat(new Color(.12f,.28f,.18f),.45f);}
                    break;
            }
            if(GameRoot.Instance!=null&&GameRoot.Instance.Chapter==6)
                for(int i=0;i<3;i++)Cube("Broken iron chain",new Vector3(-8+i*8,.7f,4),new Vector3(1.6f,.12f,.12f),gold);
            if(GameRoot.Instance!=null&&GameRoot.Instance.Chapter==8)
            {
                Cube("Alborz beacon base",new Vector3(0,.7f,5),new Vector3(2,1.4f,2),stone);
                var beaconObject=new GameObject("Alborz beacon light");beaconObject.transform.SetParent(environmentRoot,true);beaconObject.transform.position=new Vector3(0,2.5f,5);
                var beacon=beaconObject.AddComponent<Light>();beacon.type=LightType.Point;beacon.color=new Color(1f,.48f,.12f);beacon.range=14;beacon.intensity=3.2f;
            }
            for(int i=0;i<10;i++)
            {
                float x=Mathf.Sin(i*2.17f)*13f,z=Mathf.Cos(i*1.73f)*9f;
                Cube("Broken masonry",new Vector3(x,.2f,z),new Vector3(.55f+(i%3)*.2f,.4f,.65f),stone);
            }
            var lightObject=new GameObject("Warm directional light");lightObject.transform.SetParent(environmentRoot,true);
            var light=lightObject.AddComponent<Light>();light.type=LightType.Directional;light.intensity=1.25f;light.transform.rotation=Quaternion.Euler(48,-32,0);
            var fire=new GameObject("Forge glow");fire.transform.SetParent(environmentRoot,true);fire.transform.position=new Vector3(-7,2,3);
            var glow=fire.AddComponent<Light>();glow.type=LightType.Point;glow.color=new Color(1f,.32f,.08f);glow.range=9;glow.intensity=2.1f;
            var camera=Camera.main;camera.transform.position=new Vector3(0,7,-12);camera.transform.rotation=Quaternion.Euler(22,0,0);
            var follow=camera.GetComponent<FollowCamera>();if(!follow)follow=camera.gameObject.AddComponent<FollowCamera>();follow.Target=null;
        }

        public static void SpawnActors(GameRoot root)
        {
            var heroPrefab=Resources.Load<GameObject>("Characters/KavehHumanoid");
            GameObject hero;
            if(heroPrefab)
            {
                hero=Object.Instantiate(heroPrefab,new Vector3(-4,0,0),Quaternion.identity);
                hero.name="Kaveh - Blacksmith Rebel";
                var controller=hero.GetComponent<CharacterController>();if(!controller)controller=hero.AddComponent<CharacterController>();
                controller.height=1.9f;controller.radius=.38f;controller.center=new Vector3(0,.95f,0);
                if(!hero.GetComponent<PlayerMotor>())hero.AddComponent<PlayerMotor>();
            }
            else
            {
                hero=new GameObject("Kaveh - Blacksmith Rebel");hero.transform.position=new Vector3(-4,1,0);
                var controller=hero.AddComponent<CharacterController>();controller.height=2;controller.radius=.45f;
                hero.AddComponent<PlayerMotor>();var visuals=hero.AddComponent<CharacterVisuals>();visuals.IsHero=true;
            }
            root.RegisterActors(hero.GetComponent<PlayerMotor>(),null);
            SpawnEnemy(root,0);
        }

        public static void SpawnEnemy(GameRoot root,int wave)
        {
            var prefab=Resources.Load<GameObject>("Characters/DahhakHumanoid");
            Vector3[] points={new Vector3(4,0,0),new Vector3(1,0,6),new Vector3(5,0,-6)};
            Vector3 spawn=points[Mathf.Clamp(wave,0,points.Length-1)];
            GameObject enemy;
            if(prefab)
            {
                enemy=Object.Instantiate(prefab,spawn,Quaternion.identity);
                enemy.name=root.Current.enemyName+(wave>0?" - "+(wave+1):"");
                float scale=root.Chapter==9?1.22f+wave*.08f:1.05f+wave*.035f;
                enemy.transform.localScale=Vector3.one*scale;
                var collider=enemy.GetComponent<CapsuleCollider>();if(!collider)collider=enemy.AddComponent<CapsuleCollider>();
                collider.height=2.1f;collider.radius=.42f;collider.center=new Vector3(0,1.05f,0);
                foreach(var renderer in enemy.GetComponentsInChildren<Renderer>())
                    renderer.material.color=Color.Lerp(renderer.material.color,new Color(.38f,.08f,.1f),wave*.12f);
            }
            else
            {
                enemy=new GameObject(root.Current.enemyName);enemy.transform.position=spawn+Vector3.up;
                var collider=enemy.AddComponent<CapsuleCollider>();collider.height=2.4f;collider.radius=.55f;
                var visuals=enemy.AddComponent<CharacterVisuals>();visuals.IsBoss=true;
            }
            var brain=enemy.GetComponent<EnemyBrain>();if(!brain)brain=enemy.AddComponent<EnemyBrain>();
            root.RegisterEnemy(brain);
        }
    }
}