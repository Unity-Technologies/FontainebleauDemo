using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;

public abstract class InfoPanelInteraction : MonoBehaviour
{
    public int executionOrder;
    public abstract void OnActivate();
}