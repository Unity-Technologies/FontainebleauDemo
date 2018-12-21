using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;
using UnityEngine.Experimental.Rendering;

[TrackColor(0, 0, 0)]
[TrackClipType(typeof(ShadowsPlayableAsset))]
[TrackBindingType(typeof(Volume))]
public class ShadowsTrack : TrackAsset
{
    // override the type of mixer playable used by this track
    public override Playable CreateTrackMixer(PlayableGraph graph, GameObject go, int inputCount)
    {
        foreach (var c in GetClips())
        {
            ShadowsPlayableAsset postprocessingPlayable = (ShadowsPlayableAsset)c.asset;
            c.displayName = "count " + postprocessingPlayable.shadowsPlayable.cascadeCount.ToString() + " max distance " + postprocessingPlayable.shadowsPlayable.maxDistance.ToString();
        }
        return ScriptPlayable<ShadowsPlayableMixer>.Create(graph, inputCount);

    }
}