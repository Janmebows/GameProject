using UnityEngine;
using System.Collections;

// The GameObject is made to bounce using the space key.
// Also the GameOject can be moved forward/backward and left/right.
// Add a Quad to the scene so this GameObject can collider with a floor.

public class PlayerMovement : MonoBehaviour
{
    float speed;
    bool sprinting;
    public float walkSpeed = 6.0f;
    public float sprintSpeed = 10f;
    public bool toggleSprint = true;
    public float jumpSpeed = 8.0f;
    public float gravity = 9.8f;
    [Range(0,1)]
    public float rotateSpeed =0.3f;
    private Vector3 moveDirection = Vector3.zero;
    private CharacterController controller;
    public Camera camera;
    public GameObject player;
    public float inputAngle=0;
    Animator anim;
    void Start()
    {
        controller = GetComponent<CharacterController>();
        player = transform.gameObject;
        anim = GetComponent<Animator>();
    }

    void Update()
    {
        //inputs
        float inputx = Input.GetAxis("Horizontal");
        float inputy = Input.GetAxis("Vertical");
        if (toggleSprint)
            sprinting = sprinting ^ Input.GetButtonDown("XboxLeftStickClick");
        else
            sprinting = Input.GetButton("XboxLeftStickClick");
        float inputMag = Mathf.Sqrt(Mathf.Pow(inputx,2f)+ Mathf.Pow(inputy, 2f));
        if (inputMag > 0)
        {
            inputAngle = InputToRotation(inputx, inputy);
            //convert the input angle with respect to the camera
            inputAngle += camera.transform.rotation.eulerAngles.y;

            anim.SetBool("isMoving", true);
        }
        else
        {
            anim.SetBool("isMoving", false);
            sprinting = false;
        }
        if (sprinting)
        {
            anim.SetBool("sprinting", true);
            speed = sprintSpeed;
        }
        else
        {
            anim.SetBool("sprinting", false);
            speed = walkSpeed;
        }
        float yrot = Mathf.LerpAngle(player.transform.rotation.eulerAngles.y, inputAngle, rotateSpeed);
        player.transform.rotation = Quaternion.Euler(0, yrot, 0);
        //if the player is on the ground
        if (controller.isGrounded)
        {
            anim.SetBool("grounded", true);
            moveDirection = player.transform.forward * inputMag* speed;

            if (Input.GetButtonDown("XboxA"))
            {
                moveDirection.y = jumpSpeed;
            }
        }
        else
            anim.SetBool("grounded", false);
        // Apply gravity
        moveDirection.y = moveDirection.y - (gravity * Time.deltaTime);
        // Move the controller
        controller.Move(moveDirection * Time.deltaTime);
    }


    //convert 2D input into a rotation
    float InputToRotation(float x, float y)
    {
        float angle = 0f;
        if (x != 0.0f || y != 0.0f)
        {
            angle = 90 - Mathf.Atan2(y, x) * Mathf.Rad2Deg;
            //if (angle < 0)
            //    angle += 360;

        }
        return angle;
    }

}