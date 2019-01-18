using UnityEngine.Events;

namespace GameplayIngredients.Actions
{
    public class SimpleAction : ActionBase
    {
        public UnityEvent OnExecute;

        public override void Execute()
        {
            OnExecute.Invoke();
        }
    }
}
