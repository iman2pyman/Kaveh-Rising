using UnityEngine;

namespace KavehRising
{
    // Gameplay SFX only. Background music and Persian voice acting are intentionally user-supplied later.
    public sealed class GameAudio : MonoBehaviour
    {
        AudioSource source;
        AudioClip lightHit, heavyHit, hurt, victory, swing, forge, door, metal, stoneStep, grassStep;

        void Awake()
        {
            source=gameObject.AddComponent<AudioSource>();
            source.playOnAwake=false;
            source.loop=false;
            source.spatialBlend=0f;
            lightHit=Load("impact-light");
            heavyHit=Load("hammer-heavy");
            hurt=Load("armor-hit");
            victory=Load("victory-bell");
            swing=Load("weapon-swing");
            forge=Load("forge-chop");
            door=Load("dungeon-door");
            metal=Load("metal-clang");
            stoneStep=Load("footstep-stone");
            grassStep=Load("footstep-grass");
        }
        AudioClip Load(string name){return Resources.Load<AudioClip>("Audio/SFX/"+name);}
        void Play(AudioClip clip,float volume=1f){if(clip&&source)source.PlayOneShot(clip,volume);}
        public void PlayHit(bool isHeavy){Play(isHeavy?heavyHit:lightHit,isHeavy ? .95f : .72f);}
        public void PlayHurt(){Play(hurt,.8f);}
        public void PlayVictory(){Play(victory,.85f);}
        public void PlaySwing(bool isHeavy){Play(swing,isHeavy ? .9f : .58f);}
        public void PlayFootstep(bool crouching){Play(crouching?grassStep:stoneStep,crouching ? .22f : .42f);}
        public void PlayForge(){Play(forge,.65f);}
        public void PlayDoor(){Play(door,.8f);}
        public void PlayObjective(){Play(metal,.6f);}
    }
}