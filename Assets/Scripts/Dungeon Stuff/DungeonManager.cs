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
    public List<Room> instantiatedRoomScripts;
    List<GameObject> deadEnds;
    List<DoorConnection> openDoors;


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

        //Get references to all the room types
        UnityEngine.Object[] prefabs = Resources.LoadAll("Dungeon", typeof(GameObject));
        deadEnd = Resources.Load("DeadEnd", typeof(GameObject)) as GameObject;
        door = Resources.Load("Door", typeof(GameObject)) as GameObject;
        spawn = Resources.Load("Spawn", typeof(GameObject)) as GameObject;
        exit = Resources.Load("DungeonEntranceExit", typeof(GameObject)) as GameObject;
        //Initialise the heirarchy
        if (DungeonParent == null)
            DungeonParent = GameObject.Find("DungeonParent");

        //fill the roomtypes array
        foreach (GameObject prefab in prefabs)
        {
            GameObject current = prefab;
            roomTypes.Add(current);
        }
        openDoors = new List<DoorConnection>();
        //initialise rng
        if (useRandomSeed)
            seed = new System.Random();
        else
            seed = new System.Random(CustomSeed);
        StartCoroutine(DungeonGen());
    }


    //Initialise the spawn room, and start placing rooms
    IEnumerator DungeonGen()
    {
        //Place first room -> add room script to instantiatedRooms -> get links -> try to place another room (check bounds)
        StartCoroutine(AddRoom(spawn, Vector3.zero, Quaternion.identity));
        yield return null;
        StartCoroutine("PlaceRooms");
        yield return null;
    }



    //Given a chosen prefab - place a room and add it to the relevant lists
    IEnumerator AddRoom(GameObject room, Vector3 position, Quaternion rotation, Transform DoorUsed = null, GameObject previousRoom = null)
    {
        GameObject current = (Instantiate(room, position, rotation));
        //current.SetActive(true); //instantiating a hidden object will still be hidden
        current.transform.parent = DungeonParent.transform;
        instantiatedRoomObjects.Add(current);
        Room currentRoomScr = current.GetComponent(typeof(Room)) as Room;
        //if we know what the previous room is, tell the two rooms that they are connected
        if (previousRoom != null)
        {
            currentRoomScr.connectedRooms.Add(previousRoom);
            Room previousRoomSrc = previousRoom.GetComponent(typeof(Room)) as Room;
            previousRoomSrc.connectedRooms.Add(current);
        }
        instantiatedRoomScripts.Add(currentRoomScr); // now i have the room script
        //Add links from that room to a list of links
        for (int i = 0; i < currentRoomScr.Doorways.Length; i++)
        {
            if (DoorUsed == null || currentRoomScr.Doorways[i].position != DoorUsed.position)
            {
                DoorConnection doorConnection = new DoorConnection(currentRoomScr.Doorways[i], current);
                yield return null;
                openDoors.Add(doorConnection);
            }
            else
            {
                //place door
                GameObject tempDoor = Instantiate(door, currentRoomScr.Doorways[i]);
                //removes clutter from hierarchy
                tempDoor.transform.parent = currentRoomScr.Doorways[i].transform;

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
        //While we want more rooms, and can fit more rooms
        while (openDoors.Count > 0 && roomCount <= maxRooms)
        {
            Transform DoorUsed = null;
            Vector3 newRoomLocation = Vector3.zero;
            Quaternion newRoomRotation = Quaternion.identity;
            int linkIndex = seed.Next(0, openDoors.Count);
            Transform linkToAttach = openDoors[linkIndex].door; //pick a random doorway
            GameObject newRoom = roomTypes[seed.Next(0, roomTypes.Count)]; //picks a random room prefab
            //newRoom = Instantiate(newRoom);
            //newRoom.SetActive(false); //so that the room doesn't appear in scene
            yield return null; // wait so the room can initialise
            if (CheckBounds(newRoom,linkToAttach, out newRoomLocation, out newRoomRotation, out DoorUsed))
            {
                yield return StartCoroutine(AddRoom(newRoom, newRoomLocation, newRoomRotation, DoorUsed, openDoors[linkIndex].room));
                openDoors.RemoveAt(linkIndex);

            }


            yield return null;
        }
        //seal off the remaining doorways
        //before this need to check if rooms are connected but their doorways are considered not connected
        StartCoroutine(PlaceDeadEnd());
        yield return null;
    }

    //will also need to find the correct rotation for this
    IEnumerator PlaceDeadEnd()
    {
        while (openDoors.Count > 0)
        {
            GameObject current = Instantiate(deadEnd, openDoors[0].door);
            current.transform.parent = openDoors[0].room.transform;
            yield return null;
            deadEnds.Add(current);
            openDoors.RemoveAt(0);
        }
        yield return null;
        StartCoroutine(PlaceExit());

    }

    IEnumerator ReplaceDeadEndsWithDoors()
    {
        float posTolerance = 0.03f;
        GameObject checkEnd = null;
        for (int startingIndex = 0; startingIndex < deadEnds.Count; startingIndex++)
        {

            for (int i = startingIndex + 1; i <= deadEnds.Count; i++)
            {
                //if they are (almost) exactly the same
                if (Mathf.Abs((deadEnds[startingIndex].transform.position - deadEnds[i].transform.position).magnitude) < posTolerance)
                {
                    GameObject tempDoor = Instantiate(door, deadEnds[startingIndex].transform);
                    //removes clutter from hierarchy
                    tempDoor.transform.parent = deadEnds[startingIndex].transform.parent;
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



    //newRoom is the prefab of the room to try to place, offset is the localposition of the door it is attaching to
    bool CheckBounds(GameObject newRoom, Transform previousDoor, out Vector3 roomLocation, out Quaternion roomRotation, out Transform DoorToUse)
    { 
        DoorToUse = null;
        //get the room script corresponding to the prefab (gives the room size & doorways)
        Room newRoomscript = newRoom.GetComponent(typeof(Room)) as Room;


        //Shift room so it is centered at the doorway
        //rotate the room and change the position of the rooms doorway accordingly
        //shift the room by the position of the doorway

        //Vector3 rotateddoor = previousRoom.eulerAngles *Quaternion.AngleAxis(,Vector3.up) * doorLocation




        //this line is correct
        //centre the room at the previous room's doorway
        roomLocation = previousDoor.position;//previousRoom.position + previousDoor.localPosition;
        //this is the rotation the new DOOR needs to have
        roomRotation =Quaternion.Euler(0,180,0) * Quaternion.Inverse(previousDoor.rotation);


        List<Transform> Doors = new List<Transform>(newRoomscript.Doorways);
        while (Doors.Count > 0)
        {
            Transform door = Doors[seed.Next(0, Doors.Count)];
            Doors.Remove(door);
            //subtract the room's angle from the rooms rotation so the oors are at 180 degrees
            roomRotation *= Quaternion.Inverse(door.localRotation);
            //the effective position of the door will be rotated by this rotation
            Vector3 doorEffectiveLocalPosition = roomRotation * door.localPosition;
            //get the new location
            roomLocation -= doorEffectiveLocalPosition;
            //If the room does not collide with another room
            if (newRoomscript.NoCollisionAbstract(roomLocation, roomRotation, roomCollisionTolerance))
            {
                DoorToUse = door;
                return true;
            }

            roomLocation += doorEffectiveLocalPosition;
            
            roomRotation *= door.localRotation;
        }
        return false;
    }


    //the location of a door and reference to its room
    class DoorConnection
    {
        public Transform door;
        public GameObject room;

        public DoorConnection(Transform _door, GameObject _room)
        {
            door = _door;
            room = _room;
        }

    }

}