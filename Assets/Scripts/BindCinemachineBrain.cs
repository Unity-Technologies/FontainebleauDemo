using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;
using Cinemachine;

public class BindCinemachineBrain : MonoBehaviour
{
    private CinemachineBrain brain;
    private PlayableDirector timeline;

    // Use this for initialization
    private void OnEnable()
    {
        brain = FindObjectOfType<CinemachineBrain>();
        timeline = GetComponent<PlayableDirector>();

        BindTimelineTracks();        
    }

    public void BindTimelineTracks()
    {
        if (brain == null || timeline == null)
            return;
        var timelineAsset = (TimelineAsset)timeline.playableAsset;

        var track = timelineAsset.GetOutputTrack(1);
        timeline.SetGenericBinding(track, brain);
    }
}