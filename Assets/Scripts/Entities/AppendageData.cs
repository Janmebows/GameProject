using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu()]
public class AppendageData : ScriptableObject
{

    //possibly change these for different limbs
    public string limbName;
    //effective damage thresholds for crush and sever
    public float maxCrushHP;
    public float maxSeverHP;
    //whether the weapon can be crushed or severed
    public bool severable;
    public bool crushable;


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
}
