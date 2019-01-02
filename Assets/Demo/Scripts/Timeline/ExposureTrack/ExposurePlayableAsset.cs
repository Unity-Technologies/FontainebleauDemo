using System;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

[Serializable]
public class ExposurePlayable : PlayableBehaviour
{
    [Range(0,1)]
    public float exposureKey = 1;
}

[Serializable]
public class ExposurePlayableAsset : PlayableAsset, ITimelineClipAsset
{
    public ExposurePlayable exposurePlayable = new ExposurePlayable();

    // Create the runtime version of the clip, by creating a copy of the template
    public override Playable CreatePlayable(PlayableGraph graph, GameObject go)
    {
        return ScriptPlayable<ExposurePlayable>.Create(graph, exposurePlayable);
    }

    // Use this to tell the Timeline Editor what features this clip supports
    public ClipCaps clipCaps
    {
        get { return ClipCaps.Blending | ClipCaps.Extrapolation; }
    }
}
