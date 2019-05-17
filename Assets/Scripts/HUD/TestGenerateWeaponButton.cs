using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestGenerateWeaponButton : MonoBehaviour {

    
	void OnGUI() {
		if(GUILayout.Button("Generate Weapon"))
        {
            StartCoroutine(WeaponAssembly.weaponAssembly.AssembleWeapon());
        }
	}
}
