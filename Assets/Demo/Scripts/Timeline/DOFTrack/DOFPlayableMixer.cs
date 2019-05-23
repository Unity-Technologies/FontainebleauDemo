using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Rendering;
using UnityEngine.Experimental.Rendering.HDPipeline;

public class DOFPlayableMixer : PlayableBehaviour
{
    // Called each frame the mixer is active, after inputs are processed
    public override void ProcessFrame(Playable handle, FrameData info, object playerData)
    {
        Volume volume = playerData as Volume;

        if (volume == null)
            return;
        DepthOfField m_depthOfField;

        var profile = Application.isPlaying
                ? volume.profile
                : volume.sharedProfile;

        if (!profile.Has<DepthOfField>())
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

        float newFocusDistance = 0f;
        float newAperture = 0f;
        float newFocalLength = 0f;

        for (var i = 0; i < count; i++)
        {

            var inputHandle = handle.GetInput(i);
            var weight = handle.GetInputWeight(i);

            if (inputHandle.IsValid() &&
                inputHandle.GetPlayState() == PlayState.Playing &&
                weight > 0)
            {
                var data = ((ScriptPlayable<DOFPlayable>)inputHandle).GetBehaviour();
                if (data != null)
                {
                    newFocusDistance += data.focusDistance * weight;
                    newAperture += data.aperture * weight;
                    newFocalLength += data.focalLength * weight;
                }

            }
        }
        profile.TryGet<DepthOfField>(out m_depthOfField);
        m_depthOfField.focusDistance.value = newFocusDistance;
    }
}