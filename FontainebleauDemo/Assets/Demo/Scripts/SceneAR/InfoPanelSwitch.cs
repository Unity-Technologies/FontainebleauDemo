using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InfoPanelSwitch : InfoPanelInteraction
{
    public GameObject disableGameobject;
    public GameObject enableGameobject;

    public override void OnActivate()
    {
        disableGameobject.SetActive(false);
        enableGameobject.SetActive(true);
    }
}
