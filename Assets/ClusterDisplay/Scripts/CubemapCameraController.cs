using System;
using UnityEngine;

[ExecuteInEditMode]
public class CubemapCameraController : MonoBehaviour
{
    static readonly Vector3[] k_Orientations = new[]
    {
        new Vector3(0, 0, 0), 
        new Vector3(0, 1, 0), 
        new Vector3(0, 2, 0), 
        new Vector3(0, 3, 0),
        new Vector3(-1, 0, 0), // ceiling
        new Vector3(1, 0, 0), // bottom 
    };
    
    int m_Index;

    void OnEnable()
    {
        m_Index = 0;
    }

    void Update()
    {
        // B + left or right arrow to switch orientation
        if (Input.GetKey(KeyCode.B))
        {
            if (Input.GetKeyDown(KeyCode.RightArrow))
                Next();
            else if (Input.GetKeyDown(KeyCode.LeftArrow))
                Previous();
        }
    }

    public void Next()
    {
        m_Index = (m_Index + 1) % k_Orientations.Length;
        SetOrientation(m_Index);
    }

    public void Previous()
    {
        m_Index = (m_Index - 1 + k_Orientations.Length) % k_Orientations.Length;
        SetOrientation(m_Index);
    }

    // we deliberately set transform outside update to avoid conflict with other camera controllers
    void SetOrientation(int index)
    {
        var camera = Camera.main;
        if (camera != null)
        {
            camera.transform.rotation = Quaternion.Euler(k_Orientations[index] * 90);
        }
    }
}
