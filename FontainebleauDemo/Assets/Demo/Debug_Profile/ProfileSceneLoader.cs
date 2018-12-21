using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProfileSceneLoader : MonoBehaviour {

	// Use this for initialization
	void Start () {
        UnityEngine.SceneManagement.SceneManager.LoadScene("Demo_GR", UnityEngine.SceneManagement.LoadSceneMode.Additive);
        UnityEngine.SceneManagement.SceneManager.LoadScene("Demo_LI_Day", UnityEngine.SceneManagement.LoadSceneMode.Additive);
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
