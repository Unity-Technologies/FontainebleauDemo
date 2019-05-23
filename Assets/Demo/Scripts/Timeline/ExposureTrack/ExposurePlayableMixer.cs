using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Rendering;
using UnityEngine.Experimental.Rendering.HDPipeline;

public class ExposurePlayableMixer : PlayableBehaviour
{

    // Called each frame the mixer is active, after inputs are processed
    public override void ProcessFrame(Playable handle, FrameData info, object playerData)
    {
        var volume = playerData as Volume;

        if (volume == null)
            return;
        Exposure m_autoExposure;

        var profile = Application.isPlaying
                ? volume.profile
                : volume.sharedProfile;

        if (!profile.Has<Exposure>())
            return;


        var count = handle.GetInputCount();

        //Short loop
        int activeClipsCount = 0;
        for (var i = 0; i < count; i++)
        {

            var inputHandle = handle.GetInput(i);
            var weight = handle.GetInputWeight(i);
            if (weight > 0)
                activeClipsCount += 1;
        }

        if (activeClipsCount == 0)
        {
            volume.weight = 0;
            return;
        }

        volume.weight = 1;
        float newExposureKey = 0f;

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
        if(profile.TryGet<Exposure>(out m_autoExposure))
            m_autoExposure.fixedExposure.value = newExposureKey;
    }
}
