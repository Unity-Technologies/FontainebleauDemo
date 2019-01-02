using System;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;
using UnityEngine.Rendering.PostProcessing;

[Serializable]
public class CameraSwitcher : PlayableBehaviour
{
    public GameObject gameobject;
    private bool m_WasEnabled;
    
    // Called when the owning graph starts playing
	public override void OnGraphStart(Playable playable)
    {
		if (gameobject != null )
        {
            m_WasEnabled = gameobject.activeInHierarchy;
        }
	}

	// Called when the owning graph stops playing
	public override void OnGraphStop(Playable playable)
    {
        if (gameobject != null)
            gameobject.SetActive(m_WasEnabled);
	}

	// Called each frame the playable is active
	public override void PrepareFrame(Playable playable, FrameData info)

    {


    }

    public override void OnBehaviourPause(Playable playable, FrameData info)
    {
        if (gameobject != null)
        {
            gameobject.SetActive(false);
        }
    }

    public override void OnBehaviourPlay(Playable playable, FrameData info)
    {
        if (gameobject != null)
        {
            gameobject.SetActive(true);
            if (gameobject.GetComponent<PostProcessLayer>() != null && gameobject.activeInHierarchy)
            {
                gameobject.GetComponent<PostProcessLayer>().ResetHistory();
#if UNITY_EDITOR
                Debug.Log("reset history on " + gameobject.name);
#endif
            }
            
        }
    }
}

[Serializable]
public class CameraSwitcherAsset : PlayableAsset, ITimelineClipAsset
{
    public ExposedReference<GameObject> gameobject;

    // Create the runtime version of the clip, by creating a copy of the template
    public override Playable CreatePlayable(PlayableGraph graph, GameObject go)
    {
        ScriptPlayable<CameraSwitcher> playable = ScriptPlayable<CameraSwitcher>.Create(graph);

        CameraSwitcher switcher = playable.GetBehaviour();

        switcher.gameobject = gameobject.Resolve(graph.GetResolver());

        return playable;
    }

    // Use this to tell the Timeline Editor what features this clip supports
    public ClipCaps clipCaps
    {
        get { return ClipCaps.Blending | ClipCaps.Extrapolation; }
    }
}
