using System;
using Cinemachine;
using UnityEngine;

[RequireComponent(typeof(CinemachineVirtualCamera))]
public class DollyCameraController : MonoBehaviour
{
    [SerializeField]
    CinemachineSmoothPath[] m_Paths;

    [SerializeField]
    float m_MaxSpeed;
    
    [SerializeField]
    float m_MinSpeed;

    [SerializeField]
    float m_DefaultSpeed;

    [SerializeField]
    float m_SpeedDeltaPerFrame;

    int m_CurrentPathIndex;
    float m_CurrentSpeed;
    float m_CurrentPathMinPos;
    float m_CurrentPathMaxPos;
    bool m_CurrentPathIsLooped;
    CinemachineVirtualCamera m_VCamera;
    CinemachineTrackedDolly m_TrackedDolly;

    [SerializeField]
    bool m_ShowGUI;

    // Will be removed.
    void OnGUI()
    {
        if (!m_ShowGUI)
            return;
        
        GUILayout.Label($"PathIndex [{m_CurrentPathIndex}]");
        GUILayout.Label($"PathMinPos [{m_CurrentPathMinPos}]");
        GUILayout.Label($"PathMaxPos [{m_CurrentPathMaxPos}]");
        GUILayout.Label($"PathIsLooped [{m_CurrentPathIsLooped}]");
        GUILayout.Label($"Speed [{m_CurrentSpeed}]");
        GUILayout.Label($"Position [{m_TrackedDolly.m_PathPosition}]");
    }
    
    void OnEnable()
    {
        m_CurrentSpeed = m_DefaultSpeed;    
        m_VCamera = GetComponent<CinemachineVirtualCamera>();
        m_TrackedDolly = m_VCamera.GetCinemachineComponent<CinemachineTrackedDolly>();
        Reset();
    }
    
    void Update()
    {
        {   // Select Dolly track.
            var index = -1;
            if (Input.GetKeyDown(KeyCode.Alpha1))
                index = 0;
            else if (Input.GetKeyDown(KeyCode.Alpha2))
                index = 1;
            else if (Input.GetKeyDown(KeyCode.Alpha3))
                index = 2;

            if (index != -1)
                SetPath(index);
        }
        {   // Update Camera Speed.
            var delta = 0;
            if (Input.GetKey(KeyCode.DownArrow))
                delta = -1;
            else if (Input.GetKey(KeyCode.UpArrow))
                delta = 1;

            m_CurrentSpeed = Mathf.Clamp(m_CurrentSpeed + delta * m_SpeedDeltaPerFrame, m_MinSpeed, m_MaxSpeed);
        }
        {   // Move along path (we assume the camera may only move forward)
            var pos = m_TrackedDolly.m_PathPosition + m_CurrentSpeed * Time.deltaTime;
            if (pos > m_CurrentPathMaxPos)
            {
                // Switch to next path if the current one does not loop.
                if (!m_CurrentPathIsLooped)
                    SetPath((m_CurrentPathIndex + 1) % m_Paths.Length);
                pos = m_CurrentPathMinPos;
            }
            m_TrackedDolly.m_PathPosition = pos;
        }
    }

    void SetPath(int index)
    {
        m_CurrentPathIndex = index;
        var path = m_Paths[index];
        m_TrackedDolly.m_Path = path;
        m_CurrentPathMinPos = path.MinPos;
        m_CurrentPathMaxPos = path.MaxPos;
        m_CurrentPathIsLooped = path.Looped;
    }
    
    public void Reset()
    {
        SetPath(0);
        m_TrackedDolly.m_PathPosition = m_CurrentPathMinPos;
    }
}
