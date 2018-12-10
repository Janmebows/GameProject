using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
public class DungeonManager : MonoBehaviour
{
    //settings class for dungeon generation
    //this class will allow for randomisation - different dungeon types, etc.
    public DungeonSettings dS;

    public int roomCount;


    public List<GameObject> instantiatedRoomObjects;
    List<GameObject> deadEnds;
    List<DoorConnection> unusedDoors; //the list of doors which are unused

    public GameObject dungeonParent; //holds all the dungeon stuff to clear the hierarchy
                                     //reference for the different room types


    System.Random seed;


    void Start()
    {
        roomCount = 0;
        if (dungeonParent == null)
        {
            dungeonParent = new GameObject("Dungeon Parent");
        }

        unusedDoors = new List<DoorConnection>();
        deadEnds = new List<GameObject>();
        //initialise rng
        if (dS.useRandomSeed)
            seed = new System.Random();
        else
            seed = new System.Random(dS.CustomSeed);

        PlaceSpawn();
        StartCoroutine(PlaceRooms());
    }





    //Initialise the spawn room, and start placing rooms
    void PlaceSpawn()
    {
        GameObject instancedSpawn = Instantiate(dS.spawn, Vector3.zero, Quaternion.identity, dungeonParent.transform);
        Room instancedSpawnScr = instancedSpawn.GetComponent<Room>();
        instantiatedRoomObjects.Add(instancedSpawn);

        for (int i = 0; i < instancedSpawnScr.Doorways.Length; i++)
        {
            unusedDoors.Add(new DoorConnection(instancedSpawnScr.Doorways[i], instancedSpawn, i));
        }
        roomCount += 1;
        return;

    }
    IEnumerator AddRoom(DoorConnection prevDoorConnection, DoorConnection doorConnection, Vector3 position, Quaternion rotation)
    {
        //prevDoorConnection - previous (room + door used + index)
        //doorConnection - this (room + door used + index)
        GameObject newRoom = Instantiate(doorConnection.room, position, rotation, dungeonParent.transform);
        Room newRoomScript = newRoom.GetComponent<Room>();
        Room prevRoomScript = prevDoorConnection.room.GetComponent<Room>();
        newRoomScript.connectedRooms.Add(prevDoorConnection.room);
        prevRoomScript.connectedRooms.Add(doorConnection.room);
        unusedDoors.Remove(prevDoorConnection);
        //give the doorconnection the real values rather than prefabs
        doorConnection.door = newRoomScript.Doorways[doorConnection.doorIndex];
        doorConnection.room = newRoom;
        instantiatedRoomObjects.Add(newRoom);

        //instance the room
        //get references
        //add doorways to list
        for (int i = 0; i < newRoomScript.Doorways.Length; i++)
        {
            //if this is the door we used
            if (i == doorConnection.doorIndex)
            {
                GameObject newDoor = Instantiate(dS.door, doorConnection.door);
                newDoor.layer = LayerMask.NameToLayer("Doors");
                
            }
            else
            {
                unusedDoors.Add(new DoorConnection(newRoomScript.Doorways[i], newRoom, i));
            }
            yield return null;
        }
        //increment roomcount


        roomCount++;
        yield return null;
    }



    //Each frame attempts to place a new room
    //calls the checkbounds function, and if successful
    //runs the AddRoom script
    IEnumerator PlaceRooms()
    {
        //While we can fit more rooms, and want more rooms
        while (unusedDoors.Count > 0 && roomCount < dS.maxRooms)
        {
            DoorConnection doorConnection = null;
            Vector3 newRoomLocation = Vector3.zero;
            Vector3 newRoomRotation = Vector3.zero;
            int linkIndex = seed.Next(0, unusedDoors.Count);
            Transform linkToAttach = unusedDoors[linkIndex].door; //pick a random doorway
            GameObject newRoom = dS.roomTypes[seed.Next(0, dS.roomTypes.Count)]; //picks a random room prefab

            if (CheckBounds(newRoom, linkToAttach, out newRoomLocation, out newRoomRotation, out doorConnection))
            {
                yield return AddRoom(unusedDoors[linkIndex], doorConnection, newRoomLocation, Quaternion.Euler(newRoomRotation));


            }


            yield return null;
        }
        //seal off the remaining doorways
        //before this need to check if rooms are connected but their doorways are considered not connected
        yield return null;
        StartCoroutine(PlaceDeadEnd());
        yield return null;
    }

    //function to place deadends on completion
    //goes through all the doorways which have no connected rooms and places dead ends
    IEnumerator PlaceDeadEnd()
    {
        while (unusedDoors.Count > 0)
        {
            GameObject current = Instantiate(dS.deadEnd, unusedDoors[0].door);
            yield return null;
            current.transform.parent = unusedDoors[0].room.transform;
            deadEnds.Add(current);
            unusedDoors.RemoveAt(0);
        }
        yield return null;
        StartCoroutine(PlaceExit());

        //2 deadends with same position => replace with door
        //StartCoroutine(ReplaceDeadEndsWithDoors());

    }
    //if two doorways are conviniently next to each other but not considered connected then we do this!
    IEnumerator ReplaceDeadEndsWithDoors()
    {
        float posTolerance = 0.3f;
        int startingIndex = 0;
        while (startingIndex < deadEnds.Count-1)
        {
            startingIndex++;
            int i = startingIndex;
            while (i < deadEnds.Count)
            {
                i++;
                //if they are (almost) exactly the same
                if (Vector3.SqrMagnitude(deadEnds[startingIndex].transform.position - deadEnds[i].transform.position) < posTolerance)
                {
                    //lets not do this yet
                    //GameObject tempDoor = Instantiate(dS.door, deadEnds[startingIndex].transform);
                    //removes clutter from hierarchy
                    //tempDoor.transform.parent = deadEnds[startingIndex].transform.parent;
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

    //places an exit portal at some random room - possibly have some reward for going through this?
    //possibly make it a room to place at the end instead? 
    IEnumerator PlaceExit()
    {

        GameObject exitRoom = instantiatedRoomObjects[seed.Next(0, instantiatedRoomObjects.Count)];

        Instantiate(dS.exit, exitRoom.transform);
        yield return null;
    }



    //newRoom is the prefab of the room to try to place, previousDoor is the global position of the door it is attaching to
    bool CheckBounds(GameObject newRoom, Transform previousDoor, out Vector3 roomLocation, out Vector3 roomRotation, out DoorConnection doorConnection)
    {
        //make it null so no errors (yay)
        doorConnection = null;

        //get the room script corresponding to the prefab (gives the doorways)
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
            roomRotation -= door.localRotation.eulerAngles;

            Vector3 doorEffectiveLocalPosition = Quaternion.Euler(roomRotation) * door.localPosition;

            roomLocation -= doorEffectiveLocalPosition;
            //If the room does not collide with another room
            if (newRoomscript.NoCollisionAbstract(roomLocation, roomRotation, dS.collisionTolerance))
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


    //the location and index of a door & a reference to its room
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