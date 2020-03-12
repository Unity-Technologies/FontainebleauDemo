using System;
using UnityEngine;
using UnityEngine.Playables;

[RequireComponent(typeof(PlayableDirector))]
public class DirectorController : MonoBehaviour
{
    [SerializeField]
    float m_MaxSpeed;
    
    [SerializeField]
    float m_MinSpeed;

    [SerializeField]
    float m_SpeedDelta;
    
    PlayableDirector m_Director;
    float m_Speed;
    
    void OnEnable()
    {
        m_Director = GetComponent<PlayableDirector>();
        m_Speed = Mathf.Clamp(1, m_MinSpeed, m_MaxSpeed);
        m_Director.RebuildGraph();
        m_Director.playableGraph.GetRootPlayable(0).SetSpeed(m_Speed);
    }
    
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.R))
        {
            m_Director.time = 0;
            m_Director.Play();
            return;
        }

        var delta = 0;
        if (Input.GetKey(KeyCode.DownArrow))
            delta = -1;
        else if (Input.GetKey(KeyCode.UpArrow))
            delta = 1;
        else return;
        
        m_Speed = Mathf.Clamp(m_Speed + delta * m_SpeedDelta, m_MinSpeed, m_MaxSpeed);
        if(!m_Director.playableGraph.IsValid())
            m_Director.RebuildGraph();
        m_Director.playableGraph.GetRootPlayable(0).SetSpeed(m_Speed);
    }
}