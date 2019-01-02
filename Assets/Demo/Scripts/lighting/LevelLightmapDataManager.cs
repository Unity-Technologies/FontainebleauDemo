using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(LevelLightmapData))]
public class LevelLightmapDataManager : MonoBehaviour
{
    public class Handler
    {
        LevelLightmapData m_Data;

        public void SetLightingScenario(int index)
        {
            if(m_Data != null)
            {
                try
                {
                    m_Data.LoadLightingScenario(index);
                }
                catch { Debug.LogWarning("Warning, tried to load lighting scenario "+index+" but no lightmap data found."); }
            }
        }

        public void SetLightmapData(LevelLightmapData data)
        {
            m_Data = data;
        }
    }

    public static Handler handler { get { if (s_Handler == null) s_Handler = new Handler(); return s_Handler; } }
    static Handler s_Handler;

    void OnEnable()
    {
        handler.SetLightmapData(GetComponent<LevelLightmapData>());
    }
}

