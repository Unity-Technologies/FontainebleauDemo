using System;
using Cinemachine;
using UnityEngine;
using UnityEngine.Rendering.HighDefinition;

[RequireComponent(typeof(CinemachineBrain))]
public class ResetVolumetricsOnCut : MonoBehaviour
{
    CinemachineBrain m_Brain;

    void OnEnable()
    {
        m_Brain = GetComponent<CinemachineBrain>();
        m_Brain.m_CameraCutEvent.AddListener(CutHandler);
    }

    void OnDisable()
    {
        m_Brain.m_CameraCutEvent.RemoveListener(CutHandler);
    }

    void CutHandler(CinemachineBrain brain)
    {
        var cam = brain.OutputCamera;
        if (cam != null)
        {
            HDCamera hdCam = HDCamera.GetOrCreate(cam);
            hdCam.Reset();
            hdCam.volumetricHistoryIsValid = false;
            hdCam.colorPyramidHistoryIsValid = false;
        }
    }
}
