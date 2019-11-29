using GameplayIngredients;
using GameplayIngredients.Actions;
using NaughtyAttributes;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SwitchLightmapAction : ActionBase
{
    public int LightmapScenarioIndex = 0;

    [ReorderableList]
    public Callable[] AfterLoading;

    public override void Execute(GameObject instigator = null)
    {
        var levelLightmapData = FindObjectOfType<LevelLightmapData>();
        if(levelLightmapData != null)
        {
            levelLightmapData.LoadLightingScenario(LightmapScenarioIndex);
            Callable.Call(AfterLoading, instigator);
        }
    }
}
