using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Timeline;
using UnityEngine.Playables;

public class Director : MonoBehaviour {

    public static Director Instance;
    public static PlayableDirector director;
    public TimelineAsset SwitchToTopDown;

	// Use this for initialization
	void Awake () {
        Instance = this;
        director = GetComponent<PlayableDirector>();        
	}
	
}
