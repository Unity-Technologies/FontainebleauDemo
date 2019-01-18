using UnityEngine.Events;
using GameplayIngredients.LevelStreaming;

namespace GameplayIngredients.Actions
{
    public class StreamingLevelAction : ActionBase
    {
        public string[] Scenes;
        public string SceneToActivate;
        public LevelStreamingManager.StreamingAction Action = LevelStreamingManager.StreamingAction.Load;

        public bool ShowUI = false;

        public UnityEvent OnLoadComplete;

        public override void Execute()
        {
            LevelStreamingManager.instance.LoadScenes(Action, Scenes, SceneToActivate, ShowUI, OnLoadComplete);
        }
    }
}

