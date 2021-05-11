using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlatformManager : MonoBehaviour {

    public static PlatformManager Platforms;

    public static Transform GroundTransform;
    public Transform ground;


    private void Start()
    {
        Platforms = this;
        GroundTransform = ground;
    }

    public static void LowerPlatforms()
    {
        Platform[] platforms = Platforms.GetComponentsInChildren<Platform>();

        foreach(Platform platform in platforms)
        {
            platform.LowerPlatform(GroundTransform);
        }
    }

    public static void RaisePlatforms()
    {
        Platform[] platforms = Platforms.GetComponentsInChildren<Platform>();

        foreach (Platform platform in platforms)
        {
            platform.RaisePlatform();
        }
    }

    private void FixedUpdate()
    {
        
    }
    
}
