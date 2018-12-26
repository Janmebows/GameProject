using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class WeaponPiece : MonoBehaviour
{
    public enum pieceQuality
    {
        awful, broken, bad, average, decent, good, amazing, overpowered
    }
    public enum pieceType
    {
        guard, handle, head, pommel, shaft,noType
    }


    //CORE INFORMATION
    public string pieceName;
    public GameObject prefab;
    public List<ConnectPoint> connections;
    public new Collider collider;



    //damage factors
    //possibly if top qualities add a visual effect?
    public pieceQuality quality = pieceQuality.average;
    //effects the swing speed of the weapon
    public float speedMultiplier = 1f;
    //weight of the part in kg
    public float weight = 0f;
    [Range(0, 1)]
    public float severProportion = 0.5f;
    [Range(0, 1)]
    public float crushProportion = 0.5f;


    //generate this based on quality (with some randomness) and then all the damage multipliers are capped based on this
    //for now it does nothing though
    private int pieceValue;

    private void OnDrawGizmos()
    {
        try
        {
            foreach (ConnectPoint connect in connections)
            {

                Gizmos.color = new Color(0.5f, connect.acceptableConnections.Count / 5f, 0);
                Gizmos.DrawWireSphere(connect.transform.position, 0.01f);
                Gizmos.DrawLine(connect.transform.position, connect.transform.up);
            }

        }
        catch
        {
            Debug.Log("Uninstanced Connections on " + gameObject.name);
        }
    }
}
[Serializable]
public class ConnectPoint
{
    public Transform transform;
    public List<WeaponPiece.pieceType> acceptableConnections;
    [HideInInspector]
    public WeaponPiece piece;

}


