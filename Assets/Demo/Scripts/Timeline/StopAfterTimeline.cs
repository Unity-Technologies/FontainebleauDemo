using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;
#if UNITY_EDITOR
using UnityEditor;

public class StopAfterTimeline : MonoBehaviour {

    private PlayableDirector timeline;
    public float stopTime;

    // Use this for initialization
    void Start () {
        timeline = GetComponent<PlayableDirector>();
	}
	
	// Update is called once per frame
	void Update ()
    {
		if (timeline.time >= timeline.duration || timeline.time > stopTime)
        {
            EditorApplication.isPlaying = false;
        }
	}
}
#endif