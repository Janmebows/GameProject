using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[AddComponentMenu("Camera-Control/Mouse Orbit with zoom")]
public class CameraController : MonoBehaviour
{
    //reference to the player to look at
    public Transform player;
    //reference to what the player is locking on to
    public Transform target;
    public float distance = 5.0f;
    public float xSpeed = 4.0f;
    public float ySpeed = 4.0f;
    public float lockOnSpeed = 1f;
    public float yMinLimit = -20f;
    public float yMaxLimit = 80f;

    public float distanceMin = .5f;
    public float distanceMax = 15f;
    private new Rigidbody rigidbody;
    bool zoomIn = false;
    float x = 0.0f;
    float y = 0.0f;
    public bool invertY = false;

    //the offset for the camera to look over right(left) shoulder
    public Vector3 offset;
    //Whether or not the player has targetted an enemy
    public bool tryLockOn;
    bool lockedOn;
    // Use this for initialization
    void Start()
    {
        Vector3 angles = transform.eulerAngles;
        x = angles.y;
        y = angles.x;

        rigidbody = GetComponent<Rigidbody>();

        // Make the rigid body not change rotation
        if (rigidbody != null)
        {
            rigidbody.freezeRotation = true;
        }
    }

    void LateUpdate()
    {
        Vector3 position;
        Quaternion rotation;
        //do we want to be locked on?
        tryLockOn = lockedOn ^ Input.GetButtonDown("XboxRightStickClick");

        //don't want to be locked on
        if (!tryLockOn)
        {
            //stop being locked on
            lockedOn = false;
            //clear the target for future use
            target = null;
        }
        //not yet locked on but we want to be
        else if (tryLockOn && !lockedOn)
        {
            //see if there are any valid targets
            //and need to find the 'best' target
            FindBestTarget();
            if (target != null)
                lockedOn = true;

        }

        if (lockedOn && target!=null) { 
            rotation = transform.rotation;
            Vector3 negDistance = new Vector3(0.0f, 0.0f, -distance);
            position = transform.rotation * negDistance + player.position + (transform.rotation * offset);
            
            Vector3 direction = target.position - position;

            //slerp to make pretty rotating effect
            rotation = Quaternion.Slerp(rotation, Quaternion.LookRotation(direction), Time.deltaTime * lockOnSpeed);
            //DO SOMETHING HERE TO STOP RETARDED ROTATY SHIT WHEN TOO CLOSE

        }
        else
        {
            x += Input.GetAxis("XboxRightHorizontal") * xSpeed * distance;
            if (!invertY)
                y -= Input.GetAxis("XboxRightVertical") * ySpeed;
            else
                y += Input.GetAxis("XboxRightVertical") * ySpeed;
            y = ClampAngle(y, yMinLimit, yMaxLimit);

            //ZoomIn(ref distance);
            rotation = Quaternion.Euler(y, x, 0);
            Vector3 negDistance = new Vector3(0.0f, 0.0f, -distance);
            //not sure about the offset term
            position = rotation * negDistance + player.position + (rotation * offset);

        }

            transform.position = position;
            transform.rotation = rotation;
        //Will need to fix this part here - currently a bit janky (hence commented)
        //RaycastHit hit;
        //if (Physics.Linecast(target.position, transform.position, out hit))
        //{
        //    distance -= hit.distance;
        //}


    }

    //true if a target was found
    //false otherwise
    //sets the target within the script
    bool FindBestTarget()
    {
        List<GameObject> validTargets = new List<GameObject>();
        //may have a script which can manage this when it does spawning, etc.
        //HAVE TO GIVE EVERY ENEMY A TAG THAT ACKNOWLEDGES THEM AS AN ENEMY
        GameObject[] enemiesInScene = GameObject.FindGameObjectsWithTag("Enemy");
        if(enemiesInScene.Length >= 1)
        {
            //check if it is a valid target
            foreach (GameObject enemy in enemiesInScene)
            {
                if (enemy.GetComponentInChildren<Renderer>().IsVisibleFrom(Camera.main))
                    validTargets.Add(enemy);
            }
            //if theres only one valid target - we lock on to that one
            if(validTargets.Count==1)
            {
                target = validTargets[0].transform;
                return true;
            }
            //if there are multiple targets pick the best
            else if(validTargets.Count >1)
            {
                target = validTargets[0].transform;
                foreach(GameObject obj in validTargets)
                    target =BestOutOfTwoValidTargets(target, obj.transform);
                return true;
            }
        }
        //no targets were found
        target = null;
        return false;
    }
    //currently just looks at distance but i want it to prioritise centredness higher than distance (but consider both)
    Transform BestOutOfTwoValidTargets(Transform target1, Transform target2)
    {
        float invLikelihood1 = Vector3.Magnitude(target1.position - transform.position);
        float invLikelihood2 = Vector3.Magnitude(target2.position - transform.position);
        if (invLikelihood1 <= invLikelihood2)
            return target1;
        else
        {
            return target2;
        }
    }

    //implements a zoomin effect
    void ZoomIn(ref float distance)
    {

        zoomIn = zoomIn ^ Input.GetButtonDown("XboxRightStickClick");

        if (zoomIn)
            distance = Mathf.Clamp(distance - 0.1f, distanceMin, distanceMax);
        else
            distance = Mathf.Clamp(distance + 0.5f, distanceMin, distanceMax);

    }

    public static float ClampAngle(float angle, float min, float max)
    {
        if (angle < -360F)
            angle += 360F;
        if (angle > 360F)
            angle -= 360F;
        return Mathf.Clamp(angle, min, max);
    }
}