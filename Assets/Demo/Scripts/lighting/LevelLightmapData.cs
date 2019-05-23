using UnityEngine;
using UnityEngine.SceneManagement;
using System;
using System.Collections.Generic;
using UnityEngine.Rendering;
using System.Collections;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
#endif

[ExecuteInEditMode]
public class LevelLightmapData : MonoBehaviour
{
    [System.Serializable]
    public class SphericalHarmonics
    {
        public float[] coefficients = new float[27];
    }

	[System.Serializable]
	public class RendererInfo
	{
		public Renderer renderer;
		public int lightmapIndex;
		public Vector4 lightmapOffsetScale;
	}
		
	[System.Serializable]
	public class LightingScenarioData {
		public RendererInfo[] rendererInfos;
		public Texture2D[] lightmaps;
		public Texture2D[] lightmapsDir;
        public Texture2D[] shadowMasks;
        public LightmapsMode lightmapsMode;
		public SphericalHarmonics[] lightProbes;
        public bool hasRealtimeLights;
	}

    public bool latestBuildHasReltimeLights;

	[SerializeField]
	List<LightingScenarioData> lightingScenariosData;

#if UNITY_EDITOR
    [SerializeField]
	public List<SceneAsset> lightingScenariosScenes;
#endif
    [SerializeField]
    public String[] lightingScenesNames;
    public int currentLightingScenario = -1;
    public int previousLightingScenario = -1;

    [SerializeField]
    public int lightingScenariosCount;

    //TODO : enable logs only when verbose enabled
    public bool verbose = false;

    private List<SphericalHarmonicsL2[]> lightProbesRuntime = new List<SphericalHarmonicsL2[]>();

    public void LoadLightingScenario(int index)
    {
        if(index != currentLightingScenario)
        {
            previousLightingScenario = currentLightingScenario == -1 ? index : currentLightingScenario;

            currentLightingScenario = index;

            LightmapSettings.lightmapsMode = lightingScenariosData[index].lightmapsMode;

            var newLightmaps = LoadLightmaps(index);

            ApplyRendererInfo(lightingScenariosData[index].rendererInfos);

            LightmapSettings.lightmaps = newLightmaps;
        }
    }

#if UNITY_EDITOR
    private void OnEnable()
    {
        if (lightingScenariosScenes == null)
            lightingScenariosScenes = new List<SceneAsset>();
    }
#endif

    private void Start()
    {
        PrepareLightProbeArrays();
    }

    private void PrepareLightProbeArrays()
    {
        for (int x = 0; x < lightingScenariosCount; x++)
        {
            lightProbesRuntime.Add(DeserializeLightProbes(x));
        }
    }

    private SphericalHarmonicsL2[] DeserializeLightProbes(int index)
    {
        var sphericalHarmonicsArray = new SphericalHarmonicsL2[lightingScenariosData[index].lightProbes.Length];

        for (int i = 0; i < lightingScenariosData[index].lightProbes.Length; i++)
        {
            var sphericalHarmonics = new SphericalHarmonicsL2();

            // j is coefficient
            for (int j = 0; j < 3; j++)
            {
                //k is channel ( r g b )
                for (int k = 0; k < 9; k++)
                {
                    sphericalHarmonics[j, k] = lightingScenariosData[index].lightProbes[i].coefficients[j * 9 + k];
                }
            }

            sphericalHarmonicsArray[i] = sphericalHarmonics;
        }
        return sphericalHarmonicsArray;
    }

#if UNITY_EDITOR
    public void updateNames()
    {
        lightingScenesNames = new string[lightingScenariosScenes.Count];
        for (int i = 0; i< lightingScenariosScenes.Count; i++)
        {
            lightingScenesNames[i] = lightingScenariosScenes[i] != null ? lightingScenariosScenes[i].name : "missing";
        }
    }
#endif

