using UnityEngine;
using System.Collections;
using UnityEditor;
using System;

public class BakeLighting
{

    public static void BatchBake()
    {
        String[] levels = { "Assets/Scenes/master.unity","Assets/Scenes/Prototype01_lighting.unity"};
        Lightmapping.BakeMultipleScenes(levels);
    }
}
