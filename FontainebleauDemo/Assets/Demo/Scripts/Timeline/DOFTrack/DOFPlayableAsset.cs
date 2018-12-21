using System;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;
using UnityEngine.Rendering.PostProcessing;

[Serializable]
public class DOFPlayable : PlayableBehaviour
{
    public float focusDistance = 1;
    [Range(0.1f,32f)]
    public float aperture = 5.6f;
    [Range(1f, 300f)]
    public float focalLength = 50f;
}

[Serializable]
public class DOFPlayableAsset : PlayableAsset, ITimelineClipAsset
{
    public DOFPlayable dofPlayable = new DOFPlayable();

    // Create the runtime version of the clip, by creating a copy of the template
    public override Playable CreatePlayable(PlayableGraph graph, GameObject go)
    {
        return ScriptPlayable<DOFPlayable>.Create(graph, dofPlayable);
    }

    // Use this to tell the Timeline Editor what features this clip supports
    public ClipCaps clipCaps
    {
        get { return ClipCaps.Blending | ClipCaps.Extrapolation; }
    }
}
