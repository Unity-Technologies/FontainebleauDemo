using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class SceneLoad : MonoBehaviour
{
    public string[] SceneNames;
    private bool[] loaded;
    private bool[] activated;
    private float[] percentages;
    private AsyncOperation[] asyncOperations;


    public GameObject ProgressBar;
    public GameObject ProgressBarContainer;
    public Text LoadingText;
    public Text PercentageText;
    bool bLoading = false;

    // Use this for initialization
    public void LoadScenes()
    {
        if (SceneNames.Length == 0)
            return;

        int count = SceneNames.Length;
        loaded = new bool[count];
        activated = new bool[count];
        percentages = new float[count];
        asyncOperations = new AsyncOperation[count];

        StartCoroutine(LoadAllScenesCoroutine());
    }

    IEnumerator LoadAllScenesCoroutine()
    {
        string loaderSceneName = SceneManager.GetActiveScene().name;

        for (int i = 0; i < SceneNames.Length; i++)
        {
            StartCoroutine(LoadLevelCoroutine(i));
        }

        LoadingText.text = "Loading...";

        while (!AllLoaded())
        {
            yield return new WaitForEndOfFrame();
            float percentage = percentages.Sum() / SceneNames.Length;
            SetProgressBar(percentage);
        }

        LoadingText.text = "Starting...";

        // Once all loaded
        foreach(AsyncOperation async in asyncOperations)
        {
            async.allowSceneActivation = true;
        }

        while (!AllActivated())
        {
            SetProgressBar(0.9999f);
            yield return new WaitForEndOfFrame();
        }

        SceneManager.UnloadSceneAsync(loaderSceneName);
        SceneManager.SetActiveScene(SceneManager.GetSceneByName(SceneNames[0]));
    }

    private void SetProgressBar(float percentage)
    {
        PercentageText.text = ((int)(percentage * 100))+ "%";
        Vector2 size = ProgressBar.GetComponent<RectTransform>().sizeDelta;
        size.x = percentage * ProgressBarContainer.GetComponent<RectTransform>().sizeDelta.x;
        ProgressBar.GetComponent<RectTransform>().sizeDelta = size;
    }

    private bool AllLoaded()
    {
        return loaded.All(val => val == true);
    }

    private bool AllActivated()
    {
        return activated.All(val => val == true);
    }

    IEnumerator LoadLevelCoroutine(int index)
    {
        string sceneName = SceneNames[index];
        AsyncOperation async = SceneManager.LoadSceneAsync(sceneName,LoadSceneMode.Additive);
        async.allowSceneActivation = false;
        

        while (async.progress < 0.9f)
        {
            percentages[index] = async.progress;
            yield return new WaitForEndOfFrame();
        }

        loaded[index] = true;
        asyncOperations[index] = async;
        //async.allowSceneActivation = true;

        // Wait for activation
        while(!async.isDone)
        {
            yield return new WaitForEndOfFrame();
        }

        // Finished! At least!
        activated[index] = true;
    }

    public void Update()
    {
        if(!bLoading && Time.time > 3.0f)
        {
            bLoading = true;
            LoadScenes();
        }
    }
}
