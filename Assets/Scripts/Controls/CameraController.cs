using UnityEngine;
using System.Collections;

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
    public bool lockOn;
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
        //this condition will have to change to support if locking on is allowed
        lockOn = lockOn ^ Input.GetButtonDown("XboxRightStickClick");

        if (lockOn)
        {
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