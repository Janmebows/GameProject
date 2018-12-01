using System.Collections;
using System.Collections.Generic;
using UnityEngine;


//The class for a particular character's limbs
//Head, torso, leg, arm, etc. all come under this class
public class Appendage : MonoBehaviour {
    public AppendageData baseAppendageData;

    public bool severable;
    public bool crushable;
    //an appendage must have a collider
    public new Collider collider;
    

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
            //do a bunch of shitty calculations
            //StartCoroutine();

        }
    }


    //
    void TakeDamage(Weapon weapon)
    {
        Debug.Log("YOU TOOK DAMAGE FROM" + weapon.name);
    }

}
