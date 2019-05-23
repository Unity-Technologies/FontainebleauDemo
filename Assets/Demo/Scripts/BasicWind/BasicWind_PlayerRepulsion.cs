using UnityEngine;

namespace HDRPSamples
{
    [ExecuteInEditMode]
    public class BasicWind_PlayerRepulsion : MonoBehaviour {

        public float radius = 1.0f;
        public float updateRate = 0.03333f;

	    void Start ()
        {
            Shader.SetGlobalVector(BasicWindShaderIDs.PlayerPos, new Vector4(0.0f, 0.0f, 0.0f, radius));

            InvokeRepeating("UpdatePlayerPosition", 0, updateRate);  
	    }

        void UpdatePlayerPosition()
        {
            Shader.SetGlobalVector(BasicWindShaderIDs.PlayerPos, new Vector4(gameObject.transform.position.x, gameObject.transform.position.y, gameObject.transform.position.z,radius));
        }
    }
}