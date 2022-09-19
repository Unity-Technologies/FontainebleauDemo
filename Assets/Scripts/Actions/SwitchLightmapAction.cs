using GameplayIngredients;
using GameplayIngredients.Actions;
using NaughtyAttributes;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class SwitchLightmapAction : ActionBase
{
    public int LightmapScenarioIndex = 0;
    public string[] lightingScenarios = { "Day", "Sunset", "Night", "Empty" };

    [ReorderableList]
    public Callable[] AfterLoading;

    public override void Execute(GameObject instigator = null)
    {
        if (ProbeReferenceVolume.instance.isInitialized)
        {
            ProbeReferenceVolume.instance.SetNumberOfCellsLoadedPerFrame(100);
            ProbeReferenceVolume.instance.lightingScenario = lightingScenarios[LightmapScenarioIndex];
            Callable.Call(AfterLoading, instigator);
        }

    }
}

