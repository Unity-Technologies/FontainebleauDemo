using System;
using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using UnityEngine;
using UnityEngine.Rendering.HighDefinition;

/// <summary>
/// Temporal effects needs to be reset in case of camera cut,
/// this is already handled in CinemachineVolumeSettings.OnCameraCut.
/// But Cinemachine cuts are not enough,
/// we are likely to see artefacts whenever the view changes abruptly
/// between two frames, and Cinemachine cut events only reflect virtual camera switches.
/// We must track camera movement ourselves and reset temporal effects based on camera motion
/// </summary>
[RequireComponent(typeof(CinemachineBrain))]
public class AutoCameraCutController : MonoBehaviour
{
    [SerializeField]
    float m_Threshold;
    
    CinemachineBrain m_Brain;
    Vector3 m_PrevPosition;

    const int k_WindowSize = 12;
    float[] m_VelocityBuffer = new float[k_WindowSize];
    int index;

    void OnEnable()
    {
        m_Brain = GetComponent<CinemachineBrain>();
        m_PrevPosition = m_Brain.transform.position;
        index = 0;
        
        // reset buffers
        for (var i = 0; i != k_WindowSize; ++i)
            m_VelocityBuffer[i] = 0;
    }

    void LateUpdate()
    {
        var position = m_Brain.OutputCamera.transform.position;

        // compute current instantaneous value
        var timeScale = 1.0f / Time.deltaTime;
        var velocity = (position - m_PrevPosition).magnitude * timeScale;
        // don't forget to update previous value
        m_PrevPosition = position;
        
        // compute average so that we can compare against it
        var averageVelocity = 0f;
        var mul = 1.0f / k_WindowSize;
        for (var i = 0; i != k_WindowSize; ++i)
            averageVelocity += m_VelocityBuffer[i];
        averageVelocity /= (float)k_WindowSize;
        
        // write current values to buffers
        m_VelocityBuffer[index] = velocity;
        index = (index + 1) % k_WindowSize; // circular buffer
        
        // reset camera temporal effect on sudden motion
        var velocityCrossesThreshold = velocity > averageVelocity * (1 + m_Threshold);
        if (velocityCrossesThreshold)
        {
            // to prevent continuous firing related to buffer length,
            // full buffer update on threshold cross, it forces averages up
            for (var i = 0; i != k_WindowSize; ++i)
                m_VelocityBuffer[i] = velocity;
            
            Reset(m_Brain.OutputCamera);
            Debug.Log("Automatic Temporal Effects Reset.");
        }
    }

    static void Reset(Camera camera)
    {
        HDCamera hdCam = HDCamera.GetOrCreate(camera);
        hdCam.volumetricHistoryIsValid = false;
        hdCam.colorPyramidHistoryIsValid = false;
        hdCam.Reset();
    }
}
