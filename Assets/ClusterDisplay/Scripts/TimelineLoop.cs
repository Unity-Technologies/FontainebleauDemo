using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;

[RequireComponent(typeof(PlayableDirector))]
public class TimelineLoop : MonoBehaviour
{
    PlayableDirector m_Director;

    [SerializeField]
    float m_Start;
    [SerializeField]
    float m_Duration;
    
    void OnEnable()
    {
        m_Director = GetComponent<PlayableDirector>();
        StartCoroutine(Loop());
    }

    void OnDisable()
    {
        StopAllCoroutines();
    }

    IEnumerator Loop()
    {
        for (;;)
        {
            m_Director.time = m_Start;
            yield return  new WaitForSeconds(m_Duration);
        }
    }
}
