using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestGenerateWeaponButton : MonoBehaviour {

    
	void OnGUI() {
		if(GUILayout.Button("The fuck d'ya say about me cunt?"))
        {
            StartCoroutine(WeaponAssembly.weaponAssembly.AssembleWeapon());
        }
	}
}
