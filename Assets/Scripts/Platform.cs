using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Platform : MonoBehaviour {

    // stores the original (starting) position of the platform
    public Vector3 OriginalPosition { get; set; }

    public Vector3 TargetPosition { get; set; }

    // holds methods that adjust the position of the platform
    private delegate void PlatformAdjuster(Vector3 targetPosition);
    private PlatformAdjuster platformAdjuster = null;

    private Vector3 currentVelocity = Vector3.zero;

	void Start () {
        OriginalPosition = transform.position;
    }

    private void FixedUpdate()
    {
        // Need to use a delegate to run PlatformAdjuster() because SmoothDamp needs to run in FixedUpdate to work
        // Only running the platformAdjuster delegate if it contains a method
        // This is not entirely necessary, but it saves from having the method continuosly run in the background
        // even when the platform is already in its target position        
        if (platformAdjuster != null)
        {            
            platformAdjuster(TargetPosition);
        }
        
    }

    // used to adjust the position of the platform based on the targetPosition
    private void AdjustPlatform(Vector3 targetPosition)
    {
        // using smooth damp to change the position of the platform over time
        transform.position = Vector3.SmoothDamp(transform.position, targetPosition, ref currentVelocity , .05f);

        // removing the method from the platformAdjuster delegate once the platform is in its new position
        if (transform.position == targetPosition)
        {
            platformAdjuster = null;
        }
    }

    // Used to populate the platformAdjuster delegate with the AdjustPlatform method
    // and to set the TargetPosition to be just under the ground
    public void LowerPlatform(Transform ground)
    {
        // getting the y position of the ground
        float groundPosition = ground.position.y + ground.localScale.y / 2;

        TargetPosition = new Vector3(transform.position.x, groundPosition, transform.position.z);

        platformAdjuster = AdjustPlatform;
    }

    // Used to populate the platformAdjuster delegate with the AdjustPlatform method
    // and set the TargetPosition to be the original position of the platform
    public void RaisePlatform()
    {
        TargetPosition = OriginalPosition;

        platformAdjuster = AdjustPlatform;
    }
}
