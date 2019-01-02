using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.UI;
using UnityEngine.Rendering.PostProcessing;

public class ExposurePlayableMixer : PlayableBehaviour
{

    // Called each frame the mixer is active, after inputs are processed
    public override void ProcessFrame(Playable handle, FrameData info, object playerData)
    {
        var volume = playerData as PostProcessVolume;
        /*
        var camera = Camera.main;
        if (camera == null)
            return;
        var volume = camera.GetComponent<PostProcessVolume>();
        */
        if (volume == null)
            return;
        AutoExposure m_autoExposure;

        var profile = Application.isPlaying
                ? volume.profile
                : volume.sharedProfile;

        if (!profile.HasSettings<AutoExposure>())
            return;

        float newExposureKey = 0f;

        var count = handle.GetInputCount();
        for (var i = 0; i < count; i++)
        {

            var inputHandle = handle.GetInput(i);
            var weight = handle.GetInputWeight(i);

            if (inputHandle.IsValid() &&
                inputHandle.GetPlayState() == PlayState.Playing &&
                weight > 0)
            {
                var data = ((ScriptPlayable<ExposurePlayable>)inputHandle).GetBehaviour();
                if (data != null)
                {
                    newExposureKey += data.exposureKey * weight;
                }

            }
        }
        profile.TryGetSettings<AutoExposure>(out m_autoExposure);
        m_autoExposure.keyValue.value = newExposureKey;
    }
}
