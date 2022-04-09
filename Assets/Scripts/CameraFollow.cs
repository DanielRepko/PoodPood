using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public Transform level;
    public Transform player;

    [SerializeField]
    private float distanceFromPlayer;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void FixedUpdate()
    {   
        // moving the camera to look at and follow the player relative to the center of the level
        transform.LookAt(player);
        transform.position = player.position + Vector3.Normalize(player.position - level.position) * distanceFromPlayer;
    }
}
