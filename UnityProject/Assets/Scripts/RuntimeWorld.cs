using UnityEngine;

namespace KavehRising
{
    public static class RuntimeWorld
    {
        static Transform environmentRoot;
        static Material Mat(Color c)
        {
            var shader = Shader.Find("Standard");
            if (shader == null) shader = Shader.Find("Universal Render Pipeline/Lit");
            var m = new Material(shader); m.color = c; return m;
        }
        static GameObject Cube(string name, Vector3 pos, Vector3 scale, Material mat)
        {
            var o = GameObject.CreatePrimitive(PrimitiveType.Cube);
            o.name=name; o.transform.position=pos; o.transform.localScale=scale;
            o.GetComponent<Renderer>().material=mat;
            if(environmentRoot!=null) o.transform.SetParent(environmentRoot,true);
            return o;
        }
        public static void BuildEnvironment(ChapterData chapter)
        {
            if(environmentRoot!=null) Object.Destroy(environmentRoot.gameObject);
            environmentRoot=new GameObject("Chapter Environment").transform;
            RenderSettings.skybox=null;
            if(Camera.main==null) throw new System.Exception("Main Camera was not created by Bootstrap.");
            Camera.main.backgroundColor=chapter.sky;
            RenderSettings.ambientLight=new Color(.48f,.43f,.38f);
            var floor=GameObject.CreatePrimitive(PrimitiveType.Plane);
            floor.name="Persian landscape"; floor.transform.localScale=new Vector3(3,1,2);
            floor.GetComponent<Renderer>().material=Mat(chapter.ground);
            floor.transform.SetParent(environmentRoot,true);
            var stone=Mat(new Color(.34f,.27f,.22f));
            var darkStone=Mat(new Color(.19f,.17f,.17f));
            for(int i=0;i<9;i++)
            {
                float x=(i-4)*4.5f;
                Cube("Ancient ruin",new Vector3(x,1.5f,7),new Vector3(1.2f,3,1.1f),stone);
                Cube("Broken wall",new Vector3(x+.8f,.8f,-7),new Vector3(2.5f,1.6f,.8f),darkStone);
            }
            // Silhouette gate and scattered rocks give every chapter a readable play space.
            Cube("Citadel gate left",new Vector3(-8,2.5f,5),new Vector3(1.2f,5,1.2f),darkStone);
            Cube("Citadel gate right",new Vector3(8,2.5f,5),new Vector3(1.2f,5,1.2f),darkStone);
            Cube("Citadel lintel",new Vector3(0,5,5),new Vector3(17,1,1.2f),stone);
            for(int i=0;i<14;i++)
            {
                float x=Mathf.Sin(i*2.17f)*11f;
                float z=Mathf.Cos(i*1.73f)*8f;
                Cube("Scattered stone",new Vector3(x,.25f,z),new Vector3(.5f+((i%3)*.25f),.5f,.55f),stone);
            }
            var light=new GameObject("Warm key light").AddComponent<Light>();
            light.type=LightType.Directional; light.intensity=1.15f;
            light.transform.rotation=Quaternion.Euler(48,-32,0);
            light.transform.SetParent(environmentRoot,true);
            var cam=Camera.main;
            cam.transform.position=new Vector3(0,7,-12);
            cam.transform.rotation=Quaternion.Euler(22,0,0);
            var follow=cam.GetComponent<FollowCamera>();
            if(follow==null) follow=cam.gameObject.AddComponent<FollowCamera>();
            follow.Target=null;
        }
        public static void SpawnActors(GameRoot root)
        {
            var hero=GameObject.CreatePrimitive(PrimitiveType.Capsule);
            hero.name="Kaveh"; hero.transform.position=new Vector3(-3,1,0);
            hero.GetComponent<Renderer>().material=Mat(new Color(.55f,.19f,.09f));
            var cc=hero.AddComponent<CharacterController>(); cc.height=2; cc.radius=.45f;
            var motor=hero.AddComponent<PlayerMotor>();
            var hammer=GameObject.CreatePrimitive(PrimitiveType.Cube);
            hammer.name="Kaveh's hammer"; hammer.transform.SetParent(hero.transform);
            hammer.transform.localPosition=new Vector3(.55f,.15f,.25f); hammer.transform.localScale=new Vector3(.18f,.85f,.18f);
            hammer.GetComponent<Renderer>().material=Mat(new Color(.32f,.30f,.28f));
            var foe=GameObject.CreatePrimitive(PrimitiveType.Capsule);
            foe.name=root.Current.enemyName; foe.transform.position=new Vector3(3,1,0);
            foe.GetComponent<Renderer>().material=Mat(new Color(.20f,.35f,.24f));
            foe.transform.localScale=new Vector3(1.15f,1.25f,1.15f);
            var brain=foe.AddComponent<EnemyBrain>();
            root.RegisterActors(motor,brain);
        }
    }
}
