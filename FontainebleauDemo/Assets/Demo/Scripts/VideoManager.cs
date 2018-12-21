using System;
using System.Collections;
using UnityEngine.Audio;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.Events;

public class VideoManager : MonoBehaviour
{
    [Serializable]
    public enum VideoMode
    {
        Menu = 0,
        FlyBy = 1,
        FreeCamera = 2,
        ThirdPerson = 3
    }

    [Header("Gameplay")]
    public VideoMode DefaultVideoMode;
    public VideoMode DefaultPS4VideoMode;

    public GameObject FreeCamRoot;
    public GameObject ThirdPersonRoot;

    public GameObject timelineCameras;

    [Header("Lighting")]
    public int DefaultLightingScenario;
    public string[] LightingMaps;
    public GameObject[] exposureRoots;
    public GameObject[] lightingRoots;
    //public PlayableDirector lanternTimeline;
    public UnityEvent lanternStart;
    public UnityEvent lanternStop;
    private bool toggleLantern = false;

    [Header("Audio")]
    public AudioMixer DemoMixer;
    public string[] AudioMaps;
    public RandomCueManager RamdomCueManager;
    public AudioMixerSnapshot[] AmbienceSnapshots;
    public AudioMixerSnapshot SilentSnapshot;
    public AudioMixerSnapshot FlyBySnapshot;
    public MusicSwitcher MusicSwitcher;

    [Header("UI/Menu")]
    public GameObject UIMenuRoot;
    public GameObject UIFlyByRoot;
    public GameObject UIFlybyMenu;
    public Button UIFirstMenuButton;
    public Button UIFirstMenuButtonFlyby;
    public Button lanternButton;


    [Header("FadeIn/Out")]
    public Image UIFadeImage;
    public Canvas DemoCanvas;
    public Vector2 EnterMenuInOutDuration = new Vector2(0.0f, 1.0f);
    public Vector2 LightingChangeInOutDuration = new Vector2(0.5f, 2.0f);
    public Vector2 GameplayChangeInOutDuration = new Vector2(0.5f, 1.0f);

    public VideoMode currentVideoMode { get { return m_CurrentVideoMode; } }
    private VideoMode m_CurrentVideoMode;
    private int m_CurrentAmbience = -1;
    private Coroutine m_FadeCoroutine;
    private Coroutine m_laternCoroutine;

    private bool[] currentLoad;

    private bool enableInput = true;

    void Start()
    {
        DemoCanvas.gameObject.SetActive(true);
        UIFadeImage.color = Color.black;
        UIFadeImage.gameObject.SetActive(false);
        
        m_CurrentAmbience = DefaultLightingScenario;

        m_CurrentVideoMode = DefaultVideoMode;

#if UNITY_EDITOR
        // Unload work Maps
        foreach (string s in LightingMaps)
        {
            if(SceneManager.GetSceneByName(s).isLoaded)
                SceneManager.UnloadSceneAsync(s);
        }
         
        foreach(string s in AudioMaps)
        {
            if(SceneManager.GetSceneByName(s).isLoaded)
                SceneManager.UnloadSceneAsync(s);
        }
#endif
        
        SceneManager.LoadScene(LightingMaps[m_CurrentAmbience], LoadSceneMode.Additive);
        SceneManager.LoadScene(AudioMaps[m_CurrentAmbience], LoadSceneMode.Additive);

        LevelLightmapDataManager.handler.SetLightingScenario(DefaultLightingScenario);
        SwitchLightingAssets(DefaultLightingScenario);

        AmbienceSnapshots[m_CurrentAmbience].TransitionTo(3.0f);

#if UNITY_PS4
        m_CurrentVideoMode = DefaultPS4VideoMode;
#endif

        m_FadeCoroutine = StartCoroutine(GameplayFadeTransition(EnterMenuInOutDuration.x, EnterMenuInOutDuration.y, UpdateDemoMode));
    }

    void Update()
    {
        UpdateCursorLock();

        if (currentVideoMode!= 0 && Input.GetButtonDown("Cancel") && enableInput)
        {
            SwitchDemoMode(VideoMode.Menu);
        }


        if(currentVideoMode != 0 && !UIFlybyMenu.activeInHierarchy && Input.GetButtonDown("Options") && enableInput)
        {
            UIFlybyMenu.SetActive(true);
        }

        if(Input.GetKeyDown(KeyCode.Keypad0) && enableInput)
        {
            SwitchLighting(0);
        }
        if (Input.GetKeyDown(KeyCode.Keypad1) && enableInput)
        {
            SwitchLighting(1);
        }
        if (Input.GetKeyDown(KeyCode.Keypad2) && enableInput)
        {
            SwitchLighting(2);
        }
        if (Input.GetKeyDown(KeyCode.Keypad3) && enableInput)
        {
            ToggleLanterns();
        }
    }

    public void UpdateCursorLock()
    {
        bool needCursor = (currentVideoMode == VideoMode.Menu || UIFlybyMenu.activeInHierarchy);

        if (!needCursor)
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
        else
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
    }

#region LIGHTING