    LightmapData[] LoadLightmaps(int index)
    {
        if (lightingScenariosData[index].lightmaps == null
                || lightingScenariosData[index].lightmaps.Length == 0)
        {
            Debug.LogWarning("No lightmaps stored in scenario " + index);
            return null;
        }

        var newLightmaps = new LightmapData[lightingScenariosData[index].lightmaps.Length];

        for (int i = 0; i < newLightmaps.Length; i++)
        {
            newLightmaps[i] = new LightmapData();
            newLightmaps[i].lightmapColor = lightingScenariosData[index].lightmaps[i];

            if (lightingScenariosData[index].lightmapsMode != LightmapsMode.NonDirectional)
            {
                newLightmaps[i].lightmapDir = lightingScenariosData[index].lightmapsDir[i];
            }
            if (lightingScenariosData[index].shadowMasks.Length > 0)
            {
                newLightmaps[i].shadowMask = lightingScenariosData[index].shadowMasks[i];
            }
        }

        return newLightmaps;
    }

    public void ApplyRendererInfo (RendererInfo[] infos)
	{
        try
        {
            Terrain terrain = FindObjectOfType<Terrain>();
            int i = 0;
            if (terrain != null)
            {
                terrain.lightmapIndex = infos[i].lightmapIndex;
                terrain.lightmapScaleOffset = infos[i].lightmapOffsetScale;
                i++;
            }

            for ( int j = i ; j < infos.Length; j++)
            {
                RendererInfo info = infos[j];
                info.renderer.lightmapIndex = infos[j].lightmapIndex;
                if (!info.renderer.isPartOfStaticBatch)
                {
                    info.renderer.lightmapScaleOffset = infos[j].lightmapOffsetScale;
                }
                if (info.renderer.isPartOfStaticBatch && verbose == true && Application.isEditor)
                {
                    Debug.Log("Object " + info.renderer.gameObject.name + " is part of static batch, skipping lightmap offset and scale.");
                }
            }
        }
        catch (Exception e)
        {
            Debug.LogError("Error in ApplyRendererInfo:" + e.GetType().ToString());
        }
    }

    public void LoadLightProbes(int index)
    {
        if (Application.isEditor && !Application.isPlaying)
        {
            PrepareLightProbeArrays();
        }

        try
        {
            LightmapSettings.lightProbes.bakedProbes = lightProbesRuntime[index];
        }
        catch { Debug.LogWarning("Warning, error when trying to load lightprobes for scenario " + index); }
    }

#if UNITY_EDITOR

    public void StoreLightmapInfos(int index)
    {
        Debug.Log("Storing data for lighting scenario " + index);
        if (UnityEditor.Lightmapping.giWorkflowMode != UnityEditor.Lightmapping.GIWorkflowMode.OnDemand)
        {
            Debug.LogError("ExtractLightmapData requires that you have baked you lightmaps and Auto mode is disabled.");
            return;
        }

		var newLightingScenarioData = new LightingScenarioData ();
        var newRendererInfos = new List<RendererInfo>();
        var newLightmapsTextures = new List<Texture2D>();
        var newLightmapsTexturesDir = new List<Texture2D>();
		var newLightmapsMode = new LightmapsMode();
		var newSphericalHarmonicsList = new List<SphericalHarmonics>();
        var newLightmapsShadowMasks = new List<Texture2D>();

        newLightmapsMode = LightmapSettings.lightmapsMode;

        GenerateLightmapInfo(gameObject, newRendererInfos, newLightmapsTextures, newLightmapsTexturesDir, newLightmapsShadowMasks, newLightmapsMode);

        newLightingScenarioData.lightmapsMode = newLightmapsMode;

		newLightingScenarioData.lightmaps = newLightmapsTextures.ToArray();

		if (newLightmapsMode != LightmapsMode.NonDirectional)
        {
			newLightingScenarioData.lightmapsDir = newLightmapsTexturesDir.ToArray();
        }

        //Mixed or realtime support
        newLightingScenarioData.hasRealtimeLights = latestBuildHasReltimeLights;

        newLightingScenarioData.shadowMasks = newLightmapsShadowMasks.ToArray();

        newLightingScenarioData.rendererInfos = newRendererInfos.ToArray();

		var scene_LightProbes = new SphericalHarmonicsL2[LightmapSettings.lightProbes.bakedProbes.Length];
		scene_LightProbes = LightmapSettings.lightProbes.bakedProbes;

        for (int i = 0; i < scene_LightProbes.Length; i++)
        {
            var SHCoeff = new SphericalHarmonics();

            // j is coefficient
            for (int j = 0; j < 3; j++)
            {
                //k is channel ( r g b )
                for (int k = 0; k < 9; k++)
                {
                    SHCoeff.coefficients[j*9+k] = scene_LightProbes[i][j, k];
                }
            }

            newSphericalHarmonicsList.Add(SHCoeff);
        }

		newLightingScenarioData.lightProbes = newSphericalHarmonicsList.ToArray ();

        if (lightingScenariosData.Count < index + 1)
        {
            lightingScenariosData.Insert(index, newLightingScenarioData);
        }
        else
        {
            lightingScenariosData[index] = newLightingScenarioData;
        }

        lightingScenariosCount = lightingScenariosData.Count;

        if (lightingScenesNames == null || lightingScenesNames.Length< lightingScenariosCount)
        {
            lightingScenesNames = new string[lightingScenariosCount];
        }
        if (latestBuildHasReltimeLights && lightingScenesNames[index] != null)
            lightingScenesNames[index] = lightingScenariosScenes[index].name;
    }

