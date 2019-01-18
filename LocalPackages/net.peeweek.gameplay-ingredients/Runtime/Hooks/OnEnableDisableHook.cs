using UnityEngine.Events;

namespace GameplayIngredients.Hooks
{
    public class OnEnableDisableHook : HookBase
    {
        public UnityEvent OnEnableEvent;
        public UnityEvent OnDisableEvent;

        private void OnEnable()
        {
            OnEnableEvent.Invoke();
        }

        private void OnDisable()
        {
            OnDisableEvent.Invoke();
        }
    }
}

