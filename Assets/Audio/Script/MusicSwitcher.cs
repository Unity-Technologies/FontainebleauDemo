using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MusicSwitcher : MonoBehaviour {

    public AudioSource[] AmbienceAudioSources;

    public float TransitionDuration = 1.0f;
    public int CurrentAmbience;

    // Use this for initialization
    void Start () {

		for(int i = 0; i < AmbienceAudioSources.Length; i++)
        {
            if (i == CurrentAmbience)
                AmbienceAudioSources[i].volume = 1.0f;
            else
                AmbienceAudioSources[i].volume = 0.0f;
        }
	}
	
	// Update is called once per frame
	void Update ()
    {
		for(int i = 0; i < AmbienceAudioSources.Length; i++)
        {
            if (i == CurrentAmbience)
                AmbienceAudioSources[i].volume = Mathf.Min(1.0f, AmbienceAudioSources[i].volume + (Time.deltaTime / TransitionDuration));
            else
                AmbienceAudioSources[i].volume = Mathf.Max(0.0f, AmbienceAudioSources[i].volume - (Time.deltaTime / TransitionDuration));
        }
	}
}
