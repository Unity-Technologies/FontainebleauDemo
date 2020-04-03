using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

public class DebugShadows : MonoBehaviour
{
    int m_NumDirectionalLights;
    List<string> m_LightsInfo = new List<string>();
    
    void OnGUI()
    {
        GUILayout.Label($"[{m_NumDirectionalLights}] directional lights detected");
        foreach (var info in m_LightsInfo)
            GUILayout.Label(info);
    }

    void Update()
    {
        m_NumDirectionalLights = 0;
        m_LightsInfo.Clear();
        foreach (var light  in FindObjectsOfType<Light>())
        {
            if (light.type == LightType.Directional)
            {
                ++m_NumDirectionalLights;
                m_LightsInfo.Add(GetLightInfo(light));

            }
        }
    }
    
    static string GetLightInfo(Light light)
    {
        var sb = new StringBuilder();
        sb.Append(light.name);
        Transform trs = light.transform;
        while (trs.parent != null)
        {
            trs = trs.parent;
            sb.Append("/" + trs.name);
        }
        return sb.ToString();
    }
}
