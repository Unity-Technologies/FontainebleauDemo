using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

[ExecuteInEditMode]
public class LightingScenarioSwitcher : MonoBehaviour {

    public string[] SceneNames;
    private int LightingScenarioSelector;
    private int lightingScenariosCount;
    private string selectedSceneName;
    private Scene currentAdditiveScene;
    private GameObject sceneUI;
    [SerializeField]
    public int DefaultLightingScenario;
    public bool loadSceneWhenSwitching;
	
    public void loadLightingScenario(int index)
    {
        if (!Application.isPlaying)
        {
            this.GetComponent<LevelLightmapData>().LoadLightingScenario(index);
            if (loadSceneWhenSwitching)
            {
                if (currentAdditiveScene.buildIndex != -1) { SceneManager.UnloadSceneAsync(currentAdditiveScene); }
                selectedSceneName = SceneNames[index];
                SceneManager.LoadSceneAsync(selectedSceneName, LoadSceneMode.Additive);
                currentAdditiveScene = SceneManager.GetSceneByPath("Assets/Demo/" + selectedSceneName + ".unity");
                SceneManager.SetActiveScene(SceneManager.GetSceneByPath("Assets/Demo/" + selectedSceneName + ".unity"));
            }
        }
    }
}
