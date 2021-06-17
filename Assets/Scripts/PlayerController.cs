using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Timeline;
using UnityEngine.Playables;

public class PlayerController : MonoBehaviour
{

    private Rigidbody rb;
    public float moveSpeed = 6;
    public float jumpForce = 6;
    public Transform poodPood;
    public Animator animator;

    private Vector3 preFreezeVelocity;

    public bool inTopDownMode;

    // this delegate is used to house multiple methods that are to be ran
    // inside of the FixedUpdate method to make various different "checks".
    // Functionally does not differ from individually calling all of the methods inside of
    // FixedUpdate, but it does keep the method less cluttered
    private delegate void CheckDelegate();
    private CheckDelegate checkDelegate = null;


    // Use this for initialization
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        checkDelegate += CheckCameraToTopDown;
        checkDelegate += CheckCameraToSideScroll;
        checkDelegate += CheckMovement;
    }

    private void FixedUpdate()
    {
        // contains multiple miscellaneous methods that perform different kinds of "checks" on the player
        checkDelegate();
    }

    public void FreezeTime()
    {
        GameManager.Instance.TimeStopped = true;
        AccountForTimeState();
    }

    public void UnFreezeTime()
    {
        GameManager.Instance.TimeStopped = false;
        AccountForTimeState();
    }

    public void RaisePlatforms()
    {
        PlatformManager.RaisePlatforms();
    }

    public void LowerPlatforms()
    {
        PlatformManager.LowerPlatforms();
    }

    // used to check whether time is "stopped"
    // if so, freezes the character in their current position until time is resumed
    // must do this instead of manipulating the time scale because other objects need
    // to be able to move while the player is frozen
    public void AccountForTimeState()
    {
        if (GameManager.Instance.TimeStopped)
        {
            // storing the velocity of the player before time was stopped
            preFreezeVelocity = rb.velocity;
            rb.velocity = Vector3.zero;
            rb.useGravity = false;
        }
        else
        {
            // giving the player the velocity that they had before time stopped
            rb.velocity = preFreezeVelocity;
            rb.useGravity = true;
        }
    }

    // checks if the player has input to switch to topdown mode
    public void CheckCameraToTopDown()
    {
        if (Input.GetKey(KeyCode.Z) && !inTopDownMode)
        {
            FreezeTime();
            inTopDownMode = true;
            animator.SetBool("InTopDownMode", true);
            animator.SetBool("InSideScrollMode", false);            
        }
    }

    // checks if the player has input to switch to sidescroll mode
    public void CheckCameraToSideScroll()
    {
        if (Input.GetKey(KeyCode.X) && inTopDownMode)
        {
            FreezeTime();
            inTopDownMode = false;
            animator.SetBool("InTopDownMode", false);
            animator.SetBool("InSideScrollMode", true);
        }
    }   

    // checks if the player is pressing the movement (including jump) keys and moves them accordingly
    public void CheckMovement()
    {
        // if time is "stopped" player should not be able to move
        if (!GameManager.Instance.TimeStopped)
        {
            // creating variables to easily access the rigidbody's velocity on each axis
            float velocityX = rb.velocity.x;
            float velocityY = rb.velocity.y;
            float velocityZ = rb.velocity.z;

            // move left
            if (Input.GetKey(KeyCode.LeftArrow) || Input.GetKey(KeyCode.A))
            {
                rb.velocity = new Vector3(-moveSpeed, velocityY, 0);
            }

            // move right
            if (Input.GetKey(KeyCode.RightArrow) || Input.GetKey(KeyCode.D))
            {
                rb.velocity = new Vector3(moveSpeed, velocityY, 0);
            }

            // move forward
            if ((Input.GetKey(KeyCode.UpArrow) || Input.GetKey(KeyCode.W)) && inTopDownMode)
            {
                rb.velocity = new Vector3(0, velocityY, moveSpeed);
            }

            // move back
            if ((Input.GetKey(KeyCode.DownArrow) || Input.GetKey(KeyCode.S)) && inTopDownMode)
            {
                rb.velocity = new Vector3(0, velocityY, -moveSpeed);
            }

            // jump
            if (Input.GetKey(KeyCode.Space) && PlayerOnGround() && !inTopDownMode)
            {
                rb.velocity = new Vector3(velocityX, jumpForce, velocityZ);
            }

            // make the player's velocity zero when no keys are being pressed
            if (!Input.GetKey(KeyCode.RightArrow) && !Input.GetKey(KeyCode.LeftArrow) && 
                !Input.GetKey(KeyCode.UpArrow) && !Input.GetKey(KeyCode.DownArrow) && 
                !Input.GetKey(KeyCode.S) && !Input.GetKey(KeyCode.W) && 
                !Input.GetKey(KeyCode.D) && !Input.GetKey(KeyCode.A) && 
                !Input.GetKey(KeyCode.Space))
            {
                rb.velocity = new Vector3(0, velocityY, 0);
            }
        }
    }

    // Lowers the player down to ground level
    // Used to make sure the player is on the ground
    // after switching view modes
    public void BringPlayerToGround()
    {
        Ray groundRay = new Ray();
        groundRay.origin = poodPood.position;
        groundRay.direction = new Vector3(0, -1, 0);

        Debug.DrawRay(groundRay.origin, new Vector3(0, -10, 0), Color.green);

        RaycastHit hit;

        if (Physics.Raycast(groundRay, out hit, 10, 1 << LayerMask.NameToLayer("Ground"), QueryTriggerInteraction.Ignore))
        {
            Transform ground = hit.collider.GetComponentInParent<Transform>();
            transform.position = new Vector3(transform.position.x,
                                             ground.position.y + ground.localScale.y / 2 + poodPood.localScale.y / 2,
                                             transform.position.z);
        }
    }

    // checks if there is a platform below the player and raises them to the full height of the platform
    public void RaisePlayerToPlatformHeight()
    {
        Ray platformRay = new Ray();
        platformRay.origin = poodPood.position;
        platformRay.direction = new Vector3(0, -1, 0);

        Debug.DrawRay(platformRay.origin, new Vector3(0, -3, 0), Color.green);

        RaycastHit hit;

        if (Physics.Raycast(platformRay, out hit, 3, 1 << LayerMask.NameToLayer("Platform"), QueryTriggerInteraction.Ignore))
        {
            Platform platform = hit.collider.GetComponentInParent<Platform>();
            transform.position = new Vector3(transform.position.x,
                                             platform.OriginalPosition.y + poodPood.localScale.y / 2,
                                             transform.position.z);
        }
    }

    public bool PlayerOnGround()
    {
        Ray groundRay = new Ray();
        groundRay.origin = poodPood.position;
        groundRay.direction = new Vector3(0, -1, 0);

        Debug.DrawRay(groundRay.origin, new Vector3(0, -0.5f, 0), Color.green);

        if(Physics.Raycast(groundRay, 0.5f, 1 << LayerMask.NameToLayer("Ground"), QueryTriggerInteraction.Ignore) || Physics.Raycast(groundRay, 0.5f, 1 << LayerMask.NameToLayer("Platform"), QueryTriggerInteraction.Ignore))
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    
}
