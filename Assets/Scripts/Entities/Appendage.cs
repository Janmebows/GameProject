using System.Collections;
using System.Collections.Generic;
using UnityEngine;


//The class for a particular character's limbs
//Head, torso, leg, arm, etc. all come under this class
public class Appendage : MonoBehaviour {
    //contains all base information
    public AppendageData baseAppendageData;

    //an appendage must have a collider
    public new Collider collider;
    


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
