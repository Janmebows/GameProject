using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BaseEntity : MonoBehaviour
{
    //public List<Appendage> appendages = new List<Appendage>();
    public List<Appendage> appendages;
    public bool humanoid;

    // Use this for initialization
    void Start()
    {
        appendages = new List<Appendage>(gameObject.GetComponents<Appendage>());
    }
    // Update is called once per frame
    void Update()
    {

    }



}
