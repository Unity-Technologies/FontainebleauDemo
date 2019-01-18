using UnityEngine;

namespace GameplayIngredients.Actions
{
    public class SetTimeScaleAction : ActionBase
    {
        public float TimeScale = 1.0f;

        public override void Execute()
        {
            Time.timeScale = TimeScale;
        }

        public void SetTimeScale(float value)
        {
            TimeScale = value;
            Execute();
        }
    }
}