    public void SwitchLighting(int index)
    {
        if (index != m_CurrentAmbience && m_FadeCoroutine == null)
        {
            enableInput = false;
            StopCoroutine("LightingFadeTransition");
            m_FadeCoroutine = StartCoroutine(LightingFadeTransition(LightingChangeInOutDuration.x, LightingChangeInOutDuration.y, index));
        }
    }

    public void SwitchLightingAssets(int index)
    {
        for (int i = 0; i < exposureRoots.Length; i++)
        {
            exposureRoots[i].SetActive(i == index ? true : false);
        }
        for (int i = 0; i < lightingRoots.Length; i++)
        {
            lightingRoots[i].SetActive(i == index ? true : false);
        }
        //Hardcoded disable lantern button for non night scenarios
        lanternButton.interactable = index == 1 ? true : false ;
    }

    IEnumerator LightingFadeTransition(float outDuration, float inDuration, int newLighting)
    {
        SilentSnapshot.TransitionTo(outDuration *2);
        MusicSwitcher.CurrentAmbience = newLighting;
        // Fade Out
        UIFadeImage.gameObject.SetActive(true);
        while (UIFadeImage.color.a < 1.0f)
        {
            float a = Mathf.Min(1.0f, UIFadeImage.color.a + (Time.deltaTime / Mathf.Max(0.0001f, outDuration)));
            UIFadeImage.color = new Color(0, 0, 0, a);
            yield return new WaitForEndOfFrame();
        }


        // Unload
        string lightingSceneToUnload = null;
        string audioSceneToUnLoad = null;

        if (m_CurrentAmbience >= 0)
        {
            lightingSceneToUnload = LightingMaps[m_CurrentAmbience];
            audioSceneToUnLoad = AudioMaps[m_CurrentAmbience];
        }

        m_CurrentAmbience = newLighting;

        string lightingSceneToLoad = LightingMaps[m_CurrentAmbience];
        string audioSceneToLoad = AudioMaps[m_CurrentAmbience];

        currentLoad = new bool[2] {false,false} ;

        while(!currentLoad[0])
        {
            yield return UnloadSceneCoroutine(lightingSceneToUnload, lightingSceneToLoad,0);
        }

        while(!currentLoad[1])
        {
            yield return UnloadSceneCoroutine(audioSceneToUnLoad, audioSceneToLoad,1);
        }

        LevelLightmapDataManager.handler.SetLightingScenario(newLighting);

        // Then Fade In again
        if (currentVideoMode == VideoMode.FlyBy)
            FlyBySnapshot.TransitionTo(inDuration * 2);
        else
            AmbienceSnapshots[m_CurrentAmbience].TransitionTo(inDuration * 2);

        SwitchLightingAssets(newLighting);

        while (UIFadeImage.color.a > 0.0f)
        {
            float a = Mathf.Max(0.0f,UIFadeImage.color.a - (Time.deltaTime / Mathf.Max(0.0001f,inDuration)));
            UIFadeImage.color = new Color(0, 0, 0, a);
            yield return new WaitForEndOfFrame();
        }
        UIFadeImage.gameObject.SetActive(false);
        m_FadeCoroutine = null;
        enableInput = true;
    }

    IEnumerator UnloadSceneCoroutine(string sceneToUnload, string sceneToLoad, int loadStateIndex)
    {
        AsyncOperation unloadop = null;

        if(sceneToUnload != null)
            unloadop = SceneManager.UnloadSceneAsync(sceneToUnload);

        AsyncOperation loadop = SceneManager.LoadSceneAsync(sceneToLoad, LoadSceneMode.Additive);

        while((!unloadop.isDone || unloadop == null) && !loadop.isDone)
        {
            yield return new WaitForEndOfFrame();
        }
        currentLoad[loadStateIndex] = true;
    }
    
    public void ToggleLanterns()
    {
        if (m_laternCoroutine == null)
        {
            var buttonColors = new ColorBlock();
            buttonColors = lanternButton.colors;
            buttonColors.normalColor = toggleLantern ? new Color(0.663f, 0.663f, 0.663f) : new Color(1, 0.702f, 0.384f);
            buttonColors.highlightedColor = toggleLantern ? new Color(1, 1, 1) : new Color(1, 0.878f, 0.686f);
            lanternButton.colors = buttonColors;
            if (!toggleLantern)
            {
                lanternStart.Invoke();
                m_laternCoroutine = StartCoroutine(LightingFadeLanterns(toggleLantern));
            }
            else if (toggleLantern)
            {
                lanternStop.Invoke();
                m_laternCoroutine = StartCoroutine(LightingFadeLanterns(toggleLantern));
            }
            toggleLantern = !toggleLantern;
        }
    }

    IEnumerator LightingFadeLanterns(bool reversed)
    {
        float start =  0.0f;
        float end = 1.5f;

        for(var time = start; time < end; time += Time.deltaTime)
        {
            //lanternTimeline.time = reversed ? end - time : time;
            //lanternTimeline.Evaluate();
            yield return new WaitForEndOfFrame();
        }

        //lanternTimeline.time = reversed ? start : end;
        //lanternTimeline.Evaluate();
        m_laternCoroutine = null;
    }

