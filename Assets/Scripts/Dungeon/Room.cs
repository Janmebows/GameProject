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

    public int playerRoomDistance;

    public List<ColliderInfo> roomColliders;
    void Start()
    {
        ++ID;
        thisID = ID;
        connectedRooms = new List<GameObject>();
    }


    //i.e. for the T room use two check boxes - one for the vertical line, and one for the horizontal, and just take !or
    public bool NoCollisionAbstract(Vector3 centreOffset, Vector3 roomRotation, float tolerance)
    {
        //Boxoffset is where we're the room will be centered
        //Boxrotation is how the room is to be rotated
        //Tolerance is testing for how harsh the collision check should be

        bool colliding = false;
        foreach (ColliderInfo roomCol in roomColliders)
        {
            Vector3 boxCenter = centreOffset + roomCol.center.localPosition;
            Vector3 halfExtents = (roomCol.corner.localPosition - roomCol.center.localPosition) * tolerance;
            switch (roomCol.type)
            {
                case ColliderInfo.RoomType.Sphere:
                    //colliding = !Physics.CheckSphere(boxCenter, halfExtents, );
                    Debug.Log("Sphere Room Colliders are not yet implemented");
                    break;
                case ColliderInfo.RoomType.Capsule:
                    //colliding= !Physics.CheckCapsule(boxCenter, halfExtents, boxRot);
                    Debug.Log("Capsule Room Colliders are not yet implemented");
                    break;
                case ColliderInfo.RoomType.Box:
                    colliding = !Physics.CheckBox(boxCenter, halfExtents, Quaternion.Euler(roomRotation +roomCol.rotation), ~LayerMask.NameToLayer("Doors"));
                    break;
                default:
                    Debug.LogError("You forgot to assign a collider type to " + gameObject.name);
                    break;
            }
            //if at any point it collides
            if (!colliding)
                return false;
        }
        return true;

    }

    ////Unfortunately its shit if the rotation isn't a multiple of 90 deg
    //void OnDrawGizmos()
    //{
    //    foreach(ColliderInfo roomCol in roomColliders)
    //    {
    //        switch (roomCol.type)
    //        {
    //            case ColliderInfo.RoomType.Sphere:
    //                Gizmos.DrawWireSphere(roomCol.center.position, roomCol.radius);
    //                break;
    //            case ColliderInfo.RoomType.Capsule:
    //                //Gizmos.DrawWireMesh(GameObject.CreatePrimitive(PrimitiveType.Capsule).GetComponent<Mesh>());
    //                Gizmos.DrawCube((roomCol.center.position + roomCol.secondCenter.position) / 2, gameObject.transform.rotation * roomCol.corner.localPosition * 2f);
    //                break;
    //            case ColliderInfo.RoomType.Box:
    //                //gotta multiply by 2 bc not half extents bc the unity devs are assholes who can't be consistent
    //                Gizmos.DrawWireCube(roomCol.center.position,gameObject.transform.rotation*roomCol.corner.localPosition *2f);
    //                break;
    //            default:
    //                break;
    //        }

    //    }
    //}



    [System.Serializable]
    public struct ColliderInfo
    {
        public enum RoomType
        {
            Sphere, Capsule, Box
        }
        [Tooltip("Shape of the room collider")]
        public RoomType type;
        [Tooltip("Where the room collider is centered")]
        public Transform center;
        [Tooltip("The corner of the room collider - leave blank for spheres/capsules")]
        public Transform corner;
        [Tooltip("Rotation of the collider - set to 0 for default")]
        public Vector3 rotation;
        [Tooltip("For capsule colliders - leave empty for non Capsules")]
        public Transform secondCenter;
        [Tooltip("Radius of the sphere/capsule - leave empty for Boxes")]
        public float radius;

    }
}