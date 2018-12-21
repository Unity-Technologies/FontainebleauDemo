using UnityEngine;

public class DisablePerPlatform : MonoBehaviour {

    public bool showOnPC;
    public bool showOnPS4;
    public bool showOnXbox;

	// Use this for initialization
	void Start ()
    {
        gameObject.SetActive(showOnPC);

        switch(Application.platform)
        {
            case RuntimePlatform.PS4:
                {
                    gameObject.SetActive(showOnPS4);
                    break;
                }
            case RuntimePlatform.XboxOne:
                {
                    gameObject.SetActive(showOnXbox);
                    break;
                }
        }
    }
}
