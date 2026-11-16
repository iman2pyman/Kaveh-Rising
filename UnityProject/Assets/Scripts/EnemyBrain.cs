using UnityEngine;

namespace KavehRising
{
    public class EnemyBrain : MonoBehaviour
    {
        float nextStrike;
        public bool IsDead { get; private set; }
        void Update()
        {
            var root=GameRoot.Instance;
            if(root==null || root.IsPaused || root.IsGameOver || root.IsVictory || IsDead || root.Player==null) return;
            var player=root.Player.transform;
            float d=Vector3.Distance(transform.position,player.position);
            if(d>2.1f)
            {
                Vector3 dir=(player.position-transform.position); dir.y=0;
                transform.position += dir.normalized * 1.25f * Time.deltaTime;
                if(dir.sqrMagnitude>.01f) transform.forward=dir.normalized;
            }
            else if(Time.time>=nextStrike)
            {
                nextStrike=Time.time+2.0f;
                if(!root.Player.IsDodging) root.DamagePlayer(1);
            }
        }
        public void MarkDead(){ IsDead=true; gameObject.SetActive(false); }
    }
}
