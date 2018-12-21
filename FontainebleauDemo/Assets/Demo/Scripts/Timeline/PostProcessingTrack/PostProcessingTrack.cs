using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;
using UnityEngine.Rendering.PostProcessing;

[TrackColor(0.5f,0,1)]
// Specifies the type of Playable Asset this track manages
 [TrackClipType(typeof(PostProcessingSwitcherAsset))]
public class PostProcessingTrack : TrackAsset
{

    // override the type of mixer playable used by this track
    public override Playable CreateTrackMixer(PlayableGraph graph, GameObject go, int inputCount)
    {
        foreach (var c in GetClips())
        {
            PostProcessingSwitcherAsset asset = (PostProcessingSwitcherAsset)c.asset;
            PostProcessVolume volume = asset.postProcessVolume.Resolve(graph.GetResolver());
            c.displayName = volume == null ? "Postprocessing" : volume.gameObject.name;
        }
        return base.CreateTrackMixer(graph, go, inputCount);
	}
    
}