    static void GenerateLightmapInfo(GameObject root, List<RendererInfo> newRendererInfos, List<Texture2D> newLightmapsLight, List<Texture2D> newLightmapsDir, List<Texture2D> newLightmapsShadow, LightmapsMode newLightmapsMode)
    {
        Terrain terrain = FindObjectOfType<Terrain>();
        if (terrain!= null && terrain.lightmapIndex != -1 && terrain.lightmapIndex != 65534)
        {
            RendererInfo terrainRendererInfo = new RendererInfo();
            terrainRendererInfo.lightmapOffsetScale = terrain.lightmapScaleOffset;

            Texture2D lightmaplight = LightmapSettings.lightmaps[terrain.lightmapIndex].lightmapColor;
            terrainRendererInfo.lightmapIndex = newLightmapsLight.IndexOf(lightmaplight);
            if (terrainRendererInfo.lightmapIndex == -1)
            {
                terrainRendererInfo.lightmapIndex = newLightmapsLight.Count;
                newLightmapsLight.Add(lightmaplight);
            }

            if (newLightmapsMode != LightmapsMode.NonDirectional)
            {
                Texture2D lightmapdir = LightmapSettings.lightmaps[terrain.lightmapIndex].lightmapDir;
                terrainRendererInfo.lightmapIndex = newLightmapsDir.IndexOf(lightmapdir);
                if (terrainRendererInfo.lightmapIndex == -1)
                {
                    terrainRendererInfo.lightmapIndex = newLightmapsDir.Count;
                    newLightmapsDir.Add(lightmapdir);
                }
            }
            if (LightmapSettings.lightmaps[terrain.lightmapIndex].shadowMask != null)
            {
                Texture2D lightmapShadow = LightmapSettings.lightmaps[terrain.lightmapIndex].shadowMask;
                terrainRendererInfo.lightmapIndex = newLightmapsShadow.IndexOf(lightmapShadow);
                if (terrainRendererInfo.lightmapIndex == -1)
                {
                    terrainRendererInfo.lightmapIndex = newLightmapsShadow.Count;
                    newLightmapsShadow.Add(lightmapShadow);
                }
            }
            newRendererInfos.Add(terrainRendererInfo);

            Debug.Log("Terrain lightmap stored in" + terrainRendererInfo.lightmapIndex.ToString());
        }

        var renderers = FindObjectsOfType(typeof(Renderer));
        Debug.Log("stored info for " + renderers.Length + " meshrenderers");
        foreach (Renderer renderer in renderers)
        {
			if (renderer.lightmapIndex != -1 && renderer.lightmapIndex != 65534)
			{
				RendererInfo info = new RendererInfo();
				info.renderer = renderer;
				info.lightmapOffsetScale = renderer.lightmapScaleOffset;

				Texture2D lightmaplight = LightmapSettings.lightmaps[renderer.lightmapIndex].lightmapColor;
                info.lightmapIndex = newLightmapsLight.IndexOf(lightmaplight);
                if (info.lightmapIndex == -1)
				{
					info.lightmapIndex = newLightmapsLight.Count;
                    newLightmapsLight.Add(lightmaplight);
				}

                if (newLightmapsMode != LightmapsMode.NonDirectional)
                {
                    Texture2D lightmapdir = LightmapSettings.lightmaps[renderer.lightmapIndex].lightmapDir;
                    info.lightmapIndex = newLightmapsDir.IndexOf(lightmapdir);
                    if (info.lightmapIndex == -1)
                    {
                        info.lightmapIndex = newLightmapsDir.Count;
                        newLightmapsDir.Add(lightmapdir);
                    }
                }
                if (LightmapSettings.lightmaps[renderer.lightmapIndex].shadowMask != null)
                {
                    Texture2D lightmapShadow = LightmapSettings.lightmaps[renderer.lightmapIndex].shadowMask;
                    info.lightmapIndex = newLightmapsShadow.IndexOf(lightmapShadow);
                    if (info.lightmapIndex == -1)
                    {
                       info.lightmapIndex = newLightmapsShadow.Count;
                       newLightmapsShadow.Add(lightmapShadow);
                    }
                }
                newRendererInfos.Add(info);
			}
		}
    }

