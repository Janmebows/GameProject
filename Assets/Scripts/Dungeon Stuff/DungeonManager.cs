using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
public class DungeonManager : MonoBehaviour
{
    [Range(1, 1000)]
    public int maxRooms;
    int roomCount;
    [Range(0, 1f)]
    public float roomCollisionTolerance;


    //Information about the spawned rooms
    public List<GameObject> roomTypes = new List<GameObject>();
    public List<GameObject> instantiatedRoomObjects;
    List<GameObject> deadEnds;
    List<DoorConnection> unusedDoors; //the list of doors which are unused

    public GameObject DungeonParent; //holds all the dungeon stuff to clear the hierarchy
                                     //reference for the different room types
    public GameObject deadEnd;
    public GameObject door;
    public GameObject exit;
    public GameObject spawn;


    //Random number generation stuff
    public bool useRandomSeed;
    System.Random seed;
    public int CustomSeed;



    void Start()
    {
        roomCount = 1; //1 to account for the spawn room
        FillPrefabs();

        unusedDoors = new List<DoorConnection>();
        deadEnds = new List<GameObject>();
        //initialise rng
        if (useRandomSeed)
            seed = new System.Random();
        else
            seed = new System.Random(CustomSeed);

        PlaceSpawn();
        StartCoroutine(PlaceRooms());
    }

    void FillPrefabs()
    {
        //Get references to all the room types
        UnityEngine.Object[] prefabs = Resources.LoadAll("Constructs/Dungeon/General", typeof(GameObject));
        deadEnd = Resources.Load("Constructs/Dungeon/DeadEnd", typeof(GameObject)) as GameObject;
        door = Resources.Load("Constructs/Dungeon/Door", typeof(GameObject)) as GameObject;
        spawn = Resources.Load("Constructs/Dungeon/Spawn", typeof(GameObject)) as GameObject;
        exit = Resources.Load("Constructs/Dungeon/DungeonEntranceExit", typeof(GameObject)) as GameObject;
        //Initialise the heirarchy
        if (DungeonParent == null)
            DungeonParent = GameObject.Find("DungeonParent");

        //fill the roomtypes array
        foreach (GameObject prefab in prefabs)
        {
            roomTypes.Add(prefab);
        }
    }
    //Initialise the spawn room, and start placing rooms



