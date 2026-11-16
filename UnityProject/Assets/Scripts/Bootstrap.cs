using UnityEngine;

namespace KavehRising
{
    public class Bootstrap : MonoBehaviour
    {
        void Awake()
        {
            if(Camera.main==null)
            {
                var cameraObject=new GameObject("Main Camera");
                cameraObject.tag="MainCamera";
                cameraObject.AddComponent<Camera>();
                cameraObject.AddComponent<AudioListener>();
            }
            if(FindObjectOfType<GameRoot>()==null)
            {
                var root=new GameObject("KAVEH RISING - Game");
                root.AddComponent<GameRoot>();
            }
        }
    }
}
