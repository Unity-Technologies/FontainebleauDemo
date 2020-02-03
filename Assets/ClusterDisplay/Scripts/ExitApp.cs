using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExitApp : MonoBehaviour
{
    void Update()
    {
        if (Input.GetKey(KeyCode.LeftControl) && Input.GetKeyDown(KeyCode.Q))
        {
#if UNITY_EDITOR    
            UnityEditor.EditorApplication.ExitPlaymode();
#else    
            Application.Quit();
#endif
        }
    }
}
