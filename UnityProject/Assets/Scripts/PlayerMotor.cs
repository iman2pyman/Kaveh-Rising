using UnityEngine;

namespace KavehRising
{
    [RequireComponent(typeof(CharacterController))]
    public class PlayerMotor : MonoBehaviour
    {
        CharacterController body; float move; float verticalVelocity; float nextAttack; float dodgeUntil; float nextDodge;
        float dodgeDirection=1f; bool crouching;
        public bool IsDodging=>Time.time<dodgeUntil;
        public bool IsCrouching=>crouching;
        void Awake(){body=GetComponent<CharacterController>();}
        public void SetMove(float value){move=Mathf.Clamp(value,-1f,1f);}
        public void SetCrouch(bool value){crouching=value;}
        void Update()
        {
            var root=GameRoot.Instance;
            if(root==null||root.IsPaused||root.IsGameOver||root.IsVictory){move=0;return;}
            float axis=Mathf.Abs(move)>.01f?move:Input.GetAxisRaw("Horizontal");
            float speed=crouching?1.65f:4.2f;
            if(body.isGrounded&&verticalVelocity<0)verticalVelocity=-2f;
            verticalVelocity+=Physics.gravity.y*Time.deltaTime;
            Vector3 dir=new Vector3(axis,0,0); Vector3 planar=dir*speed;
            if(IsDodging)planar=Vector3.right*dodgeDirection*8.5f;
            body.Move((planar+Vector3.up*verticalVelocity)*Time.deltaTime);
            if(dir.sqrMagnitude>.01f)transform.forward=dir.normalized;
            move=0;
        }
        public void Dodge()
        {
            var root=GameRoot.Instance;
            if(root==null||root.IsPaused||root.IsGameOver||root.IsVictory||Time.time<nextDodge)return;
            nextDodge=Time.time+1.7f; dodgeUntil=Time.time+.38f;
            dodgeDirection=Mathf.Abs(move)>.1f?Mathf.Sign(move):Mathf.Sign(transform.forward.x);
            if(Mathf.Abs(dodgeDirection)<.1f)dodgeDirection=1f;
        }
        public void Attack(bool heavy)
        {
            var root=GameRoot.Instance;
            if(root==null||root.IsPaused||root.IsGameOver||root.IsVictory||Time.time<nextAttack)return;
            nextAttack=Time.time+(heavy?1.25f:.48f);
            var enemy=root.Enemy; if(enemy==null||enemy.IsDead)return;
            Vector3 delta=enemy.transform.position-transform.position;
            bool front=delta.x*transform.forward.x>=-.15f;
            if(delta.magnitude<=(heavy?3.6f:2.65f)&&front)
            {
                root.DamageEnemy(heavy?3:1);
                CombatEffects.Burst(enemy.transform.position+Vector3.up,heavy?new Color(1f,.48f,.12f):Color.gray,heavy?28:14);
                var audio=FindObjectOfType<GameAudio>(); if(audio)audio.PlayHit(heavy);
            }
        }
    }
}
