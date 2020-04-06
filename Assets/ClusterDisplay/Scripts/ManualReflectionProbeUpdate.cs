
using System.Collections;
using UnityEngine;
using UnityEngine.Rendering.HighDefinition;
using GameplayIngredients.Actions;

public class ManualReflectionProbeUpdate : ActionBase
{
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.P))
            UpdateReflectionProbes();
    }
    
    public override void Execute(GameObject instigator = null)
    {
        StartCoroutine(DelayProbesUpdate());
    }

    IEnumerator DelayProbesUpdate()
    {
        yield return null;
        UpdateReflectionProbes();
    }

    static public void UpdateReflectionProbes()
    {
        int count = 0;
        foreach (var probe  in FindObjectsOfType<HDAdditionalReflectionData>())
        {
            ++count;
            probe.RequestRenderNextUpdate();
        }
        Debug.Log($"Manually updated {count} probes");
    }
}
