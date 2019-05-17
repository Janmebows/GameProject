using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelInformation : MonoBehaviour {
    public static LevelInformation levelInfo;
    public System.Random rng;


    public bool useRandomSeed = true;
    public int customSeed;
   

    //singleton
    private void Awake()
    {
        if (levelInfo == null)
        {
            levelInfo = this;
        }
        else if (levelInfo != this)
        {
            Destroy(this);
        }

        if (useRandomSeed)
        {
            rng = new System.Random();
        }
        else
        {
            rng = new System.Random(customSeed);
        }
    }

}
