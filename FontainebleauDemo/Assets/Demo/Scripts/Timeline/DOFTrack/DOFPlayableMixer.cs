using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.UI;
using UnityEngine.Rendering.PostProcessing;

public class DOFPlayableMixer : PlayableBehaviour
{
    // Called each frame the mixer is active, after inputs are processed
    public override void ProcessFrame(Playable handle, FrameData info, object playerData)
    {
        var camera = Camera.main;
        if (camera == null || Camera.main.transform.parent.gameObject.tag == "Player")
            return;
        var volume = camera.GetComponent<PostProcessVolume>();
        if (volume == null)
            return;
        DepthOfField m_depthOfField;

        var profile = Application.isPlaying
                ? volume.profile
                : volume.sharedProfile;

        if (!profile.HasSettings<DepthOfField>())
            return;

        float newFocusDistance = 0f;
        float newAperture = 0f;
        float newFocalLength = 0f;

        var count = handle.GetInputCount();
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
        profile.TryGetSettings<DepthOfField>(out m_depthOfField);
        m_depthOfField.focusDistance.value = newFocusDistance;
        m_depthOfField.aperture.value = newAperture;
        m_depthOfField.focalLength.value = newFocalLength;
    }
}