    public void BuildLightingScenario(int ScenarioID)
    {
        //Remove reference to LightingDataAsset so that Unity doesn't delete the previous bake
        Lightmapping.lightingDataAsset = null;

        Debug.Log("Loading " + lightingScenariosScenes[ScenarioID].name);

        string lightingSceneGUID = AssetDatabase.FindAssets(lightingScenesNames[ScenarioID])[0];
        string lightingScenePath = AssetDatabase.GUIDToAssetPath(lightingSceneGUID);
        if (!lightingScenePath.EndsWith(".unity"))
            lightingScenePath = lightingScenePath + ".unity";

        EditorSceneManager.OpenScene(lightingScenePath, OpenSceneMode.Additive);

        Scene lightingScene = SceneManager.GetSceneByName(lightingScenariosScenes[ScenarioID].name);
        EditorSceneManager.SetActiveScene(lightingScene);

        SearchLightsNeededRealtime();

        Debug.Log("Start baking");
        StartCoroutine(BuildLightingAsync(lightingScene));
    }

    void SearchLightsNeededRealtime()
    {
        var lights = FindObjectsOfType<Light>();
        var reflectionProbes = FindObjectsOfType<ReflectionProbe>();
        latestBuildHasReltimeLights = false;

        foreach (Light light in lights)
        {
            if (light.lightmapBakeType == LightmapBakeType.Mixed || light.lightmapBakeType == LightmapBakeType.Realtime)
                latestBuildHasReltimeLights = true;
        }
        if (reflectionProbes.Length > 0)
            latestBuildHasReltimeLights = true;
    }

    private IEnumerator BuildLightingAsync(Scene lightingScene)
    {
        var newLightmapMode = new LightmapsMode();
        newLightmapMode = LightmapSettings.lightmapsMode;
        Lightmapping.BakeAsync();
        while (Lightmapping.isRunning) { yield return null; }
        //EditorSceneManager.SaveScene(EditorSceneManager.GetSceneByPath("Assets/Scenes/" + ScenarioName + ".unity"));
        //Lightmapping.lightingDataAsset = null;
        EditorSceneManager.SaveScene(lightingScene);
        EditorSceneManager.CloseScene(lightingScene, true);
        LightmapSettings.lightmapsMode = newLightmapMode;
    }
#endif
}