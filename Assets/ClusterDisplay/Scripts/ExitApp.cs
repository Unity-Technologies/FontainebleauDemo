using Unity.ClusterRendering;
using UnityEngine;

public class ExitApp : MonoBehaviour
{
    void Update()
    {
        if (ClusterSynch.Active)
        {
            if (Input.GetKey(KeyCode.LeftControl) && Input.GetKeyDown(KeyCode.Q))
                ClusterSynch.Instance.ShutdownAllClusterNodes();
        }
        else
        {
            if (ClusterSynch.Terminated)
                Application.Quit(0);
        }
    }
}
