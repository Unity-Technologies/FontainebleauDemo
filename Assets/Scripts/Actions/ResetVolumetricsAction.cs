using UnityEngine;
using GameplayIngredients;
using GameplayIngredients.Actions;
using UnityEngine.Rendering.HighDefinition;
public class ResetVolumetricsAction : ActionBase
{
    public override void Execute(GameObject instigator = null)
    {
        var cam = Manager.Get<VirtualCameraManager>().Camera;
        HDCamera hdCam = HDCamera.GetOrCreate(cam);
        hdCam.Reset();
        hdCam.volumetricHistoryIsValid = false;
        hdCam.colorPyramidHistoryIsValid = false;
    }
}