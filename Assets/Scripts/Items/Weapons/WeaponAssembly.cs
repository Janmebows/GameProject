using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponAssembly : MonoBehaviour
{

    //This guy is here to make shit work please ignore <3
    public ConnectPoint[] connectPoints;

    //s i n g l e t o n
    public static WeaponAssembly weaponAssembly;
    System.Random seed;
    //lists of all the weapon types
    public List<WeaponPieceData> weaponPieces;
    public Weapon weaponProduced;
    Queue<ConnectPoint> openConnections;
    GameObject parent;
    bool assembling;

    void Awake()
    {
        if (weaponAssembly == null)
        {
            weaponAssembly = this;
        }
        else if (weaponAssembly != this)
        {
            Destroy(this);
        }
    }
    void Start()
    {
        assembling = false;
        seed = LevelInformation.levelInfo.rng;

        //StartCoroutine(CallGenerateStupidName());
    }


    public IEnumerator AssembleWeapon()
    {
        if (assembling)
            yield break;
        else
        {
            assembling = true;
        }
        openConnections = new Queue<ConnectPoint>();
        //get all the parts that we can start with (handles)
        parent = new GameObject("Parent");
        AddPiece(WeaponPieceData.pieceType.handle, Vector3.zero, Vector3.zero);
        yield return null;
        while (openConnections.Count > 0)
        {
            ConnectPoint connect = openConnections.Dequeue();
            int pieceType = seed.Next(0, connect.acceptableConnections.Length);
            AddPiece(connect.acceptableConnections[pieceType], connect.connectionPosition, connect.connectionRotation);
        }
        assembling = false;
    }
    void AddPiece(WeaponPieceData.pieceType typeToAdd, Vector3 positionOffset, Vector3 rotationOffset)
    {
        List<WeaponPieceData> acceptableParts = weaponPieces.FindAll(x => x.typeOfPiece == WeaponPieceData.pieceType.handle);
        int pieceIndex = seed.Next(0, acceptableParts.Count);
        WeaponPieceData newPiece = new WeaponPieceData(acceptableParts[pieceIndex]);
        weaponPieces.Add(newPiece);
        foreach (ConnectPoint connection in acceptableParts[pieceIndex].connections)
        {
            openConnections.Enqueue(connection);
        }
        Instantiate(newPiece.prefab, positionOffset, Quaternion.Euler(rotationOffset), parent.transform);

    }


    IEnumerator CallGenerateStupidName()
    {
        while (true)
        {
            yield return new WaitForSeconds(1f);
            Debug.Log(GenerateStupidName());
        }
    }

    //Will eventually make this so that it names based on parts but for now this is fun
    string GenerateStupidName()
    {
        string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
        int nameLength = seed.Next(5, 20);
        char[] stringChars = new char[nameLength];

        for (int i = 0; i < stringChars.Length; i++)
        {
            stringChars[i] = chars[seed.Next(chars.Length)];
        }

        return new string(stringChars);
    }


    private void OnDrawGizmos()
    {
        Gizmos.color = Color.cyan;
        foreach (ConnectPoint connect in connectPoints)
        {
            Gizmos.DrawCube(connect.connectionPosition, Vector3.one * 0.01f);
        }
    }

}
