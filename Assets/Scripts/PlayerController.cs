using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Timeline;
using UnityEngine.Playables;

public class PlayerController : MonoBehaviour
{

    private Rigidbody rb;

    [SerializeField]
    float _moveForce;    
    public float MoveForce
    {
        get
        {
            return _moveForce;
        }
        set { _moveForce = value; }
    }

    [SerializeField]
    float _jumpForce;
    public float JumpForce
    {
        get { return _jumpForce * 50; }
        set { _jumpForce = value; }
    }

    [SerializeField]
    float _maxSpeed;
    public float MaxSpeed
    {
        get
        {            
            return _maxSpeed;
        }
        set { _maxSpeed = value; }
    }


    public Transform poodPood;
    public Animator animator;

    private Vector3 preFreezeVelocity;

    public bool inTopDownMode;

    // used to determine if the player can switch between topdown and 3D mode
    // cannot go to 3D if over a level 6 normal platform (nor can you jump on top them)
    // cannot go to topdown if over an impassable platform
    public bool canSwitchModes;

    // this delegate is used to house multiple methods that are to be ran
    // inside of the FixedUpdate method to make various different "checks".
    // Functionally does not differ from individually calling all of the methods inside of
    // FixedUpdate, but it does keep the method less cluttered
    private delegate void CheckDelegate();
    private CheckDelegate checkDelegate = null;


    // Use this for initialization
    void Start()
    {
        rb = GetComponentInChildren<Rigidbody>();
        checkDelegate += CheckCameraToTopDown;
        checkDelegate += CheckCameraToSideScroll;
        checkDelegate += CheckMovement;
        checkDelegate += CheckCurrentPlatform;
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
        if (Input.GetKey(KeyCode.Z) && !inTopDownMode && canSwitchModes)
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
        if (Input.GetKey(KeyCode.X) && inTopDownMode && canSwitchModes)
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
                rb.AddForce(new Vector3(-MoveForce, 0, -velocityZ * 2), ForceMode.Acceleration);
            }

            // move right
            if (Input.GetKey(KeyCode.RightArrow) || Input.GetKey(KeyCode.D))
            {
                rb.AddForce(new Vector3(MoveForce, 0, -velocityZ * 2), ForceMode.Acceleration);
            }

            // move forward
            if ((Input.GetKey(KeyCode.UpArrow) || Input.GetKey(KeyCode.W)))
            {
                rb.AddForce(new Vector3(-velocityX * 2, 0, MoveForce), ForceMode.Acceleration);
            }

            // move back
            if ((Input.GetKey(KeyCode.DownArrow) || Input.GetKey(KeyCode.S)))
            {
                rb.AddForce(new Vector3(-velocityX * 2, 0, -MoveForce), ForceMode.Acceleration);
            }

            // jump
            if (Input.GetKey(KeyCode.Space) && PlayerOnGround() && !inTopDownMode)
            {
                rb.velocity = new Vector3(velocityX, 0, velocityZ);
                rb.AddForce(new Vector3(0, JumpForce, 0), ForceMode.Acceleration);
            }

            if(Mathf.Abs(velocityX) > MaxSpeed)
            {
                rb.velocity = new Vector3(MaxSpeed * rb.velocity.normalized.x, velocityY, velocityZ);
            }
                
            if(Mathf.Abs(velocityZ) > MaxSpeed)
            {
                rb.velocity = new Vector3(velocityX, velocityY, MaxSpeed * rb.velocity.normalized.z);
            }
        }
    }

    // Checks what kind of platform the player is on
    public void CheckCurrentPlatform()
    {
        Ray ray = new Ray();
        ray.origin = poodPood.position;
        ray.direction = new Vector3(0, -1, 0);

        Debug.DrawRay(ray.origin, new Vector3(0, -10, 0), Color.blue);

        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, 10, 1 << LayerMask.NameToLayer("Platform"), QueryTriggerInteraction.Ignore))
        {
            if(hit.transform.CompareTag("Impassable"))
            {
                canSwitchModes = false;
            }
            else if(hit.transform.CompareTag("Passable") && hit.transform.parent.name.EndsWith("6"))
            {
                canSwitchModes = false;
            }
            else
            {
                canSwitchModes = true;
            }
        }
        else
        {
            canSwitchModes = true;
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

        if(Physics.SphereCast(groundRay, 0.4f, 0.07f, (1 << LayerMask.NameToLayer("Ground") | 1 << LayerMask.NameToLayer("Platform")), QueryTriggerInteraction.Ignore))
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    void OnDrawGizmosSelected()
    {
        // Draw a yellow sphere at the transform's position
        Gizmos.color = Color.blue;
        Gizmos.DrawSphere(new Vector3(poodPood.position.x, poodPood.position.y - .07f, poodPood.position.z), .4f);
    }

}
