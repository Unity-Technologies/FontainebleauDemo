using UnityEngine;
using UnityEngine.Events;

namespace GameplayIngredients.Hooks
{
    public class OnAwakeHook : MonoBehaviour
    {
        public UnityEvent onAwake;

        private void Awake()
        {
            onAwake.Invoke();
        }
    }
}