    void toggleTimelineLightingEntities(bool active)
    {
        exposureRoots[0].transform.parent.gameObject.SetActive(active);
        lightingRoots[0].transform.parent.gameObject.SetActive(active);
    }

        #endregion

        #region GAMEPLAY

    public void SwitchDemoMode(int mode)
    {
        SwitchDemoMode((VideoMode)mode);
    }

    public void SwitchDemoMode(VideoMode mode)
    {
        if (m_CurrentVideoMode == mode)
            return;

        m_CurrentVideoMode = mode;
        StopAllCoroutines();

        // Audio Mix
        if (currentVideoMode == VideoMode.FlyBy)
            FlyBySnapshot.TransitionTo(GameplayChangeInOutDuration.x + GameplayChangeInOutDuration.y);
        else
            AmbienceSnapshots[m_CurrentAmbience].TransitionTo(GameplayChangeInOutDuration.x + GameplayChangeInOutDuration.y);

        m_FadeCoroutine = StartCoroutine(GameplayFadeTransition(GameplayChangeInOutDuration.x, GameplayChangeInOutDuration.y, UpdateDemoMode));

    }

    public void ExitDemo()
    {
        Application.Quit();
    }

    private void UpdateDemoMode()
    {
        EventSystem eventSystem = FindObjectOfType<EventSystem>();

        switch (m_CurrentVideoMode)
        {
            case VideoMode.Menu:
                toggleTimelineLightingEntities(true);
                timelineCameras.SetActive(true);
                UIMenuRoot.SetActive(true);
                UIFlyByRoot.SetActive(false);
                FreeCamRoot.SetActive(false);
                ThirdPersonRoot.SetActive(false);
                UIFlybyMenu.SetActive(false);
                DemoCanvas.gameObject.SetActive(false);
                DemoCanvas.gameObject.SetActive(true);
                eventSystem.SetSelectedGameObject( UIFirstMenuButton.gameObject);
                RamdomCueManager.m_ListenerObject = RamdomCueManager.gameObject;

                break;
            case VideoMode.FlyBy:
                toggleTimelineLightingEntities(true);
                timelineCameras.SetActive(true);
                UIMenuRoot.SetActive(false);
                UIFlyByRoot.SetActive(true);
                FreeCamRoot.SetActive(false);
                ThirdPersonRoot.SetActive(false);
                UIFlybyMenu.SetActive(true);
                //UIFirstMenuButtonFlyby.Select();
                DemoCanvas.gameObject.SetActive(false);
                DemoCanvas.gameObject.SetActive(true);
                eventSystem.SetSelectedGameObject(UIFirstMenuButtonFlyby.gameObject);
                RamdomCueManager.m_ListenerObject = RamdomCueManager.gameObject;
                break;
            case VideoMode.FreeCamera:
                toggleTimelineLightingEntities(false);
                timelineCameras.SetActive(false);
                UIMenuRoot.SetActive(false);
                UIFlyByRoot.SetActive(false);
                FreeCamRoot.SetActive(true);
                ThirdPersonRoot.SetActive(false);
                UIFlybyMenu.SetActive(false);
                RamdomCueManager.m_ListenerObject = FreeCamRoot.GetComponentInChildren<Camera>().gameObject;
                break;
            case VideoMode.ThirdPerson:
                toggleTimelineLightingEntities(false);
                timelineCameras.SetActive(false);
                UIMenuRoot.SetActive(false);
                UIFlyByRoot.SetActive(false);
                FreeCamRoot.SetActive(false);
                ThirdPersonRoot.SetActive(true);
                UIFlybyMenu.SetActive(false);
                RamdomCueManager.m_ListenerObject = ThirdPersonRoot.GetComponentInChildren<Camera>().gameObject;
                break;
        }
    }

#endregion

    IEnumerator GameplayFadeTransition(float outDuration, float inDuration, Action action)
    {
        // Fade Out
        UIFadeImage.gameObject.SetActive(true);
        while(UIFadeImage.color.a < 1.0f)
        {
            float a = Mathf.Min(1.0f,UIFadeImage.color.a + (Time.deltaTime / Mathf.Max(0.0001f,outDuration)));
            UIFadeImage.color = new Color(0, 0, 0, a);
            yield return new WaitForEndOfFrame();
        }

        // Call Action...
        action.Invoke();

        // Then Fade In again
        while(UIFadeImage.color.a > 0.0f)
        {
            float a = Mathf.Max(0.0f,UIFadeImage.color.a - (Time.deltaTime / Mathf.Max(0.0001f,inDuration)));
            UIFadeImage.color = new Color(0, 0, 0, a);
            yield return new WaitForEndOfFrame();
        }
        UIFadeImage.gameObject.SetActive(false);
        m_FadeCoroutine = null;
    }

}
