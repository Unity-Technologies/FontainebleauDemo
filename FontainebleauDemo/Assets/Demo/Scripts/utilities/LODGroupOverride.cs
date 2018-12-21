using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class LODGroupOverride : MonoBehaviour {

    public float lodSize = 1.0f;
    public Vector3 localReference = Vector3.zero;

	// Use this for initialization
	void OnStart () {
        GetComponent<LODGroup>().size = lodSize;
        GetComponent<LODGroup>().localReferencePoint = localReference;
    }

#if UNITY_EDITOR
    void Update () {
        if (GetComponent<LODGroup>().size != lodSize)
            GetComponent<LODGroup>().size = lodSize;
        if (GetComponent<LODGroup>().localReferencePoint != localReference)
            GetComponent<LODGroup>().localReferencePoint = localReference;
    }
#endif
}
