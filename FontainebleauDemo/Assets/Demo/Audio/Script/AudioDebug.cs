using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AudioDebug : MonoBehaviour {

    public MusicSwitcher MusicSwitcher;
    public Text m_text;
#if UNITY_EDITOR
    private List<string> m_Lines;
#endif
    public bool Enabled;
    public GameObject UIRoot;

	// Use this for initialization
	void Start ()
    {
#if UNITY_EDITOR
        m_Lines = new List<string>();
#endif
    }
#if UNITY_EDITOR
    // Update is called once per frame
    void Update ()
    {
        if(Enabled)
        {
            m_Lines.Clear();

            m_Lines.Add("Audio Debug");
            m_Lines.Add("-----------");
            m_Lines.Add("");
            m_Lines.Add("Current Music Ambience: " + MusicSwitcher.CurrentAmbience);
            foreach(AudioSource source in MusicSwitcher.AmbienceAudioSources)
            {
                m_Lines.Add(source.clip.name + " Mix : " + source.volume.ToString());
            }
            m_Lines.Add("");
            m_Lines.Add("Registered Ramdom Emitters");
            m_Lines.Add("--------------------------");
            foreach(RandomCuePlayer player in RandomCueManager.manager.Players)
            {
                if(player.AudioSource.isPlaying)
                    m_Lines.Add("(PLAYING) " + player.CueList.name +"("+ player.AudioSource.clip+") // TTL = " + player.TTL);
            }

            m_text.text = string.Join("\n",m_Lines.ToArray());
        }

        if(Input.GetKeyDown(KeyCode.F10))
        {
            Enabled = !Enabled;
            if (!Enabled)
                m_text.text = "";
            UIRoot.SetActive(Enabled);
        }


	}
#endif
}
