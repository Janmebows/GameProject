using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponControl : MonoBehaviour
{
    
    BaseEntity baseEntity;
    public Transform sheathed;
    public Transform drawn;
    public Transform axe;
    public bool weaponDrawn = false;

    // Use this for initialization
    void Start ()
    {

    }
	
	// Update is called once per frame
	void Update ()
    {
        weaponDrawn = weaponDrawn ^ Input.GetButtonDown("XboxY");

        if (weaponDrawn)
        {
            axe.transform.parent = drawn.transform;
            axe.transform.localPosition = Vector3.zero;
            axe.transform.localRotation = Quaternion.identity;
        }

        else
        {
            axe.transform.parent = sheathed.transform;
            axe.transform.localPosition = Vector3.zero;
            axe.transform.localRotation = Quaternion.identity;
        }
           
        
        
        // reference to weapon > animation > slot/parent
    }
}
