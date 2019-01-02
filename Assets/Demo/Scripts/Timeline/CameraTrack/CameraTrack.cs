using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

[TrackColor(1,0.4f,0)]
// Specifies the type of Playable Asset this track manages
 [TrackClipType(typeof(CameraSwitcherAsset))]
public class CameraTrack : TrackAsset
{

    // override the type of mixer playable used by this track
    public override Playable CreateTrackMixer(PlayableGraph graph, GameObject go, int inputCount)
    {
        foreach (var c in GetClips())
        {
            CameraSwitcherAsset asset = (CameraSwitcherAsset)c.asset;
            GameObject camera = asset.gameobject.Resolve(graph.GetResolver());
            c.displayName = camera == null ? "Null Camera" : camera.name;
        }

        return base.CreateTrackMixer(graph, go, inputCount);
	}
    
}