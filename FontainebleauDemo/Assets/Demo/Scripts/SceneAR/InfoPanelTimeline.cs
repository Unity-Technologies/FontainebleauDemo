using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;

public class InfoPanelTimeline : InfoPanelInteraction
{
    public GameObject disableGameobject;
    public GameObject enableGameobject;
    public PlayableDirector timelineDirector;

    public override void OnActivate()
    {
        disableGameobject.SetActive(false);
        StartCoroutine(playTimeline());
    }

    IEnumerator playTimeline()
    {
        timelineDirector.gameObject.SetActive(true);
        timelineDirector.Play();
        yield return new WaitForSeconds((float)timelineDirector.duration);
        timelineDirector.gameObject.SetActive(false);
        PostTimelineAction();
    }

    void PostTimelineAction()
    {
        enableGameobject.SetActive(true);
    }
}
