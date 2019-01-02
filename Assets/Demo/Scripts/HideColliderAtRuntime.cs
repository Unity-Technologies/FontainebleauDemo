using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HideColliderAtRuntime : MonoBehaviour {

    private MeshRenderer colliderRenderer;

	// Use this for initialization
	void Start () {
        colliderRenderer = GetComponent<MeshRenderer>();
        if (colliderRenderer != null)
            colliderRenderer.enabled = false;
		
	}
}
