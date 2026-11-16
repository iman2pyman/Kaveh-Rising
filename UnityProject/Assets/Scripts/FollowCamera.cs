using UnityEngine;

namespace KavehRising
{
    public class FollowCamera : MonoBehaviour
    {
        public Transform Target;
        public Vector3 Offset = new Vector3(0f, 6.5f, -10.5f);
        public float Smooth = 6f;

        void LateUpdate()
        {
            if (Target == null && GameRoot.Instance != null && GameRoot.Instance.Player != null)
                Target = GameRoot.Instance.Player.transform;
            if (Target == null) return;
            Vector3 desired = Target.position + Offset;
            transform.position = Vector3.Lerp(transform.position, desired, Time.deltaTime * Smooth);
            transform.LookAt(Target.position + Vector3.up * 1.1f);
        }
    }
}
