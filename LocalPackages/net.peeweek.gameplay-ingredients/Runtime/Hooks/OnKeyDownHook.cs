using UnityEngine;
using UnityEngine.Events;

namespace GameplayIngredients.Hooks
{
    public class OnKeyDownAction : HookBase
    {
        public enum ActionType
        {
            KeyDown,
            KeyUp
        }

        public KeyCode Key = KeyCode.F5;

        public UnityEvent OnKeyDownEvent;
        public UnityEvent OnKeyUpEvent;

        void Update()
        {
            if (Input.GetKeyDown(Key))
                OnKeyDownEvent.Invoke();

            if (Input.GetKeyUp(Key))
                OnKeyUpEvent.Invoke();
        }
    }
}


