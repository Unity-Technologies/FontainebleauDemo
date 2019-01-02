using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FPVManager : MonoBehaviour
{
    public GameObject FPVController;
    public GameObject StartPoint;

    public float ZKillHeight = -20.0f;
    public Image UIFadeImage;
    void OnEnable()
    {
        UIFadeImage.color = new Color(0, 0, 0, 0);
        UIFadeImage.gameObject.SetActive(false);
        Respawn();
    }

    void Respawn()
    {
        FPVController.transform.position = StartPoint.transform.position;
        FPVController.transform.rotation = StartPoint.transform.rotation;
    }
	
	// Update is called once per frame
	void Update ()
    {
        if (FPVController.transform.position.y < ZKillHeight)
            StartCoroutine(FallRespawnCoroutine());
            
	}

    IEnumerator FallRespawnCoroutine()
    {
        // Fade Out
        UIFadeImage.gameObject.SetActive(true);
        while (UIFadeImage.color.a < 1.0f)
        {
            float a = Mathf.Min(1.0f, UIFadeImage.color.a + (Time.deltaTime / 0.5f));
            UIFadeImage.color = new Color(0, 0, 0, a);
            yield return new WaitForEndOfFrame();
        }

        Respawn();

        // Then Fade In again
        while (UIFadeImage.color.a > 0.0f)
        {
            float a = Mathf.Max(0.0f,UIFadeImage.color.a - (Time.deltaTime /0.5f));
            UIFadeImage.color = new Color(0, 0, 0, a);
            yield return new WaitForEndOfFrame();
        }
        UIFadeImage.gameObject.SetActive(false);

    }
}
