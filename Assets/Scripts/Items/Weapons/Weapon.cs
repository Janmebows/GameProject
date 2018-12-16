using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Weapon : MonoBehaviour {
    public string weaponName;
    GameObject owner;
    //a weapon may have more than one collider (e.g. hilt + blade) 
    public Collider[] colliders;
    GameObject weapon;
    //public WeaponPieceData[] pieces;
    //A weapon is Active if it is swinging
    bool isActive = false;

    //constructor for WeaponAssembly to do nice things
    Weapon(ref WeaponPieceData[] piece) : base()
    {       

    }
	// Use this for initialization
	void Start () {
        owner = this.transform.root.gameObject;

        NoInternalCollision();

	}
    void Update()
    {

        
    }

    void OnSwing()
    {
        isActive = true;
        for (int i = 0; i < colliders.Length; i++)
        {
            colliders[i].enabled = true;
        }


    }


    //NoInternalCollision stops a weapon from colliding with its owner
    //May need to handle this differently, could produce some weird results
    void NoInternalCollision()
    {
        for(int i=1; i<=colliders.Length; i++)
        {
            foreach (Collider collider in owner.GetComponentsInChildren<Collider>())
            {
                Physics.IgnoreCollision(colliders[i],collider,true);
            }

        }
    }

    void OnCollisionEnter(Collision collision)
    {
        
    }
}
