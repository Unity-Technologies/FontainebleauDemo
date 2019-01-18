using System.Collections;
using UnityEngine;
using UnityEngine.Events;

namespace GameplayIngredients.Logic
{
    public class DelayedLogic : LogicBase
    {
        public float Delay = 1.0f;
        public UnityEvent OnDelayComplete;
        public UnityEvent OnCanceled;

        IEnumerator m_Coroutine;

        public void Cancel()
        {
            if(m_Coroutine != null)
            {
                StopCoroutine(m_Coroutine);
                OnCanceled.Invoke();
                m_Coroutine = null;
            }
        }

        public override void Execute()
        {
            if (m_Coroutine != null) Cancel();

            m_Coroutine = RunDelay(Delay);
            StartCoroutine(m_Coroutine);
        }

        IEnumerator RunDelay(float Seconds)
        {
            yield return new WaitForSeconds(Seconds);
            OnDelayComplete.Invoke();
            m_Coroutine = null;
        }
    }
}

