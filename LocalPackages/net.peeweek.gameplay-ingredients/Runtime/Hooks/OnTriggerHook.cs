using UnityEngine;
using UnityEngine.Events;

namespace GameplayIngredients.Hooks
{
    public class OnTriggerHook : HookBase
    {
        public int EnterMaxCount = 0;
        public int ExitMaxCount = 0;

        private int m_RemainingEnterCount;
        private int m_RemainingExitCount;

        public UnityEvent onTriggerEnter;
        public UnityEvent onTriggerExit;

        public bool OnlyInteractWithTag = true;
        public string Tag = "Player";

        void Start()
        {
            m_RemainingEnterCount = EnterMaxCount;
            m_RemainingExitCount = ExitMaxCount;
        }

        private void OnTriggerEnter(Collider other)
        {
            if (EnterMaxCount > 0)
            {
                if (m_RemainingEnterCount == 0) return;
                m_RemainingEnterCount--;
            }
            if (OnlyInteractWithTag && other.tag == Tag )
            {
                onTriggerEnter.Invoke();
            }
            if (!OnlyInteractWithTag)
            {
                onTriggerEnter.Invoke();
            }
        }

        private void OnTriggerExit(Collider other)
        {
            if (ExitMaxCount > 0)
            {
                if (m_RemainingExitCount == 0) return;
                m_RemainingExitCount--;
            }
            if (OnlyInteractWithTag && other.tag == Tag )
            {
                onTriggerExit.Invoke();
            }
            if (!OnlyInteractWithTag)
            {
                onTriggerExit.Invoke();
            }
        }
    }
}
