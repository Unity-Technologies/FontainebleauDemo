using UnityEngine;
using UnityEngine.Events;

namespace GameplayIngredients.Actions
{
    public class EditorOnlyAction : ActionBase
    {
        public enum Mode
        {
            EditorOnly,
            PlayerOnly
        }

        public Mode ExecutionPath;

        public UnityEvent OnExecute;

        public override void Execute()
        {
            switch(ExecutionPath)
            {
                case Mode.EditorOnly:
                    if (Application.isEditor) OnExecute.Invoke();
                    break;
                case Mode.PlayerOnly:
                    if (!Application.isEditor) OnExecute.Invoke();
                    break;
            }
        }
    }
}
