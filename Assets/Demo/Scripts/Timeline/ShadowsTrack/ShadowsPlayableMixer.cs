using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.UI;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Experimental.Rendering.HDPipeline;

public class ShadowsPlayableMixer : PlayableBehaviour
{
    // Called each frame the mixer is active, after inputs are processed
    public override void ProcessFrame(Playable handle, FrameData info, object playerData)
    {
        if (playerData == null)
            return;
        var volume = playerData as Volume;
        //var camera = Camera.main;
        //if (camera == null || Camera.main.transform.parent.gameObject.tag == "Player")
        //    return;
        //var volume = camera.GetComponent<Volume>();
        HDShadowSettings m_shadowSettings;

        var profile = Application.isPlaying
                ? volume.profile
                : volume.sharedProfile;

        if (!profile.Has<HDShadowSettings>())
            return;

        float newMaxDistance = 0;
        int newCascadeCount = 0;
        float newSplit0 = 0f;
        float newSplit1 = 0f;
        float newSplit2 = 0f;

        var count = handle.GetInputCount();
        for (var i = 0; i < count; i++)
        {

            var inputHandle = handle.GetInput(i);
            var weight = handle.GetInputWeight(i);

            if (inputHandle.IsValid() &&
                inputHandle.GetPlayState() == PlayState.Playing &&
                weight > 0)
            {
                var data = ((ScriptPlayable<ShadowsPlayable>)inputHandle).GetBehaviour();
                if (data != null)
                {
                    newMaxDistance = data.maxDistance * weight;
                    newCascadeCount = Mathf.FloorToInt((float)data.cascadeCount * weight);
                    newSplit0 = data.split0 * weight;
                    newSplit1 = data.split1 * weight;
                    newSplit2 = data.split2 * weight;
                }

            }
        }
        profile.TryGet<HDShadowSettings>(out m_shadowSettings);
        m_shadowSettings.maxShadowDistance.value = newMaxDistance;
        m_shadowSettings.cascadeShadowSplitCount.value = newCascadeCount;
        m_shadowSettings.cascadeShadowSplit0.value = newSplit0;
        m_shadowSettings.cascadeShadowSplit1.value = newSplit1;
        m_shadowSettings.cascadeShadowSplit2.value = newSplit2;
    }
}
