using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu()]
public class WeaponGenerationSettings : ScriptableObject
{
    public List<GameObject> handles = new List<GameObject>();
    public List<GameObject> guards = new List<GameObject>();
    public List<GameObject> shafts = new List<GameObject>();
    public List<GameObject> pommels = new List<GameObject>();
    public List<GameObject> heads = new List<GameObject>();




}
