using System;
using System.Collections.Generic;
using UnityEngine;

namespace GameplayIngredients
{
    public static class Messager
    {
        private static Dictionary<string, Action> m_RegisteredEvents;

        static Messager()
        {
            m_RegisteredEvents = new Dictionary<string, Action>();
        }


        public static void RegisterEvent(string eventName, Action action)
        {
            if (!m_RegisteredEvents.ContainsKey(eventName))
                m_RegisteredEvents.Add(eventName, action);
            else
                m_RegisteredEvents[eventName] += action;
        }

        public static void UnregisterEvent(string eventName, Action action)
        {
            var currentEvent = m_RegisteredEvents[eventName];
            currentEvent -= action;
            if (currentEvent == null)
                m_RegisteredEvents.Remove(eventName);
        }

        public static void Send(string eventName)
        {
            Debug.Log(string.Format("[MessageManager] Broadcast: {0}", eventName));

            if (m_RegisteredEvents.ContainsKey(eventName))
            {
                m_RegisteredEvents[eventName]();
            }
            else
            {
                Debug.Log("[MessageManager] could not find any listeners for event : " + eventName);
            }
        }
    }
}

