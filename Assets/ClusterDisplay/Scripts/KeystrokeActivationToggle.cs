using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KeystrokeActivationToggle : MonoBehaviour
{
    public KeyCode code;
    public MonoBehaviour behaviour;

    void Update()
    {
        if (behaviour == null)
            return;
        if (Input.GetKeyDown(code))
            behaviour.enabled = !behaviour.enabled;
    }
}
