using UnityEngine;

[ExecuteInEditMode]
public class SameRotationAs : MonoBehaviour
{
    public GameObject Target;

    void Update()
    {
        if(Target != null)
            transform.rotation = Target.transform.rotation;
    }

}
