using UnityEngine;
using UnityEngine.Events;

namespace GameplayIngredients.Hooks
{
    public class OnStartHook : HookBase
    {
        public UnityEvent OnStart;

        private void Start()
        {
            OnStart.Invoke();
        }
    }
}


