using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;
using UnityEngine.Rendering.PostProcessing;

[TrackColor(0.5f, 0, 1)]
// Specifies the type of Playable Asset this track manages
[TrackClipType(typeof(ExposurePlayableAsset))]
[TrackBindingType(typeof(PostProcessVolume))]
public class ExposureTrack : TrackAsset
{
    public float stuff = 0;

    // override the type of mixer playable used by this track
    public override Playable CreateTrackMixer(PlayableGraph graph, GameObject go, int inputCount)
    {
        foreach (var c in GetClips())
        {
            ExposurePlayableAsset postprocessingPlayable = (ExposurePlayableAsset)c.asset;
            c.displayName = postprocessingPlayable.exposurePlayable.exposureKey.ToString();
        }
        return ScriptPlayable<ExposurePlayableMixer>.Create(graph, inputCount);

    }
}