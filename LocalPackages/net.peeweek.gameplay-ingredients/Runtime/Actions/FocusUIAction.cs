using UnityEngine.UI;

namespace GameplayIngredients.Actions
{
    public class FocusUIAction : ActionBase
    {
        public Selectable UIObjectToFocus;

        public override void Execute()
        {
            if (UIObjectToFocus != null)
                UIObjectToFocus.Select();
        }
    }
}
