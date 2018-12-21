using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(LightAnimationManager))]
public class LightAnimationNoise : AbstractLightAnimation
{
	public float frequency = 5;
	public float minimumValue = 0;
	public float maximumValue = 1;
	[Range(0.0f,1.0f)]
	public float jumpFrequency = 0;
	public bool enableLightFromStart = true;
	
	private float fade = 1;
    private bool animate = false;
    private float localTime = 0 ;
	private bool lightEnabled = true;
	private int seed;

	void Start ()
    {
        if ( animationMode == AnimationMode.animateFromStart) { animate = true; }
	    if (!enableLightFromStart) { lightEnabled = false; }
	    seed = (int)Random.Range(0,10000);
	}

    public override float getCurrentValue()
    {
        if (!lightEnabled) { fade = 0; }
        if (animate)
        {
	        localTime += Time.deltaTime;
	        if (jumpFrequency>0)
	        {
	        	float jumpRand;
	        	jumpRand = Random.value;
	        	jumpRand = Mathf.Round(jumpRand*10)/10;
	        	if ( jumpRand < jumpFrequency )
	        	{
	        		localTime = localTime + 1;
	        	}
	        }
	        fade = samplePerlinNoise(localTime, frequency, seed);
	        fade = fade*(maximumValue-minimumValue)+minimumValue;
        }
        return fade;
    }
	
	private float samplePerlinNoise(float localtime, float frequency, int seed)
	{
		float noiseFade;
		noiseFade = Mathf.PerlinNoise(localtime*frequency,(float)seed);
		return noiseFade;
	}
	
    void Animate()
    {
        animate = true;
    }

    void PauseAnimate()
    {
        animate = false;
    }

    void StopAnimate()
    {
        if (fade > 0 ) { lightEnabled = true; }
        PauseAnimate();
        localTime = 0;
    }

    //Public Events
    public override void StartAnimation()
    {
        Animate();
    }

    public override void StopAnimation()
    {
        StopAnimate();
    }

    public override void PauseAnimation()
    {
        PauseAnimate();
    }
}
