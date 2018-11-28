using System.Collections;
using System.Collections.Generic;
using UnityEngine;


//The class for a particular character's limbs
//Head, torso, leg, arm, etc. all come under this class
public class Appendage : MonoBehaviour {

    //possibly change these for different limbs
    public enum Limb
    {
        Head,Neck,UpperChest,LowerChest,UpperArm,LowerArm,Hand,UpperLeg,LowerLeg,Foot,None
    }
    public Limb limbName = Limb.None;
    //effective damage thresholds for crush and sever
    public float maxCrushHP;
    public float maxSeverHP;
    //whether the weapon can be crushed or severed
    public bool severable;
    public bool crushable;
    //an appendage must have a collider
    public new Collider collider;

    public Appendage uplimb   = null;
    public Appendage downlimb = null;
    public Appendage leftlimb = null;
    public Appendage rightlim = null;

    //BROKEN LIMB STUFF
    //broken limbs will affect animations, damage, ability to perform certain attacks
    public bool isBroken;
    public float breakThreshold;
    //If damage surpasses this value the appendage may break in one hit
    public float oneHitBreakValue;
    public float breakRandomRange;

    //limb-damage multipliers for different damage types
    public float crushDamageMultiplier;
    public float severDamageMultiplier;


	// Use this for initialization
	void Start () { 

	}
	
	// Update is called once per frame
	void Update () {
		
	}

    void OnCollisionEnter(Collision collision)
    {
        Weapon weapon = collision.gameObject.GetComponent<Weapon>();
        //If hit by a weapon
        if (weapon!=null)
        {
            //TAKE A BIG LOAD OF DAMAGE
            TakeDamage(weapon);

        }
    }


    //
    void TakeDamage(Weapon weapon)
    {
        Debug.Log("YOU TOOK DAMAGE FROM" + weapon.name);
    }

}
