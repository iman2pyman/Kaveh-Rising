using UnityEngine;

namespace KavehRising
{
    public class EnemyBrain : MonoBehaviour
    {
        float nextStrike; HumanoidAnimationDriver animationDriver;
        void Awake(){animationDriver=GetComponentInChildren<HumanoidAnimationDriver>();}
        public bool IsDead { get; private set; }

        void Update()
        {
            var root = GameRoot.Instance;
            if (root == null || root.IsPaused || root.IsGameOver || root.IsVictory || IsDead || root.Player == null) return;
            var player = root.Player.transform;
            Vector3 delta = player.position - transform.position;
            delta.y = 0;
            float distance = delta.magnitude;
            if (distance > 2.15f)
            {
                float speed = root.Player.IsCrouching ? .8f : 1.45f;
                transform.position += delta.normalized * speed * Time.deltaTime;
                if (delta.sqrMagnitude > .01f) transform.forward = delta.normalized;
                if(animationDriver)animationDriver.SetLocomotion(speed/1.45f,false);
            }
            else if (Time.time >= nextStrike)
            {
                nextStrike = Time.time + 1.8f;
                if (!root.Player.IsDodging) root.DamagePlayer(1);
                if(animationDriver)animationDriver.PlayAttack(false);
            }
        }

        public void MarkDead()
        {
            if (IsDead) return;
            IsDead = true;
            if(animationDriver)animationDriver.PlayDeath();
            foreach (var c in GetComponents<Collider>()) c.enabled = false;
            Destroy(gameObject, 2.5f);
            enabled = false;
        }
    }
}
