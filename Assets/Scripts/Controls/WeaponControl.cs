using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponControl : MonoBehaviour
{
    
    BaseEntity baseEntity;
    public Transform sheathed;
    public Transform drawn;
    public GameObject weapon;
    public bool weaponDrawn = false;

    // Use this for initialization
    void Start ()
    {

    }
	
	// Update is called once per frame
	void Update ()
    {
        if (Input.GetButtonDown("XboxY"))
        {
            DrawWeapon(true);
        }
           
        
        
        // reference to weapon > animation > slot/parent
    }

    //kinda clunky code rn but does the trick!
    void DrawWeapon(bool input)
    {
        weaponDrawn = weaponDrawn ^ input;

        if (weaponDrawn)
        {
            weapon.transform.parent = drawn.transform;
            weapon.transform.localPosition = Vector3.zero;
            weapon.transform.localRotation = Quaternion.identity;
        }

        else
        {
            weapon.transform.parent = sheathed.transform;
            weapon.transform.localPosition = Vector3.zero;
            weapon.transform.localRotation = Quaternion.identity;
        }
    }
}
