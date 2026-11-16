using System.Collections;
using UnityEngine;

namespace KavehRising
{
    public sealed class EnemyBrain : MonoBehaviour
    {
        float nextStrike,nextPatrol;
        Vector3 patrolTarget;
        HumanoidAnimationDriver animationDriver;
        RiggedCharacterAnimation rigAnimation;
        public bool IsDead { get; private set; }

        void Awake()
        {
            animationDriver=GetComponentInChildren<HumanoidAnimationDriver>();
            rigAnimation=GetComponent<RiggedCharacterAnimation>();
            patrolTarget=transform.position+Vector3.left*3f;
        }
        void Update()
        {
            var root=GameRoot.Instance;
            if(root==null||root.IsPaused||root.IsGameOver||root.IsVictory||IsDead||root.Player==null)return;
            var player=root.Player.transform;
            Vector3 delta=player.position-transform.position;delta.y=0;
            float distance=delta.magnitude;
            if(root.Player.IsCrouching&&distance>3.4f)
            {
                if(Time.time>=nextPatrol)
                {
                    nextPatrol=Time.time+2.5f;
                    patrolTarget=transform.position+(Random.value>.5f?Vector3.left:Vector3.right)*2f;
                }
                Vector3 patrol=patrolTarget-transform.position;patrol.y=0;
                if(patrol.magnitude>.3f){transform.position+=patrol.normalized*.55f*Time.deltaTime;transform.forward=patrol.normalized;}
                if(animationDriver)animationDriver.SetLocomotion(.2f,false);
                if(rigAnimation)rigAnimation.SetLocomotion(.2f,false);
                return;
            }
            if(distance>2.15f)
            {
                float speed=root.Player.IsCrouching ? 0.8f : 1.45f;
                transform.position+=delta.normalized*speed*Time.deltaTime;
                if(delta.sqrMagnitude>.01f)transform.forward=delta.normalized;
                if(animationDriver)animationDriver.SetLocomotion(speed/1.45f,false);
                if(rigAnimation)rigAnimation.SetLocomotion(speed/1.45f,false);
            }
            else if(Time.time>=nextStrike)
            {
                nextStrike=Time.time+1.8f;
                if(animationDriver)animationDriver.PlayAttack(false);
                if(rigAnimation)rigAnimation.PlayAttack(false);
                StartCoroutine(StrikePlayer(root));
            }
        }
        IEnumerator StrikePlayer(GameRoot root)
        {
            yield return new WaitForSeconds(.28f);
            if(root==null||root.IsPaused||root.IsGameOver||root.IsVictory||root.Player==null||IsDead)return;
            if(Vector3.Distance(root.Player.transform.position,transform.position)<2.6f&&!root.Player.IsDodging)
                root.DamagePlayer(1);
        }

        public void MarkDead()
        {
            if(IsDead)return;
            IsDead=true;
            if(animationDriver)animationDriver.PlayDeath();
            if(rigAnimation)rigAnimation.PlayDeath();
            foreach(var collider in GetComponents<Collider>())collider.enabled=false;
            enabled=false;
            Destroy(gameObject,2.5f);
        }
    }
}