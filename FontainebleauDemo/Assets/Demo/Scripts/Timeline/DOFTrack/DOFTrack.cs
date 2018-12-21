using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;
using UnityEngine.Rendering.PostProcessing;

[TrackColor(0.5f, 0, 1)]
[TrackClipType(typeof(DOFPlayableAsset))]
public class DOFTrack : TrackAsset
{
    // override the type of mixer playable used by this track
    public override Playable CreateTrackMixer(PlayableGraph graph, GameObject go, int inputCount)
    {
        foreach (var c in GetClips())
        {
            DOFPlayableAsset postprocessingPlayable = (DOFPlayableAsset)c.asset;
            c.displayName = postprocessingPlayable.dofPlayable.focusDistance.ToString();
        }
        return ScriptPlayable<DOFPlayableMixer>.Create(graph, inputCount);

    }
}

