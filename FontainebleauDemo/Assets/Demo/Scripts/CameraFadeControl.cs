using UnityEngine;
using UnityEngine.UI;
using System.Collections;

[ExecuteInEditMode]
public class CameraFadeControl : MonoBehaviour
{
    public float Opacity = 0.0f;
    public Image FadeImage;

    // Use this for initialization
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        if (Opacity > 0)
            FadeImage.gameObject.SetActive(true);
        else
            FadeImage.gameObject.SetActive(false);

        Color c = FadeImage.color;
        FadeImage.color = new Color(c.r, c.g, c.b, Opacity);
        
    }
}
