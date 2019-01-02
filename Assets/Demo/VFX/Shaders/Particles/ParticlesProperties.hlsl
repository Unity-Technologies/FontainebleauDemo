TEXTURE2D(_ColorMap);
SAMPLER(sampler_ColorMap);
float _Brightness;

TEXTURE2D(_DistortionVectorMap);
SAMPLER(sampler_DistortionVectorMap);
float _DistortionIntensity;

float _AlphaCutoff;

// _SoftParticleInvDistance tells us if the feature is disabled (<0)
float _SoftParticleInvDistance;

// These two params tells us if the feature is disabled (both = 0)
float _CameraFadeNearDistance;
float _CameraFadeFarDistance;
