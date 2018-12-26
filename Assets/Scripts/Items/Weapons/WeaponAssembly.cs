using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponAssembly : MonoBehaviour
{

    public WeaponGenerationSettings weaponGenSettings;
    //s i n g l e t o n
    public static WeaponAssembly weaponAssembly;
    System.Random seed;
    //lists of all the weapon pieces in the whole weapon
    public List<WeaponPiece> weaponPieces;
    public Weapon weaponProduced;
    Queue<ConnectPoint> openConnections;
    GameObject parent;
    bool assembling = false;
    List<WeaponPiece.pieceType> nextPieceTypes;
    ConnectPoint connect;
    public GameObject entityToGiveWeapon;
    //singleton
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
        StartCoroutine(AssembleWeapon());
    }


    public IEnumerator AssembleWeapon()
    {
        //so it doesn't try and make more weapons if its already trying (stop errors, good fun)
        //can use this class as a means of pre making weapons before they truly get spawned in
        if (assembling)
            yield break;
        else
        {
            assembling = true;
        }
        openConnections = new Queue<ConnectPoint>();
        //get all the parts that we can start with (handles)
        //holder
        parent = new GameObject("Parent");
        //add a handle (the necessary component)
        //this piece should be zeroed
        //yield return AddPiece(WeaponPiece.pieceType.handle, WeaponPiece.pieceType.noType, parent.transform);
        FirstPiece();
        //and now iterate through openConnections until there are no more connections
        while (openConnections.Count > 0)
        {
            //connect is the current connection we are using
            connect = openConnections.Dequeue();
            WeaponPiece.pieceType type;
            ChoosePieceType(out type,true);
            yield return AddPiece(type, connect.transform);
        }
        FinishWeapon();
        assembling = false;
    }

    //Way to go through piece types until end
    bool ChoosePieceType(out WeaponPiece.pieceType type, bool reset = true)
    {
        if (reset)
        {
            nextPieceTypes = new List<WeaponPiece.pieceType>(connect.acceptableConnections);

        }
        else if (nextPieceTypes.Count < 1)
        {
            type = WeaponPiece.pieceType.noType;
            return false;
        }


        int index = seed.Next(0, nextPieceTypes.Count);
        type = nextPieceTypes[index];
        nextPieceTypes.RemoveAt(index);



        return true;
    }

    void FirstPiece()
    {

        int pieceIndex = seed.Next(weaponGenSettings.handles.Count);

        GameObject newObject = Instantiate(weaponGenSettings.handles[pieceIndex], parent.transform);
        WeaponPiece pieceInfo = newObject.GetComponent<WeaponPiece>();
        weaponPieces.Add(pieceInfo);
        foreach (ConnectPoint connection in pieceInfo.connections)
        {
                openConnections.Enqueue(connection);
        }


    }

    IEnumerator AddPiece(WeaponPiece.pieceType typeToAdd, Transform offset)
    {
        List<GameObject> acceptableParts = new List<GameObject>();
        switch (typeToAdd)
        {
            case WeaponPiece.pieceType.guard:
                acceptableParts.AddRange(new List<GameObject>(weaponGenSettings.guards));
                break;
            case WeaponPiece.pieceType.handle:
                    acceptableParts.AddRange(new List<GameObject>(weaponGenSettings.handles));
                    break;
            case WeaponPiece.pieceType.head:
                    acceptableParts.AddRange(new List<GameObject>(weaponGenSettings.heads));
                    break;
            case WeaponPiece.pieceType.pommel:
                    acceptableParts.AddRange(new List<GameObject>(weaponGenSettings.pommels));
                    break;
            case WeaponPiece.pieceType.shaft:
                    acceptableParts.AddRange(new List<GameObject>(weaponGenSettings.shafts));
                    break;
            default:
                Debug.Log("Weapon Generation Failed!");
                yield break;

        }
        //this guy should solve 90% of the remaining problems
        List<ConnectPoint> acceptableConnections = new List<ConnectPoint>();

        FilterParts(ref acceptableParts,ref acceptableConnections);

        if (acceptableConnections.Count == 0)
        {
                Debug.Log("Absolutely nothing works");
                yield break;
            //try a different part to add here
        }
        else
        {
            int pieceIndex = seed.Next(acceptableConnections.Count);
            //have to invert shit as usual
            Quaternion rot =Quaternion.Euler(180,0,0)* offset.rotation * acceptableConnections[pieceIndex].transform.rotation;
            Vector3 pos = offset.position - rot*(acceptableConnections[pieceIndex].transform.position);

            GameObject newObject = Instantiate(acceptableConnections[pieceIndex].piece.prefab, pos, rot, parent.transform);
            WeaponPiece pieceInfo = newObject.GetComponent<WeaponPiece>();
            weaponPieces.Add(pieceInfo);
            foreach (ConnectPoint connection in pieceInfo.connections)
            {
                if (!(connection.transform.name == acceptableConnections[pieceIndex].transform.name))
                openConnections.Enqueue(connection);
                
            }

        }
        yield return null;
    }

    //Remove parts which do not allow for a connection of type prevConnection
    void FilterParts(ref List<GameObject> parts, ref List<ConnectPoint> acceptableConnections)
    {

        int i = parts.Count;
        while (i > 0)
        {
            i--;
            WeaponPiece info = parts[i].GetComponent<WeaponPiece>();
            List<ConnectPoint> points= info.connections;

            if(points.Count > 0)
            {
                foreach (ConnectPoint point in points)
                {
                    point.piece = info;
                    acceptableConnections.Add(point);
                }
            }
            else
            {
                parts.RemoveAt(i);

            }

        }
    }

    void FinishWeapon()
    {
        parent.name = GenerateStupidName();
        weaponProduced = parent.AddComponent<Weapon>();
        weaponProduced.colliders = parent.GetComponentsInChildren<Collider>();
        CleanMesh();
        weaponProduced.weaponName = parent.name;

    }
    void CleanMesh()
    {
        MeshFilter[] meshFilters = parent.GetComponentsInChildren<MeshFilter>();
        CombineInstance[] combine = new CombineInstance[meshFilters.Length];
        int i = 0;
        while (i < meshFilters.Length)
        {
            combine[i].mesh = meshFilters[i].sharedMesh;
            combine[i].transform = meshFilters[i].transform.localToWorldMatrix;
            meshFilters[i].gameObject.SetActive(false);
            i++;
        }
        MeshFilter thisFilter = parent.AddComponent<MeshFilter>();
        MeshRenderer renderer = parent.AddComponent<MeshRenderer>();
        thisFilter.mesh = new Mesh();
        thisFilter.mesh.CombineMeshes(combine,true,true);
        parent.gameObject.SetActive(true);
        

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



}
