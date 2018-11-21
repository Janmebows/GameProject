using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Room : MonoBehaviour {
    public static int ID = 0; //room counter
    public int thisID = 0; //value corresponding to this room
    public GameObject room; //reference to the gameobject
    public Transform[] Doorways; //stores spots where the room can link to another room (doorways)
    //Note the doorways rotations need to add to 180
    Vector3[] points; //
    public List<GameObject> connectedRooms;
    protected bool playerInRoom;
    public List<int> doorRotations; //order should be the same as doorways
    public float rotation=0;
    public Transform center; //centre of the box the room generates
    public Transform corner; //Distance from centre to a corner
    void Start()
    {
        ID += 1;
        thisID = ID;
    }


    public void Move(Vector3 offset)
    { 
        room.transform.position += offset;

    }
    public void MoveRoom(Transform doorway,bool negate)
    {
        if (negate)
        {
            room.transform.position -= doorway.transform.localPosition;
            Rotate(doorway.transform.position, -doorway.rotation.eulerAngles.y);
        }
        else
        {
            room.transform.position += doorway.transform.localPosition;
            Rotate(doorway.transform.position, doorway.rotation.eulerAngles.y);
        }
        
    }
    public void Rotate(Vector3 link, float angle)
    {
        rotation += angle;
        room.transform.RotateAround(link, Vector3.up, angle);
    }

    public bool NoCollision(float tolerance)
    {
        Vector3 boxCenter = room.transform.position + center.localPosition;
        Vector3 halfExtents = (corner.localPosition  - center.localPosition) * tolerance;
        Quaternion boxRotation = room.transform.rotation;
        return !Physics.CheckBox(boxCenter, halfExtents);// boxRotation);
        
    }
    
    //Will add delegate functions for different rooms for better checks
    //i.e. for the T room use two check boxes - one for the vertical line, and one for the horizontal, and just take !or
    public bool NoCollisionAbstract(Vector3 boxOffset, Quaternion boxRotation, float tolerance)
    {
        //Boxoffset is where we're the room will be centered
        //Boxrotation is how the room is to be rotated
        //Tolerance is testing for how harsh the collision check should be
        Vector3 boxCenter = boxOffset;
        Vector3 halfExtents = (corner.localPosition - center.localPosition) * tolerance;
        //Returns true if there is NO collision
        return !Physics.CheckBox(boxCenter, halfExtents, boxRotation);

    }
}