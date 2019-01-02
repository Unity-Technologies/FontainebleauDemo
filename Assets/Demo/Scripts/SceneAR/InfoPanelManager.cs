using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;

public class InfoPanelManager : MonoBehaviour
{
    private int interactionSelector = 0;

    private InfoPanelInteraction[] interactions;

    private void Start()
    {
        interactions = GetComponents<InfoPanelInteraction>();
    }

    private void Update()
    {
        if (Input.GetButtonDown("Fire1"))
        {
            Debug.Log("length"+ interactions.Length);
            if (interactionSelector <= interactions.Length) interactionSelector = interactionSelector + 1;
            if (interactionSelector > interactions.Length) interactionSelector = 1;

            foreach (InfoPanelInteraction interaction in interactions)
            {
                if (interaction.executionOrder == interactionSelector) interaction.OnActivate();
                Debug.Log("interaction" + interactionSelector);
            }
        }
    }
}