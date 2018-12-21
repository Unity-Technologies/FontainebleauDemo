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
		public LightmapsMode lightmapsMode;
		public SphericalHarmonics[] lightProbes;
	}

	[SerializeField]
	List<LightingScenarioData> lightingScenariosData = null;

#if UNITY_EDITOR
    [SerializeField]
	public List<SceneAsset> lightingScenariosScenes;
#endif

    [SerializeField]
    public int lightingScenariosCount;
    public bool clearCache;

    private List<SphericalHarmonicsL2[]> lightProbesRuntime = new List<SphericalHarmonicsL2[]>();

    //TODO : enable logs only when verbose enabled
    public bool verbose = false;

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

    public void LoadLightingScenario(int index)
    {
		if (lightingScenariosData[index].lightmaps == null
			|| lightingScenariosData[index].lightmaps.Length == 0)
        {
            Debug.LogWarning("No lightmaps stored in scenario "+index);
            return;
        }

        //DEMO: same lightmaps mode for all scenarios
		//LightmapSettings.lightmapsMode = lightingScenariosData[index].lightmapsMode;

		var newLightmaps = new LightmapData[lightingScenariosData[index].lightmaps.Length];

        for (int i = 0; i < newLightmaps.Length; i++)
        {
            newLightmaps[i] = new LightmapData();
			newLightmaps[i].lightmapColor = lightingScenariosData[index].lightmaps[i];
            //Demo : always directional
            newLightmaps[i].lightmapDir = lightingScenariosData[index].lightmapsDir[i];
            
            //DEMO:Always directional for demo
            /*
            if (lightingScenariosData[index].lightmapsMode != LightmapsMode.NonDirectional)
            {
				newLightmaps[i].lightmapDir = lightingScenariosData[index].lightmapsDir[i];
            }
            */
        }

        LoadLightProbes(index);

		ApplyRendererInfo(lightingScenariosData[index].rendererInfos);

        LightmapSettings.lightmaps = newLightmaps;
    }

    public void ApplyRendererInfo (RendererInfo[] infos)
	{
        for (int i = 0; i < infos.Length; i++)
        {
            try
            {
                var info = infos[i];
                info.renderer.lightmapIndex = infos[i].lightmapIndex;
                if (!info.renderer.isPartOfStaticBatch)
                {
                    info.renderer.lightmapScaleOffset = infos[i].lightmapOffsetScale;
                }
                if (info.renderer.isPartOfStaticBatch && verbose == true)
                {
                    Debug.Log("Object " + info.renderer.gameObject.name + " is part of static batch, skipping lightmap offset and scale.");
                }
            }
            catch
            {
                if (infos[i].renderer != null)
                    Debug.LogWarning("Error in ApplyRendererInfo on " + infos[i].renderer.gameObject.name);
                else if (verbose == true)
                    Debug.LogWarning("Null reference in ApplyRendererInfo");
            }
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

		newLightmapsMode = LightmapSettings.lightmapsMode;

		GenerateLightmapInfo(gameObject, newRendererInfos, newLightmapsTextures, newLightmapsTexturesDir, newLightmapsMode);

		newLightingScenarioData.lightmapsMode = newLightmapsMode;

		newLightingScenarioData.lightmaps = newLightmapsTextures.ToArray();

		if (newLightmapsMode != LightmapsMode.NonDirectional)
        {
			newLightingScenarioData.lightmapsDir = newLightmapsTexturesDir.ToArray();
        }

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
        Debug.Log("Stored " + newLightingScenarioData.lightProbes.Length + " lightprobes" );

		if (lightingScenariosData.Count < index+1) 
		{
            lightingScenariosData.Insert(index, newLightingScenarioData);
        } 
		else 
		{
            lightingScenariosData[index] = newLightingScenarioData;
		}

        lightingScenariosCount = lightingScenariosData.Count;


    }

	static void GenerateLightmapInfo (GameObject root, List<RendererInfo> newRendererInfos, List<Texture2D> newLightmapsLight, List<Texture2D> newLightmapsDir, LightmapsMode newLightmapsMode )
	{
		var renderers = FindObjectsOfType(typeof(MeshRenderer));
        Debug.Log("stored info for "+renderers.Length+" meshrenderers");
        foreach (MeshRenderer renderer in renderers)
		{
			if (renderer.lightmapIndex != -1)
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
                newRendererInfos.Add(info);
			}
		}
	}

    public void BuildLightingScenario(SceneAsset scenneAsset)
    {
        //Remove reference to LightingDataAsset so that Unity doesn't delete the previous bake
        Lightmapping.lightingDataAsset = null;

        Debug.Log("Baking" + scenneAsset.name);

        EditorSceneManager.OpenScene("Assets/Demo/" + scenneAsset.name + ".unity", OpenSceneMode.Additive);
        EditorSceneManager.SetActiveScene(EditorSceneManager.GetSceneByPath("Assets/Demo/" + scenneAsset.name + ".unity"));

        StartCoroutine(BuildLightingAsync(scenneAsset.name));
    }

    private IEnumerator BuildLightingAsync(string ScenarioName)
    {
        if ( clearCache ) Lightmapping.ClearDiskCache();
        var newLightmapMode = new LightmapsMode();
        newLightmapMode = LightmapSettings.lightmapsMode;
        Lightmapping.BakeAsync();
        while (Lightmapping.isRunning) { yield return null; }
        Lightmapping.lightingDataAsset = null;
        EditorSceneManager.SaveScene(EditorSceneManager.GetSceneByPath("Assets/Demo/" + ScenarioName + ".unity"));
        EditorSceneManager.CloseScene(EditorSceneManager.GetSceneByPath("Assets/Demo/" + ScenarioName + ".unity"), true);
        LightmapSettings.lightmapsMode = newLightmapMode;
    }
#endif

}