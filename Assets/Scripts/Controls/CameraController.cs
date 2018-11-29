using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class CameraController : MonoBehaviour
{
    //reference to the player to look at
    public Transform player;
    //reference to what the player is locking on to
    Transform target;
    private new Rigidbody rigidbody;
    public CameraSettings cameraSettings;
    float x = 0.0f;
    float y = 0.0f;

    //Whether or not the player wants to and has targeted an enemy
    bool tryLockOn = false;
    bool lockedOn = false;
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

        if (lockedOn && target != null)
        {
            if (Input.GetButtonDown("XboxLB"))
            {
                ChangeTarget(true);
            }
            else if (Input.GetButtonDown("XboxRB"))
            {
                ChangeTarget(false);
            }
            rotation = transform.rotation;
            Vector3 negDistance = new Vector3(0.0f, 0.0f, -cameraSettings.distance);
            position = transform.rotation * negDistance + player.position + (transform.rotation * cameraSettings.offset);

            Vector3 direction = target.position - position;

            //slerp to make pretty rotating effect
            rotation = Quaternion.Slerp(rotation, Quaternion.LookRotation(direction), Time.deltaTime * cameraSettings.lockOnSpeed);
            //DO SOMETHING HERE TO STOP RETARDED ROTATY SHIT WHEN TOO CLOSE

        }
        else
        {
            x += Input.GetAxis("XboxRightHorizontal") * cameraSettings.xSpeed * cameraSettings.distance;
            if (!cameraSettings.invertY)
                y -= Input.GetAxis("XboxRightVertical") * cameraSettings.ySpeed;
            else
                y += Input.GetAxis("XboxRightVertical") * cameraSettings.ySpeed;
            y = ClampAngle(y, cameraSettings.yMinLimit, cameraSettings.yMaxLimit);

            rotation = Quaternion.Euler(y, x, 0);
            Vector3 negDistance = new Vector3(0.0f, 0.0f, -cameraSettings.distance);
            //not sure about the offset term
            position = rotation * negDistance + player.position + (rotation * cameraSettings.offset);

        }

        transform.position = position;
        transform.rotation = rotation;
        //might need some code so the camera doesn't clip through things


    }

    //Finds all the enemies seen by the main camera
    void FindValidTargets(ref List<GameObject> validTargets)
    {
        //may have a script which can manage this with spawning/despawning, etc.
        //HAVE TO GIVE EVERY ENEMY A TAG THAT ACKNOWLEDGES THEM AS AN ENEMY
        GameObject[] enemiesInScene = GameObject.FindGameObjectsWithTag("Enemy");
        if (enemiesInScene.Length >= 1)
        {
            //check if it is a valid target
            foreach (GameObject enemy in enemiesInScene)
            {
                if (LiesInCamera(enemy.transform.position))
                {
                    validTargets.Add(enemy);
                }
            }
        }
    }
    //Given a Vector3, tells if it appears in camera
    bool LiesInCamera(Vector3 position)
    {
        Vector3 pt = Camera.main.WorldToViewportPoint(position);
        if (pt.x > 1 || pt.x < 0 || pt.y > 1 || pt.y < 0 || pt.z < 0)
            return false;
        else
        {
            return true;
        }
    }
    bool FindBestTarget()
    {
        List<GameObject> validTargets = new List<GameObject>();
        FindValidTargets(ref validTargets);
        //if theres only one valid target - we lock on to that one
        if (validTargets.Count == 1)
        {
            target = validTargets[0].transform;
            return true;
        }
        //if there are multiple targets pick the best
        else if (validTargets.Count > 1)
        {
            target = validTargets[0].transform;
            foreach (GameObject obj in validTargets)
                target = BestOutOfTwoValidTargets(target, obj.transform);
            return true;
        }
        //no targets were found
        target = null;
        return false;
    }

    Transform BestOutOfTwoValidTargets(Transform target1, Transform target2)
    {
        float angle1 = Vector3.Angle(target1.position - transform.position, transform.forward);
        float angle2 = Vector3.Angle(target2.position - transform.position, transform.forward);
        float dist1 = Vector3.Magnitude(target1.position - transform.position);
        float dist2 = Vector3.Magnitude(target2.position - transform.position);
        float distanceScale = 0.1f;
        //want the smallest angle and smallest distance
        float invLikelihood1 = angle1 * angle1 + (dist1 * distanceScale);
        float invLikelihood2 = angle2 * angle2 + (dist2 * distanceScale);
        if (invLikelihood1 <= invLikelihood2)
            return target1;
        else
        {
            return target2;
        }
    }

    //given a current target and a direction to go (left/right)
    void ChangeTarget(bool leftTrightF)
    {
        List<GameObject> validTargets = new List<GameObject>();
        FindValidTargets(ref validTargets);
        float currentangle = Vector3.SignedAngle(target.position - transform.position, transform.forward, Vector3.up);

        //pointless to consider the current object
        validTargets.Remove(target.gameObject);
        if (validTargets.Count >= 1)
        {
            int counter = validTargets.Count - 1;
            float bestAngle = leftTrightF ? 500 : -500;

            while (counter >= 0)
            {
                float thisAngle = Vector3.SignedAngle(validTargets[counter].transform.position - transform.position, transform.forward, Vector3.up);
                //If its to the left but less to the left than the last
                if (leftTrightF && (thisAngle > currentangle && thisAngle < bestAngle) || !leftTrightF && (thisAngle < currentangle && thisAngle > bestAngle))
                {
                    target = validTargets[counter].transform;
                    bestAngle = thisAngle;

                }

                counter--;
            }
        }
        else { return; }


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