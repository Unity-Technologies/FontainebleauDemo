using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EscKeyToMenu : MonoBehaviour
{
    public VideoManager m_VideoManager;

	// Update is called once per frame
	void Update ()
    {
        if (Input.GetButtonDown("Cancel") && m_VideoManager != null)
            m_VideoManager.SwitchDemoMode(VideoManager.VideoMode.Menu);
		
	}
}
