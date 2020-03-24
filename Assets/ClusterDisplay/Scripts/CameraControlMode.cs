using System;
using System.Collections.Generic;
using Cinemachine;
using UnityEngine;
using UnityEngine.Rendering;

[RequireComponent(typeof(CinemachineBrain))]
[RequireComponent(typeof(FreeCamera))]
[RequireComponent(typeof(CubemapCameraController))]
public class CameraControlMode : MonoBehaviour
{
    List<MonoBehaviour> m_CameraControllers = new List<MonoBehaviour>();
    int m_Index = 0;
    
    void Awake()
    {
        m_CameraControllers.Add(GetComponent<CinemachineBrain>());
        m_CameraControllers.Add(GetComponent<FreeCamera>());
        m_CameraControllers.Add(GetComponent<CubemapCameraController>());
    }

    void OnEnable()
    {
        m_Index = 0;
        SetController(m_Index);
    }

    void OnDestroy()
    {
        m_CameraControllers.Clear();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.C))
        {
            m_Index = (m_Index + 1) % m_CameraControllers.Count;
            SetController(m_Index);
        }
    }

    void SetController(int index)
    {
        for (var i = 0; i != m_CameraControllers.Count; ++i)
            m_CameraControllers[i].enabled = i == index;
    }
}
