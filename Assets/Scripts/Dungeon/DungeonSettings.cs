using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu()]
public class DungeonSettings : ScriptableObject {
    
    [Range(1,10000)]
    public int maxRooms;
    [Range(0,1)]
    public float collisionTolerance;


    public GameObject deadEnd;
    public GameObject door;
    public GameObject exit;
    public GameObject spawn;
    public List<GameObject> roomTypes = new List<GameObject>();

    public bool useRandomSeed;
    public int CustomSeed;

    

}
