using System.Collections;
using UnityEngine;
using UnityEngine.Events;

namespace GameplayIngredients.Logic
{
    public class NextFrameLogic : LogicBase
    {
        public UnityEvent OnComplete;
        IEnumerator m_Coroutine;

        public override void Execute()
        {
            m_Coroutine = RunDelay();
            StartCoroutine(m_Coroutine);
        }

        IEnumerator RunDelay()
        {
            yield return new WaitForEndOfFrame();
            OnComplete.Invoke();
            m_Coroutine = null;
        }
    }
}

