using UnityEngine;
using UnityEngine.Experimental.Rendering.HDPipeline;

[RequireComponent(typeof(Light))]
public class LightAnimationManager : MonoBehaviour
{
    private float initialIntensity;
    private HDAdditionalLightData lightData;

    void Start()
    {
        lightData = gameObject.GetComponent<HDAdditionalLightData>();
        initialIntensity = lightData.lightDimmer;
    }

    void Update()
    {
        var currentValue = 1.0f;
        foreach (var lightAnimator in gameObject.GetComponents<AbstractLightAnimation>())
        {
            currentValue *= lightAnimator.getCurrentValue();
        }
        lightData.lightDimmer = currentValue * initialIntensity;
        lightData.volumetricDimmer = currentValue * initialIntensity;
    }
}