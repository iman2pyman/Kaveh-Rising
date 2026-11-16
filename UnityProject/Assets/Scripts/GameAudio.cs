using UnityEngine;

namespace KavehRising
{
    // Small synthesized effects keep the prototype self-contained until recorded audio is authored.
    public class GameAudio : MonoBehaviour
    {
        AudioSource source;
        AudioClip hit, heavy, hurt, win, ambience;
        void Awake()
        {
            source=gameObject.AddComponent<AudioSource>(); source.playOnAwake=false; source.spatialBlend=0f;
            hit=Tone("Hammer impact",115f,.16f,.28f);
            heavy=Tone("Heavy hammer",72f,.32f,.38f);
            hurt=Tone("Damage",185f,.22f,.24f);
            win=Tone("Victory",520f,.55f,.2f);
            ambience=Tone("Low forge ambience",58f,2f,.055f);
            source.clip=ambience; source.loop=true; source.Play();
        }
        AudioClip Tone(string name,float frequency,float seconds,float volume)
        {
            const int rate=22050; int count=Mathf.CeilToInt(rate*seconds);
            var data=new float[count];
            for(int i=0;i<count;i++)
            {
                float t=(float)i/rate;
                float envelope=Mathf.Exp(-t*(name.Contains("ambience")?.8f:9f));
                float fundamental=Mathf.Sin(2*Mathf.PI*frequency*t);
                float grit=Mathf.Sin(2*Mathf.PI*frequency*2.73f*t)*.22f;
                data[i]=(fundamental+grit)*envelope*volume;
            }
            var clip=AudioClip.Create(name,count,1,rate,false); clip.SetData(data,0); return clip;
        }
        public void PlayHit(bool isHeavy){source.PlayOneShot(isHeavy?heavy:hit);}
        public void PlayHurt(){source.PlayOneShot(hurt);}
        public void PlayVictory(){source.PlayOneShot(win);}
    }
}
