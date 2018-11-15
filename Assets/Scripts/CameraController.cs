using UnityEngine;
using System.Collections;

[AddComponentMenu("Camera-Control/Mouse Orbit with zoom")]
public class CameraController: MonoBehaviour
{

    public Transform target;
    public float distance = 5.0f;
    public float xSpeed = 120.0f;
    public float ySpeed = 120.0f;

    public float yMinLimit = -20f;
    public float yMaxLimit = 80f;

    public float distanceMin = .5f;
    public float distanceMax = 15f;
    private Rigidbody rigidbody;

    //the offset for the camera to look over right(left) shoulder
    public Vector3 offset;
    //Whether or not the player has targetted an enemy
    private bool lockOn;
    bool zoomIn = false;
    float x = 0.0f;
    float y = 0.0f;
    public AnimationCurve animCurve;
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
        if (target)
        {
            x += Input.GetAxis("XboxRightHorizontal") * xSpeed * distance * 0.02f;
            y -= Input.GetAxis("XboxRightVertical") * ySpeed * 0.02f;

            y = ClampAngle(y, yMinLimit, yMaxLimit);

            Quaternion rotation = Quaternion.Euler(y, x, 0);
            //zoom on first click, zoomout on second 
            // ^ is XOR
            zoomIn = zoomIn ^ Input.GetButtonDown("XboxRightStickClick");

            if (zoomIn)
                distance = Mathf.Clamp(distance - 0.1f, distanceMin, distanceMax);
            else
                distance = Mathf.Clamp(distance + 0.5f, distanceMin, distanceMax);
            RaycastHit hit;
            if (Physics.Linecast(target.position, transform.position, out hit))
            {
                distance -= hit.distance;
            }
            Vector3 negDistance = new Vector3(0.0f, 0.0f, -distance);
            //not sure about the offset term
            Vector3 position = rotation * negDistance + target.position + (rotation*offset);

            transform.rotation = rotation;
            transform.position = position;
        }
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