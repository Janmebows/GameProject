using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Room : MonoBehaviour {
    public static int ID = 0; //room counter
    public int thisID = 0; //value corresponding to this room
    public GameObject room; //reference to the gameobject
    public Transform[] Doorways; //stores spots where the room can link to another room (doorways)
    //Note the doorways rotations need to add to 180
    public List<int> doorRotations; //order should be the same as doorways
    public List<GameObject> connectedRooms;
    protected bool playerInRoom;
    public float rotation=0;
    public Transform center; //centre of the box the room generates
    public Transform corner; //Distance from centre to a corner

    public List<ColliderInfo> roomColliders;
    void Start()
    {
        ID += 1;
        thisID = ID;
        connectedRooms = new List<GameObject>();
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
    
    //Might add delegate functions for different rooms for better checks
    //i.e. for the T room use two check boxes - one for the vertical line, and one for the horizontal, and just take !or
    public bool NoCollisionAbstract(Vector3 boxOffset, Vector3 boxRotation, float tolerance)
    {
        //Boxoffset is where we're the room will be centered
        //Boxrotation is how the room is to be rotated
        //Tolerance is testing for how harsh the collision check should be
        Vector3 boxCenter = boxOffset;
        Vector3 halfExtents = (corner.localPosition - center.localPosition) * tolerance;
        //Returns true if there is NO collision
        return !Physics.CheckBox(boxCenter, halfExtents, Quaternion.Euler(boxRotation));

        //Quaternion boxRot = Quaternion.Euler(boxRotation);
        //Vector3 boxCenter = boxOffset + center.localPosition;

        //bool colliding = false;
        //for(int i=0; i<roomColliders.Count;i++)
        //{
        //    ColliderInfo roomCol = roomColliders[i];
        //    Vector3 halfExtents = (roomCol.corner.localPosition - roomCol.center.localPosition) * tolerance;
        //    switch (roomCol.type)
        //    {
        //        case ColliderInfo.RoomType.Sphere:
        //            //colliding = !Physics.CheckSphere(boxCenter, halfExtents, );
        //            break;
        //        case ColliderInfo.RoomType.Capsule:
        //            //colliding= !Physics.CheckCapsule(boxCenter, halfExtents, boxRot);
        //            break;
        //        case ColliderInfo.RoomType.Box:
        //            colliding = !Physics.CheckBox(boxCenter, halfExtents, boxRot);
        //            break;
        //        default:
        //            colliding = !Physics.CheckBox(boxCenter, halfExtents, boxRot);
        //            break;
        //    }
        //    if (colliding)
        //        return false;
        //}
        //return true;

    }

    [System.Serializable]
    public struct ColliderInfo
    {
        public enum RoomType
        {
            Sphere, Capsule, Box
        }
        public RoomType type;
        public Transform center;
        public Transform corner;

    }
}