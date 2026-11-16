using UnityEngine;
namespace KavehRising {
 public sealed class MissionDirector : MonoBehaviour {
  public static MissionDirector Instance {get;private set;}
  static readonly string[][] Tasks={
   new[]{"روشن کردن آتشدان آهنگری"},new[]{"آزاد کردن اسیر مردم"},new[]{"برداشتن آذوقه کاروان"},
   new[]{"آزاد کردن زندانی اول","آزاد کردن زندانی دوم","آزاد کردن زندانی سوم"},
   new[]{"همراه کردن مردم","همراه کردن پیشه‌وران"},new[]{"برافراشتن درفش کاویانی"},
   new[]{"شکستن زنجیر اسیر اول","شکستن زنجیر اسیر دوم","شکستن زنجیر اسیر سوم"},
   new[]{"شکستن بت مار اول","شکستن بت مار دوم"},new[]{"روشن کردن آتش راهنما"},
   new[]{"از کار انداختن نشان مار اول","از کار انداختن نشان مار دوم","از کار انداختن نشان مار سوم"}
  };
  int chapter,done; Transform root; Material material;
  public bool IsComplete=>done>=Tasks[Mathf.Clamp(chapter,0,Tasks.Length-1)].Length;
  public string CurrentTask=>IsComplete?"هدف فرعی کامل شد":Tasks[Mathf.Clamp(chapter,0,Tasks.Length-1)][Mathf.Clamp(done,0,Tasks[Mathf.Clamp(chapter,0,Tasks.Length-1)].Length-1)];
  void Awake(){if(Instance&&Instance!=this){Destroy(this);return;}Instance=this;var s=Shader.Find("Standard");material=new Material(s);material.color=new Color(.9f,.48f,.12f);}
  public void BeginChapter(int index){
   chapter=Mathf.Clamp(index,0,Tasks.Length-1);done=0;if(root)Destroy(root.gameObject);root=new GameObject("Mission Targets").transform;
   var list=Tasks[chapter];
   for(int i=0;i<list.Length;i++){
    float x=list.Length==1?0:-7f+i*14f/(list.Length-1);
    var o=GameObject.CreatePrimitive(chapter==5?PrimitiveType.Cube:PrimitiveType.Sphere);o.name=list[i];o.transform.SetParent(root);
    o.transform.position=new Vector3(x,chapter==5?1.9f:1f,chapter==3||chapter==6?3f:1.5f);
    o.transform.localScale=chapter==5?new Vector3(.8f,2.4f,.18f):new Vector3(.85f,1f,.65f);
    o.GetComponent<Renderer>().material=material;o.AddComponent<ObjectiveTarget>().Init(list[i]);
   }
  }
  public bool TryInteract(Transform actor){
   if(IsComplete||!root)return false;ObjectiveTarget candidate=null;float nearest=2.8f;
   foreach(var t in root.GetComponentsInChildren<ObjectiveTarget>()){float d=Vector3.Distance(actor.position,t.transform.position);if(!t.Done&&d<nearest){nearest=d;candidate=t;}}
   if(!candidate)return false;string label=candidate.Label;candidate.Complete();done++;
   if(GameRoot.Instance){GameRoot.Instance.SetNotice(label+" — انجام شد");GameRoot.Instance.CheckChapterCompletion();}return true;
  }
  void OnDestroy(){if(Instance==this)Instance=null;}
 }
 public sealed class ObjectiveTarget:MonoBehaviour{
  public string Label{get;private set;} public bool Done{get;private set;}
  public void Init(string label){Label=label;}
  public void Complete(){if(Done)return;Done=true;var r=GetComponent<Renderer>();if(r)r.material.color=Color.green;var c=GetComponent<Collider>();if(c)c.enabled=false;Destroy(gameObject,1f);}
 }
}