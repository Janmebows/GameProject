using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu()]
public class CameraSettings : ScriptableObject {

    public float distance;
    public float xSpeed;
    public float ySpeed;
    public float lockOnSpeed;
    public float yMinLimit;
    public float yMaxLimit;
    public Vector3 offset;

    public float distanceMin;
    public float distanceMax;
    public bool invertY;
    [Range(20,120)]
    public float FOV;

}
