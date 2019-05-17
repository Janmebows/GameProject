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
    Animator anim;

    // Use this for initialization
    void Start ()
    {
        anim = GetComponent<Animator>();
    }
	
	// Update is called once per frame
	void Update ()
    {
        if (Input.GetButtonDown("XboxY"))
        {
            DrawWeapon(true);
        }
           
        if (Input.GetButtonDown("XboxX"))
        {
            anim.SetTrigger("isSwinging");
        }
        
        // reference to weapon > animation > slot/parent
    }

    //kinda clunky code rn but does the trick!
    void DrawWeapon(bool input)
    {
        weaponDrawn ^= input;

        if (weaponDrawn)
        {
            weapon.transform.parent = drawn.transform;
            weapon.transform.localPosition = Vector3.zero;
            weapon.transform.localRotation = Quaternion.identity;
            anim.SetBool("weaponDrawn", true);
        }

        else
        {
            weapon.transform.parent = sheathed.transform;
            weapon.transform.localPosition = Vector3.zero;
            weapon.transform.localRotation = Quaternion.identity;
            anim.SetBool("weaponDrawn", false);
        }
    }
}
