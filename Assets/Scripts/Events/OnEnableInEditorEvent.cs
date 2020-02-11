using NaughtyAttributes;
using UnityEngine;

namespace GameplayIngredients.Events
{
    [ExecuteInEditMode]
    public class OnEnableInEditorEvent : EventBase
    {
        [ReorderableList]
        public Callable[] OnEnableEvent;
#if UNITY_EDITOR
        private void OnEnable()
        {
            if(!Application.isPlaying && OnEnableEvent.Length >0) Callable.Call(OnEnableEvent, gameObject);
        }
#endif
    }
}