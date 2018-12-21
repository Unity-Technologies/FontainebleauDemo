using System;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class RandomCuePlayer : MonoBehaviour
{
    public enum State
    {
        Delay,
        Playing
    }

    public CueList CueList;
    public float AwakeMaxDelay = 4.0f;
    public float MinDelay = 4.0f;
    public float MaxDelay = 15.0f;
    public bool CheckDistance = false;
    public float MinDistance = 3.0f;

    public float TTL;
    public State CurrentState;
    public AudioSource AudioSource;

    void Start()
    {
        TTL = UnityEngine.Random.Range(0, AwakeMaxDelay);
        CurrentState = State.Delay;
    }

    void OnEnable()
    {
        AudioSource = GetComponent<AudioSource>();
        RandomCueManager.manager.Register(this);
    }

    void OnDisable()
    {
        RandomCueManager.manager.DeRegister(this);
    }

    void OnDrawGizmos()
    {
        if(CheckDistance)
        {
            Gizmos.color = new Color(1,0,0,0.2f);
            Gizmos.DrawWireSphere(transform.position, MinDistance);
        }
    }

    void OnDrawGizmosSelected()
    {
        if(CheckDistance)
        {
            Gizmos.color = new Color(1,0,0,0.2f);
            Gizmos.DrawSphere(transform.position, MinDistance);
            Gizmos.color = new Color(1, 0, 0, 1);
            Gizmos.DrawWireSphere(transform.position, MinDistance);
        }
    }

    public void Delay()
    {
        TTL = UnityEngine.Random.Range(MinDelay, MaxDelay);
        CurrentState = State.Delay;
    }


}