    void PlaceSpawn()
    {
        GameObject instancedSpawn = Instantiate(spawn, Vector3.zero, Quaternion.identity, DungeonParent.transform);
        Room instancedSpawnScr = instancedSpawn.GetComponent<Room>();
        instantiatedRoomObjects.Add(instancedSpawn);

        for (int i = 0; i < instancedSpawnScr.Doorways.Length; i++)
        {
            unusedDoors.Add(new DoorConnection(instancedSpawnScr.Doorways[i], instancedSpawn, i));
        }
        roomCount += 1;
        return;

    }
    IEnumerator AddRoom(DoorConnection prevRoomDoorConnection, DoorConnection doorConnection, Vector3 position, Quaternion rotation)
    {
        //Given a chosen prefab - place a room and add it to the relevant lists
        //prevRoomDoorConnection is the door/index/room of the NEW room
        //doorconnection         is the door/index/room of the NEW room
        //position + rotation make the transform of the room


        //initialise the room
        GameObject current = (Instantiate(doorConnection.room, position, rotation, DungeonParent.transform));
        //replace the temp room with the actual instance
        doorConnection.room = current;
        instantiatedRoomObjects.Add(current);
        yield return null;
        Room currentRoomScr = current.GetComponent<Room>();
        //tell them they are connected
        currentRoomScr.connectedRooms.Add(doorConnection.room);
        Room previousRoomSrc = prevRoomDoorConnection.room.GetComponent<Room>();
        previousRoomSrc.connectedRooms.Add(current);

        for (int i = 0; i < currentRoomScr.Doorways.Length; i++)
        {
            //if looking at new unused doors
            if (i != doorConnection.doorIndex)
            {
                DoorConnection newDoor = new DoorConnection(currentRoomScr.Doorways[i], doorConnection.room, i);
                unusedDoors.Add(newDoor);
            }
            //the one which is actually connected
            else
            {
                Instantiate(door, doorConnection.door);
                unusedDoors.Remove(prevRoomDoorConnection);
            }
        }
        
        roomCount += 1;
        yield return null;

    }
    


//Each frame attempts to place a new room
//calls the checkbounds function, and if successful
//runs the AddRoom script
IEnumerator PlaceRooms()
{
    //While we can fit more rooms, and want more rooms
    while (unusedDoors.Count > 0 && roomCount <= maxRooms)
    {
        DoorConnection doorConnection = null;
        Vector3 newRoomLocation = Vector3.zero;
        Vector3 newRoomRotation = Vector3.zero;
        int linkIndex = seed.Next(0, unusedDoors.Count);
        Transform linkToAttach = unusedDoors[linkIndex].door; //pick a random doorway
        GameObject newRoom = roomTypes[seed.Next(0, roomTypes.Count)]; //picks a random room prefab

        if (CheckBounds(newRoom, linkToAttach, out newRoomLocation, out newRoomRotation, out doorConnection))
        {
            yield return AddRoom(unusedDoors[linkIndex], doorConnection, newRoomLocation, Quaternion.Euler(newRoomRotation));
            //openDoors.RemoveAt(linkIndex);

        }


        yield return null;
    }
    //seal off the remaining doorways
    //before this need to check if rooms are connected but their doorways are considered not connected
    StartCoroutine(PlaceDeadEnd());
    yield return null;
}

IEnumerator PlaceDeadEnd()
{
    while (unusedDoors.Count > 0)
    {
        GameObject current = Instantiate(deadEnd, unusedDoors[0].door);
        yield return null;
        current.transform.parent = unusedDoors[0].room.transform;
        deadEnds.Add(current);
        unusedDoors.RemoveAt(0);
    }
    yield return null;
    StartCoroutine(PlaceExit());
    StartCoroutine(ReplaceDeadEndsWithDoors());

}
//if two door are conviniently next to each other but not considered connected then we do this!
IEnumerator ReplaceDeadEndsWithDoors()
{
    float posTolerance = 0.3f;
    int startingIndex = 0;
    Debug.Log(deadEnds.Count);
    while (startingIndex < deadEnds.Count)
    {
        startingIndex++;
        int i = startingIndex;
        while (i <= deadEnds.Count)
        {
            i++;
            //if they are (almost) exactly the same
            if (Vector3.SqrMagnitude(deadEnds[startingIndex].transform.position - deadEnds[i].transform.position) < posTolerance)
            {
                GameObject tempDoor = Instantiate(door, deadEnds[startingIndex].transform);
                //removes clutter from hierarchy
                tempDoor.transform.parent = deadEnds[startingIndex].transform.parent;
                GameObject temp = deadEnds[startingIndex];
                deadEnds.RemoveAt(startingIndex);
                Destroy(temp);

                //i is always greater than starting index so the -1 is necessary
                temp = deadEnds[i - 1];
                Debug.Log(temp.transform.position.ToString());
                deadEnds.RemoveAt(i - 1);
                Destroy(temp);
            }
            yield return null;
        }
    }

    yield return null;
}
IEnumerator PlaceExit()
{

    GameObject exitRoom = instantiatedRoomObjects[seed.Next(0, instantiatedRoomObjects.Count)];

    Instantiate(exit, exitRoom.transform);
    yield return null;
}



//newRoom is the prefab of the room to try to place, previousDoor is the global position of the door it is attaching to
bool CheckBounds(GameObject newRoom, Transform previousDoor, out Vector3 roomLocation, out Vector3 roomRotation, out DoorConnection doorConnection)
{
    doorConnection = null;
    //get the room script corresponding to the prefab (gives the room size & doorways)
    Room newRoomscript = newRoom.GetComponent(typeof(Room)) as Room;

    roomLocation = previousDoor.position;

    roomRotation = previousDoor.rotation.eulerAngles + new Vector3(0, 180, 0);


    List<Transform> Doors = new List<Transform>(newRoomscript.Doorways);

    //create a list of indices
    List<int> indexList = new List<int>();
    for (int i = 0; i < Doors.Count; i++)
    {
        indexList.Add(i);
    }
    while (indexList.Count > 0)
    {
        int index = seed.Next(0, indexList.Count);
        Transform door = Doors[indexList[index]];
        //subtract the room's angle from the rooms rotation so the doors are at 180 degrees
        roomRotation -= door.localRotation.eulerAngles;
        //the effective position of the door will be rotated by this rotation
        Vector3 doorEffectiveLocalPosition = Quaternion.Euler(roomRotation) * door.localPosition;
        //get the new location
        roomLocation -= doorEffectiveLocalPosition;
        //If the room does not collide with another room
        if (newRoomscript.NoCollisionAbstract(roomLocation, roomRotation, roomCollisionTolerance))
        {
            doorConnection = new DoorConnection(door, newRoom, indexList[index]);
            return true;
        }

        roomLocation += doorEffectiveLocalPosition;

        roomRotation += door.localRotation.eulerAngles;


        indexList.RemoveAt(index);
    }
    return false;
}


//the location and index of a door and reference to its room
class DoorConnection
{
    public Transform door;
    public GameObject room;
    public int doorIndex;

    public DoorConnection(Transform _door, GameObject _room, int _doorIndex)
    {
        door = _door;
        room = _room;
        doorIndex = _doorIndex;
    }

}

}