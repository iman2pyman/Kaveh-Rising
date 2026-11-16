using UnityEngine;
namespace KavehRising {
 [RequireComponent(typeof(CharacterController))]
 public class PlayerMotor:MonoBehaviour {
  CharacterController body; float moveX,moveZ,verticalVelocity,nextAttack,dodgeUntil,nextDodge;
  Vector3 dodgeDirection=Vector3.right; bool crouching; HumanoidAnimationDriver animationDriver; RiggedCharacterAnimation rigAnimation; CharacterVisuals visuals;
  public bool IsDodging=>Time.time<dodgeUntil; public bool IsCrouching=>crouching; public float MoveAmount{get;private set;}
  void Awake(){body=GetComponent<CharacterController>();animationDriver=GetComponentInChildren<HumanoidAnimationDriver>();rigAnimation=GetComponent<RiggedCharacterAnimation>();visuals=GetComponent<CharacterVisuals>();}
  public void SetMove(float x){SetMove(x,0);}
  public void SetMove(float x,float z){moveX=Mathf.Clamp(x,-1,1);moveZ=Mathf.Clamp(z,-1,1);}
  public void SetCrouch(bool value){crouching=value;}
  void Update(){
   var root=GameRoot.Instance;if(root==null||root.IsPaused||root.IsGameOver||root.IsVictory){moveX=moveZ=0;MoveAmount=0;return;}
   float x=Mathf.Abs(moveX)>.01f?moveX:Input.GetAxisRaw("Horizontal");float z=Mathf.Abs(moveZ)>.01f?moveZ:Input.GetAxisRaw("Vertical");
   Vector3 direction=new Vector3(x,0,z);if(direction.sqrMagnitude>1)direction.Normalize();
   float speed=crouching?1.65f:4.2f;Vector3 planar=direction*speed;if(IsDodging)planar=dodgeDirection*8.5f;
   if(body.isGrounded&&verticalVelocity<0)verticalVelocity=-2f;verticalVelocity+=Physics.gravity.y*Time.deltaTime;
   body.Move((planar+Vector3.up*verticalVelocity)*Time.deltaTime);
   if(direction.sqrMagnitude>.01f)transform.forward=direction.normalized;
   MoveAmount=planar.magnitude/4.2f;if(animationDriver)animationDriver.SetLocomotion(MoveAmount,crouching,body.isGrounded);if(rigAnimation)rigAnimation.SetLocomotion(MoveAmount,crouching);
   moveX=moveZ=0;
  }
  public void Dodge(){
   var root=GameRoot.Instance;if(root==null||root.IsPaused||root.IsGameOver||root.IsVictory||Time.time<nextDodge)return;
   nextDodge=Time.time+1.7f;dodgeUntil=Time.time+.38f;Vector3 input=new Vector3(moveX,0,moveZ);
   dodgeDirection=input.sqrMagnitude>.1f?input.normalized:transform.forward;if(animationDriver)animationDriver.PlayDodge();if(rigAnimation)rigAnimation.PlayDodge();
  }
  public void PlayHit(){if(animationDriver)animationDriver.PlayHit();if(rigAnimation)rigAnimation.PlayHit();}
  public void Attack(bool heavy){
   var root=GameRoot.Instance;if(root==null||root.IsPaused||root.IsGameOver||root.IsVictory||Time.time<nextAttack)return;
   nextAttack=Time.time+(heavy?1.25f:.48f);if(animationDriver)animationDriver.PlayAttack(heavy);if(rigAnimation)rigAnimation.PlayAttack(heavy);if(visuals)visuals.PlayAttack(heavy);
   var target=root.Enemy;if(target==null||target.IsDead)return;Vector3 delta=target.transform.position-transform.position;delta.y=0;
   bool front=delta.sqrMagnitude<.01f||Vector3.Dot(delta.normalized,transform.forward)>.05f;
   if(delta.magnitude<=(heavy?3.6f:2.65f)&&front){root.DamageEnemy(heavy?3:1);CombatEffects.Burst(target.transform.position+Vector3.up,heavy?new Color(1f,.48f,.12f):Color.gray,heavy?28:14);var audio=FindObjectOfType<GameAudio>();if(audio)audio.PlayHit(heavy);}
  }
 }
}