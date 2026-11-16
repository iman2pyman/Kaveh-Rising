using UnityEngine;

namespace KavehRising
{
    // Drives imported Humanoid rigs. Assign a real rigged FBX/Animator Controller in the prefab.
    // Parameters: Speed(float), Crouch(bool), Attack(trigger), HeavyAttack(trigger),
    // Dodge(trigger), Hit(trigger), Die(trigger), Grounded(bool).
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Animator))]
    public sealed class HumanoidAnimationDriver : MonoBehaviour
    {
        static readonly int Speed = Animator.StringToHash("Speed");
        static readonly int Crouch = Animator.StringToHash("Crouch");
        static readonly int Attack = Animator.StringToHash("Attack");
        static readonly int HeavyAttack = Animator.StringToHash("HeavyAttack");
        static readonly int Dodge = Animator.StringToHash("Dodge");
        static readonly int Hit = Animator.StringToHash("Hit");
        static readonly int Die = Animator.StringToHash("Die");
        static readonly int Grounded = Animator.StringToHash("Grounded");

        Animator animator;
        bool dead;
        void Awake() { animator = GetComponent<Animator>(); animator.applyRootMotion = false; }
        public void SetLocomotion(float speed, bool crouching, bool grounded = true)
        {
            if (!animator || dead) return;
            animator.SetFloat(Speed, Mathf.Clamp01(speed), .12f, Time.deltaTime);
            animator.SetBool(Crouch, crouching);
            animator.SetBool(Grounded, grounded);
        }
        public void PlayAttack(bool heavy) { if (!dead && animator) animator.SetTrigger(heavy ? HeavyAttack : Attack); }
        public void PlayDodge() { if (!dead && animator) animator.SetTrigger(Dodge); }
        public void PlayHit() { if (!dead && animator) animator.SetTrigger(Hit); }
        public void PlayDeath() { if (!dead && animator) { dead = true; animator.SetTrigger(Die); } }
    }
}
