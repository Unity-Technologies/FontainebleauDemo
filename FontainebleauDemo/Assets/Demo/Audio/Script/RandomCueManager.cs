using System;
using System.Collections.Generic;
using UnityEngine;

public class RandomCueManager : MonoBehaviour
{
    public class CueManagerSingleton
    {
        public List<RandomCuePlayer> Players {get { return m_players; } }
        List<RandomCuePlayer> m_players;

        public void Initialize()
        {
            m_players = new List<RandomCuePlayer>();
        }
        
        public void Register(RandomCuePlayer player)
        {
            if (!m_players.Contains(player))
                m_players.Add(player);
        }

        public void DeRegister(RandomCuePlayer player)
        {
            if (m_players.Contains(player))
                m_players.Remove(player);
        }

        public void Update(Vector3 listenerPosition)
        {
            foreach(var player in m_players)
            {
                if(player.isActiveAndEnabled)
                    UpdatePlayer(player, listenerPosition);
            }
        }


        void UpdatePlayer(RandomCuePlayer player, Vector3 listenerPosition)
        {
            if (player.CueList == null || player.CueList.AudioClips.Length == 0)
                return;

            if(player.CheckDistance)
            {
                if (Vector3.Distance(listenerPosition, player.transform.position) < player.MinDistance)
                    return;
            }

            player.TTL -= Time.deltaTime;

            if(player.TTL <= 0)
            {
                if(player.CurrentState == RandomCuePlayer.State.Delay)
                {
                    int rnd = UnityEngine.Random.Range(0, player.CueList.AudioClips.Length - 1);
                    player.AudioSource.Stop();
                    player.AudioSource.clip = player.CueList.AudioClips[rnd];
                    player.AudioSource.Play();
                    player.CurrentState = RandomCuePlayer.State.Playing;
                }
                else
                {
                    if (!player.AudioSource.isPlaying && player.CurrentState == RandomCuePlayer.State.Playing )
                        player.Delay();
                }
            }
        }
    }

    public static CueManagerSingleton manager
    {
        get
        {
            if (s_Singleton == null)
                s_Singleton = new CueManagerSingleton();
            return s_Singleton;
        }
    }
    private static CueManagerSingleton s_Singleton;

    public GameObject m_ListenerObject;

    void OnEnable()
    {
        manager.Initialize();
    }

    void Update()
    {
        manager.Update(m_ListenerObject.transform.position);
    }

}


