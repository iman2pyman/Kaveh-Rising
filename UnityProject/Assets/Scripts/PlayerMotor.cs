using UnityEngine;

namespace KavehRising
{
    [RequireComponent(typeof(CharacterController))]
    public class PlayerMotor : MonoBehaviour
    {
        CharacterController body;
        float move;
        float verticalVelocity;
        float nextAttack;
        float dodgeUntil;
        float nextDodge;
        Animator animator;

        void Awake() { body = GetComponent<CharacterController>(); }
        public void SetMove(float value) { move = value; }
        void Update()
        {
            var root = GameRoot.Instance;
            if (root == null || root.IsPaused || root.IsGameOver || root.IsVictory) { move = 0; return; }
            float axis = Mathf.Abs(move) > .01f ? move : Input.GetAxisRaw("Horizontal");
            if (body.isGrounded && verticalVelocity < 0) verticalVelocity = -2f;
            verticalVelocity += Physics.gravity.y * Time.deltaTime;
            Vector3 direction = new Vector3(axis,0,0);
            body.Move((direction * 4.2f + Vector3.up * verticalVelocity) * Time.deltaTime);
            if (direction.sqrMagnitude > .01f) transform.forward = direction.normalized;
            move = 0;
        }

        public void Dodge()
        {
            if (Time.time < nextDodge) return;
            nextDodge = Time.time + 2.0f;
            dodgeUntil = Time.time + .45f;
            body.Move(transform.forward * 3.5f);
        }
        public bool IsDodging => Time.time < dodgeUntil;

        public void Attack(bool heavy)
        {
            if (Time.time < nextAttack) return;
            nextAttack = Time.time + (heavy ? 1.5f : .55f);
            var enemy = GameRoot.Instance.Enemy;
            if (enemy == null || enemy.IsDead) return;
            float distance = Vector3.Distance(transform.position, enemy.transform.position);
            if (distance <= (heavy ? 3.4f : 2.5f))
                GameRoot.Instance.DamageEnemy(heavy ? 3 : 1);
        }
    }
}
