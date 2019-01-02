using UnityEngine;
using UnityEditor;
using System.IO;


public class LightUnitConverterWindow : EditorWindow
{

    Color m_sRGB;
    Color m_RGB;


    [MenuItem("Window/Light Unit Converter")]
    private static void Init()
    {
        var window = GetWindow<LightUnitConverterWindow>();
        window.titleContent = new GUIContent("Light Unit Converter");
        window.minSize = new Vector2(200, 100);
        window.Show();
    }

    

    private void OnEnable()
    {
    }
    private void OnGUI()
    {

        m_sRGB = EditorGUILayout.ColorField("sRGB", m_sRGB);
        m_RGB = EditorGUILayout.ColorField("RGB", m_RGB);

        m_RGB = m_sRGB.linear;

        using (new EditorGUILayout.HorizontalScope())
        {
            if (GUILayout.Button("Copy RGB Value 0-255", EditorStyles.miniButtonLeft))
            {
                EditorGUIUtility.systemCopyBuffer = Mathf.RoundToInt(m_RGB.r * 255).ToString() + "\t" + Mathf.RoundToInt(m_RGB.g * 255).ToString() + "\t" + Mathf.RoundToInt(m_RGB.b * 255).ToString();
            }
            if (GUILayout.Button("Copy RGB Value 0.0 - 1.0 ", EditorStyles.miniButtonMid))
            {
                EditorGUIUtility.systemCopyBuffer = m_RGB.r.ToString() + "\t" + m_RGB.g.ToString() + "\t" + m_RGB.b.ToString();
            }
        }

    }

}

