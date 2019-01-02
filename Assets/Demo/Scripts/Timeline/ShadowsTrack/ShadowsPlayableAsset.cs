using System;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;
using UnityEngine.Rendering.PostProcessing;

[Serializable]
public class ShadowsPlayable : PlayableBehaviour
{
    public float maxDistance = 500;
    [Range(1, 4)]
    public int cascadeCount = 4;
    [Range(0,1)]
    public float split0 = 0.05f;
    [Range(0, 1)]
    public float split1 = 0.12f;
    [Range(0, 1)]
    public float split2 = 0.3f;
}

[Serializable]
public class ShadowsPlayableAsset : PlayableAsset, ITimelineClipAsset
{
    public ShadowsPlayable shadowsPlayable = new ShadowsPlayable();

    // Create the runtime version of the clip, by creating a copy of the template
    public override Playable CreatePlayable(PlayableGraph graph, GameObject go)
    {
        return ScriptPlayable<ShadowsPlayable>.Create(graph, shadowsPlayable);
    }

    // Use this to tell the Timeline Editor what features this clip supports
    public ClipCaps clipCaps
    {
        get { return ClipCaps.Blending | ClipCaps.Extrapolation; }
    }
}
