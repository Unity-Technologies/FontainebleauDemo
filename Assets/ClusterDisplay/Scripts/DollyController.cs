using System;
using Cinemachine;
using UnityEngine;

[RequireComponent(typeof(CinemachineDollyCart))]
public class DollyController : MonoBehaviour
{
    [SerializeField]
    float m_MaxSpeed;
    
    [SerializeField]
    float m_MinSpeed;

    [SerializeField]
    float m_SpeedDeltaPerFrame;
    
    CinemachineDollyCart m_TrackedDolly;

    void OnEnable()
    {
        m_TrackedDolly = GetComponent<CinemachineDollyCart>();
        m_TrackedDolly.m_Position = 0;
        m_TrackedDolly.m_Speed = Mathf.Clamp(m_TrackedDolly.m_Speed, m_MinSpeed, m_MaxSpeed);
    }
    
    void Update()
    {
        var delta = 0;
        if (Input.GetKey(KeyCode.DownArrow))
            delta = -1;
        else if (Input.GetKey(KeyCode.UpArrow))
            delta = 1;

        m_TrackedDolly.m_Speed = Mathf.Clamp(m_TrackedDolly.m_Speed + delta * m_SpeedDeltaPerFrame, m_MinSpeed, m_MaxSpeed);
    }
}