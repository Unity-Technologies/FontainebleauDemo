using GameplayIngredients.Actions;
using GameplayIngredients;
using UnityEngine;
using NaughtyAttributes;

public class RefreshLPPVAction : ActionBase
{
    private LightProbeProxyVolume[] volumes;
    [ReorderableList]
    public Callable[] onRefreshComplete;
    private void OnEnable()
    {
        volumes = GameObject.FindObjectsOfType<LightProbeProxyVolume>();
    }

    public override void Execute(GameObject instigator = null)
    {
        foreach (LightProbeProxyVolume volume in volumes)
        {
            volume.Update();
        }
        Callable.Call(onRefreshComplete, this.gameObject);
    }
}