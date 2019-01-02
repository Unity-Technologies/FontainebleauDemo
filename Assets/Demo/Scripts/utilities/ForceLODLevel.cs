using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class ForceLODLevel : MonoBehaviour {

    public int LODIndex = 0;

	// Use this for initialization
	void OnStart () {
        GetComponent<LODGroup>().ForceLOD(LODIndex);
    }
}
