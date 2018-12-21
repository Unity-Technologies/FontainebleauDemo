using System;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;
using UnityEngine.Rendering.PostProcessing;

[Serializable]
public class PostProcessingSwitcher : PlayableBehaviour
{
    public PostProcessVolume postProcessVolume;
    private bool m_WasEnabled;
    
    // Called when the owning graph starts playing
	public override void OnGraphStart(Playable playable)
    {
		if (postProcessVolume != null )
        {
            m_WasEnabled = postProcessVolume.enabled;
        }
	}

	// Called when the owning graph stops playing
	public override void OnGraphStop(Playable playable)
    {
        if (postProcessVolume != null)
            postProcessVolume.enabled = m_WasEnabled;
	}

	// Called each frame the playable is active
	public override void PrepareFrame(Playable playable, FrameData info)

    {


    }

    public override void OnBehaviourPause(Playable playable, FrameData info)
    {
        if (postProcessVolume != null)
        {
            postProcessVolume.enabled = false;
        }
    }

    public override void OnBehaviourPlay(Playable playable, FrameData info)
    {
        if (postProcessVolume != null)
        {
            postProcessVolume.enabled = true;
        }
    }
}

[Serializable]
public class PostProcessingSwitcherAsset : PlayableAsset, ITimelineClipAsset
{
    public ExposedReference<PostProcessVolume> postProcessVolume;

    // Create the runtime version of the clip, by creating a copy of the template
    public override Playable CreatePlayable(PlayableGraph graph, GameObject go)
    {
        ScriptPlayable<PostProcessingSwitcher> playable = ScriptPlayable<PostProcessingSwitcher>.Create(graph);

        PostProcessingSwitcher switcher = playable.GetBehaviour();

        switcher.postProcessVolume = postProcessVolume.Resolve(graph.GetResolver());

        return playable;
    }

    // Use this to tell the Timeline Editor what features this clip supports
    public ClipCaps clipCaps
    {
        get { return ClipCaps.Blending | ClipCaps.Extrapolation; }
    }
}
