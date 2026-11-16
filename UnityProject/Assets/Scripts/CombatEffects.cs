using UnityEngine;

namespace KavehRising
{
    public static class CombatEffects
    {
        public static void Burst(Vector3 position, Color color, int amount=18)
        {
            var go=new GameObject("Impact particles");
            go.transform.position=position;
            var ps=go.AddComponent<ParticleSystem>();
            var main=ps.main; main.duration=.35f; main.loop=false; main.startLifetime=.35f;
            main.startSpeed=3.2f; main.startSize=.12f; main.startColor=color; main.maxParticles=amount;
            var emission=ps.emission; emission.rateOverTime=0; emission.SetBursts(new[]{new ParticleSystem.Burst(0,(short)amount)});
            var shape=ps.shape; shape.shapeType=ParticleSystemShapeType.Sphere; shape.radius=.18f;
            var renderer=ps.GetComponent<ParticleSystemRenderer>();
            renderer.material=new Material(Shader.Find("Sprites/Default"));
            Object.Destroy(go,1.2f);
        }
    }
}
