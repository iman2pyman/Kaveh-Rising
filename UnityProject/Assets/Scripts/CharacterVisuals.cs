using UnityEngine;

namespace KavehRising
{
    // Lightweight procedural low-poly character kit: no external asset dependency.
    public class CharacterVisuals : MonoBehaviour
    {
        public bool IsBoss;
        public bool IsHero;
        public Transform Weapon;
        Transform leftArm, rightArm, leftLeg, rightLeg, hammer;\n        float attackTime, attackDuration; bool heavyAttack;
        Vector3 armRest, legRest;
        float phase;

        Material Make(Color color, float metallic = 0f)
        {
            var shader = Shader.Find("Standard");
            var m = new Material(shader); m.color = color; m.SetFloat("_Metallic", metallic); return m;
        }
        GameObject Part(string name, PrimitiveType type, Vector3 pos, Vector3 scale, Material mat, Transform parent)
        {
            var p=GameObject.CreatePrimitive(type); p.name=name; p.transform.SetParent(parent,false);
            p.transform.localPosition=pos; p.transform.localScale=scale;
            p.GetComponent<Renderer>().material=mat;
            var c=p.GetComponent<Collider>(); if(c) Destroy(c);
            return p;
        }
        void Start()
        {
            var skin=Make(IsHero?new Color(.68f,.43f,.28f):new Color(.43f,.48f,.39f));
            var cloth=Make(IsHero?new Color(.48f,.12f,.055f):new Color(.13f,.19f,.16f));
            var leather=Make(new Color(.12f,.075f,.05f));
            var bronze=Make(new Color(.62f,.37f,.12f),.55f);
            var steel=Make(new Color(.35f,.39f,.42f),.72f);
            var dark=Make(new Color(.055f,.045f,.04f));
            Part("Torso / woven tunic",PrimitiveType.Capsule,new Vector3(0,.05f,0),new Vector3(.78f,.88f,.48f),cloth,transform);
            Part("Shoulder mantle",PrimitiveType.Cube,new Vector3(0,.42f,0),new Vector3(1.05f,.18f,.56f),IsHero?bronze:steel,transform);
            Part("Head",PrimitiveType.Sphere,new Vector3(0,.83f,0),new Vector3(.48f,.55f,.45f),skin,transform);
            Part("Hair",PrimitiveType.Sphere,new Vector3(0,1.02f,-.045f),new Vector3(.52f,.28f,.48f),dark,transform);
            if(IsHero)
            {
                Part("Blacksmith beard",PrimitiveType.Sphere,new Vector3(0,.63f,.19f),new Vector3(.34f,.34f,.24f),dark,transform);
                Part("Leather apron",PrimitiveType.Cube,new Vector3(0,-.08f,.25f),new Vector3(.48f,.66f,.08f),leather,transform);
                Part("Waist belt",PrimitiveType.Cube,new Vector3(0,-.28f,.02f),new Vector3(.8f,.12f,.52f),bronze,transform);
            }
            else
            {
                Part("War mask",PrimitiveType.Cube,new Vector3(0,.78f,.22f),new Vector3(.35f,.12f,.08f),bronze,transform);
                Part("Crest",PrimitiveType.Cylinder,new Vector3(0,1.22f,0),new Vector3(.10f,.36f,.10f),bronze,transform);
                if(IsBoss)
                {
                    Part("Demon left horn",PrimitiveType.Cylinder,new Vector3(-.25f,1.16f,0),new Vector3(.09f,.5f,.09f),bronze,transform).transform.localRotation=Quaternion.Euler(0,0,28);
                    Part("Demon right horn",PrimitiveType.Cylinder,new Vector3(.25f,1.16f,0),new Vector3(.09f,.5f,.09f),bronze,transform).transform.localRotation=Quaternion.Euler(0,0,-28);
                    Part("Scale armor",PrimitiveType.Sphere,new Vector3(0,-.05f,-.22f),new Vector3(.9f,.72f,.22f),steel,transform);
                }
            }
            leftArm=Part("Left arm",PrimitiveType.Capsule,new Vector3(-.48f,.12f,0),new Vector3(.23f,.62f,.23f),skin,transform).transform;
            rightArm=Part("Right arm",PrimitiveType.Capsule,new Vector3(.48f,.12f,0),new Vector3(.23f,.62f,.23f),skin,transform).transform;
            leftLeg=Part("Left leg",PrimitiveType.Capsule,new Vector3(-.2f,-.62f,0),new Vector3(.24f,.65f,.26f),leather,transform).transform;
            rightLeg=Part("Right leg",PrimitiveType.Capsule,new Vector3(.2f,-.62f,0),new Vector3(.24f,.65f,.26f),leather,transform).transform;
            Part("Left bracer",PrimitiveType.Cube,new Vector3(-.49f,-.02f,.02f),new Vector3(.28f,.2f,.28f),bronze,transform);
            Part("Right bracer",PrimitiveType.Cube,new Vector3(.49f,-.02f,.02f),new Vector3(.28f,.2f,.28f),bronze,transform);
            if(IsHero)
            {
                var h=Part("Hammer handle",PrimitiveType.Cylinder,new Vector3(.66f,.02f,.2f),new Vector3(.075f,.55f,.075f),leather,transform); hammer=h.transform;
                h.transform.localRotation=Quaternion.Euler(0,0,-15);
                Part("Forged hammer head",PrimitiveType.Cube,new Vector3(.72f,.34f,.2f),new Vector3(.42f,.2f,.25f),steel,transform);
            }
            phase=Random.value*6f;
        }
        public void PlayAttack(bool heavy){heavyAttack=heavy;attackDuration=heavy?.58f:.32f;attackTime=attackDuration;}\n        void Update()
        {
            float speed=GetComponent<PlayerMotor>()?GetComponent<PlayerMotor>().MoveAmount:0f;
            phase+=Time.deltaTime*(speed>.1f?9f:2f);
            float swing=Mathf.Sin(phase)*Mathf.Clamp01(speed);
            leftLeg.localRotation=Quaternion.Euler(swing*24,0,0);
            rightLeg.localRotation=Quaternion.Euler(-swing*24,0,0);
            leftArm.localRotation=Quaternion.Euler(-swing*18,0,0);
            rightArm.localRotation=Quaternion.Euler(swing*18,0,0);\n            if(attackTime>0){float t=1f-attackTime/attackDuration;float arc=Mathf.Sin(t*Mathf.PI)*(heavyAttack?105f:72f);rightArm.localRotation=Quaternion.Euler(-arc,0,heavyAttack?18f:8f);attackTime-=Time.deltaTime;}
        }
    }
}
