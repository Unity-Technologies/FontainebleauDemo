using UnityEngine;

namespace LightingTools.LightProbesVolumes
{
    [ExecuteInEditMode]
    [RequireComponent(typeof(BoxCollider))]
    public class LightProbesVolumeSettings : MonoBehaviour
    {
        public float horizontalSpacing = 2.0f;
        public float verticalSpacing = 2.0f;
        public float offsetFromFloor = 0.5f;
        public int numberOfLayers = 2;
        public bool fillVolume = false;
        public bool followFloor = true;
        public bool discardInsideGeometry;
        public bool drawDebug = false;

        private void OnEnable()
        {
            var boxCollider = GetComponent<BoxCollider>();
            boxCollider.isTrigger = true;
        }

    #if UNITY_EDITOR
        public void Populate()
        {
            LightProbesPlacement.Populate(gameObject,horizontalSpacing,verticalSpacing,offsetFromFloor,numberOfLayers,drawDebug,fillVolume,discardInsideGeometry, followFloor);
        }
    #endif
    }
}