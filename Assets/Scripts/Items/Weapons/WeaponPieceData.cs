using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

[CreateAssetMenu()]
public class WeaponPieceData : ScriptableObject
{
    public enum pieceQuality
    {
        awful, broken, bad, average, decent, good, amazing, overpowered
    }
    public enum pieceType
    {
        guard, handle, head, pommel, shaft
    }


    //CORE INFORMATION
    public string pieceName;
    public pieceType typeOfPiece;
    public GameObject prefab;
    public ConnectPoint[] connections;
    public Collider collider;



    //damage factors
    //possibly if top qualities add a visual effect?
    public pieceQuality quality = pieceQuality.average;
    //effects the swing speed of the weapon
    public float speedMultiplier =1f;
    //weight of the part in kg
    public float weight=0f;
    [Range(0,1)]
    public float severProportion =0.5f;
    [Range(0,1)]
    public float crushProportion =0.5f;


    //generate this based on quality (with some randomness) and then all the damage multipliers are capped based on this
    //for now it does nothing though
    private int pieceValue;

    public WeaponPieceData(pieceType type, ConnectPoint[] connectPoints, Collider _collider)
    {
        typeOfPiece = type;
        connections = connectPoints;
        collider = _collider;
    }

    public WeaponPieceData(WeaponPieceData defaultData)
    {
        //important info
        pieceName = defaultData.pieceName;
        typeOfPiece = defaultData.typeOfPiece;
        prefab = defaultData.prefab;
        collider = defaultData.collider;



        //area where we can have some randomness - 
        //let this be affected by the difficulty of the area it is made in?
        //for now just using defaults
        quality = defaultData.quality;
        speedMultiplier = defaultData.speedMultiplier;
        weight = defaultData.weight;
        severProportion = defaultData.severProportion;
        crushProportion = defaultData.crushProportion;
    }

    //

}
[Serializable]
public class ConnectPoint
{
    public Vector3 connectionPosition;
    public Vector3 connectionRotation;
    public WeaponPieceData.pieceType[] acceptableConnections;


}


