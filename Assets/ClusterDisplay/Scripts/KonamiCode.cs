using System;
using UnityEngine;

public class KonamiCode : MonoBehaviour
{
    [SerializeField]
    GameObject m_TargetObject;
    
    [SerializeField]
    string m_Code;
    
    int m_Index;

    void OnEnable() { m_Index = 0; }

    void Update()
    {
        if (m_TargetObject == null)
            return;
        
        foreach (var c in Input.inputString)
        {
            m_Index = c == m_Code[m_Index] ? m_Index + 1 : 0;

            if (m_Index == m_Code.Length - 1)
            {
                m_TargetObject.SetActive(!m_TargetObject.activeSelf);
                m_Index = 0;
            }
        }
    }
}