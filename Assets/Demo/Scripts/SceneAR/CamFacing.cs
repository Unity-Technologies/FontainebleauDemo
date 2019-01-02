using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CamFacing : MonoBehaviour
{
    public string FameCameraObjectName = "FirstPersonCharacter";
    public Camera GameCamera;

    void Start()
    {
        if (FameCameraObjectName != null)
            GameCamera = GameObject.Find(FameCameraObjectName).GetComponent<Camera>();
    }

	// Update is called once per frame
	void Update ()
    {
        if(GameCamera != null)
        {
            this.transform.forward = GameCamera.transform.forward;
        }

    }
}
