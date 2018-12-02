using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu()]
public class AppendageData : ScriptableObject
{

    //possibly change these for different limbs
    public string limbName ="";
    public enum AppendageType
    {
        ArmU,ArmL,Foot,Hand,Head,LegU,LegL,Neck,TorsoU,TorsoL,Waist,Other
    }
    public AppendageType appendageType = AppendageType.Other;
    //effective damage thresholds for crush and sever
    public float maxCrushHP =0f;
    public float maxSeverHP=0f;
    //whether the weapon can be crushed or severed
    public bool severable=true;
    public bool crushable = true;


    //BROKEN LIMB STUFF
    //broken limbs will affect animations, damage, ability to perform certain attacks
    public bool isBroken = false;
    public float breakThreshold =0f;
    //If damage surpasses this value the appendage may break in one hit
    public float oneHitBreakValue =0f;
    public float breakRandomRange =0f;

    //limb-damage multipliers for different damage types
    public float crushDamageMultiplier =1f;
    public float severDamageMultiplier =1f;
    // Use this for initialization
    void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
