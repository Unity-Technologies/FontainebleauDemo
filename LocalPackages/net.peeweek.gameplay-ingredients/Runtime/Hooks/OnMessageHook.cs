using UnityEngine.Events;

namespace GameplayIngredients.Hooks
{
    public class OnMessageHook : HookBase
    {
        public string MessageName = "Message";

        public UnityEvent Event;

        void OnEnable()
        {
            Messager.RegisterEvent(MessageName, Execute);
        }

        void OnDisable()
        {
            Messager.UnregisterEvent(MessageName, Execute);
        }

        void Execute()
        {
            Event.Invoke();
        }


    }
}
