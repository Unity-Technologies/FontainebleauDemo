using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class InfoPanel : MonoBehaviour
{

    public enum AlignMode
    {
        Top, Down, Left, Right
    }

    [Header("InfoPanel Configuration")]
    public string Title = "Title";

    [Multiline]
    public string Body = @"Lorem ipsum dolor sit amet, consectetur adipiscing elit, sed do eiusmod tempor
incididunt ut labore et dolore magna aliqua. Ut enim ad minim veniam, quis 
nostrud exercitation ullamco laboris nisi ut aliquip ex ea commodo consequat.
Duis aute irure dolor in reprehenderit in voluptate velit esse cillum dolore eu 
fugiat nulla pariatur. Excepteur sint occaecat cupidatat non proident, sunt in 
culpa qui officia deserunt mollit anim id est laborum.";

    public Color PanelColor = new Color(0.1f, 0.1f, 0.1f, 0.85f);
    public Color TitleColor = new Color(1.0f,1.0f,1.0f,1.0f);
    public Color BodyColor = new Color(1.0f,1.0f,1.0f,0.80f);


    public AlignMode Align = AlignMode.Top;
    public Vector2 Size = new Vector2(1.4f, 2.0f);

    [Header("Links (Do not touch)")]
    public TextMesh TitleObject;
    public TextMesh BodyObject;
    public GameObject[] OtherSubObjects;
    public NineSliceRenderer PanelObject;
    public GameObject Root;
    public string CameraName = "FirstPersonCharacter";



    private Camera TargetCamera;
    private Vector3 m_BaseForward;

	// Use this for initialization
	void Start ()
    {
        m_BaseForward = transform.forward;
        TargetCamera = GameObject.Find(CameraName).GetComponent<Camera>();
		Layout();
	}
	
	// Update is called once per frame
	void Update ()
    {
        if(!Application.isPlaying)
        {
            Layout();
            return;
        }

        if(TargetCamera != null)
        {
            Vector3 fwd = Vector3.Normalize(transform.position-TargetCamera.transform.position);

            if (Vector3.Dot(fwd, m_BaseForward) > 0)
                transform.forward = m_BaseForward;
            else
                transform.forward = -m_BaseForward;
        }
	}

    public void Layout()
    {
        TitleObject.text = Title;
        BodyObject.text = Body;

        PanelObject.transform.localScale = new Vector2(Mathf.Max(PanelObject.MinWidth,Size.x),Mathf.Max(PanelObject.MinHeight,Size.y));
        PanelObject.PanelColor = PanelColor;

        TitleObject.color = TitleColor;
        BodyObject.color = BodyColor;

        switch(Align)
        {
            case AlignMode.Down:
                Root.transform.localPosition = new Vector2(-Size.x / 2, Size.y);
                break;
            case AlignMode.Top:
                Root.transform.localPosition = new Vector2(-Size.x / 2, 0.0f);
                break;
            case AlignMode.Left:
                Root.transform.localPosition = new Vector2(0.0f, Size.y/2);
                break;
            case AlignMode.Right:
                Root.transform.localPosition = new Vector2(Size.x, Size.y/2);
                break;
        }


    }
}
