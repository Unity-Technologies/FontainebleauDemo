using GameplayIngredients;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CamFacing : MonoBehaviour
{
	// Update is called once per frame
	void Update ()
    {
        if(Manager.Has<VirtualCameraManager>())
        {
            transform.forward = Manager.Get<VirtualCameraManager>().Camera.transform.forward;
        }
    }
}
