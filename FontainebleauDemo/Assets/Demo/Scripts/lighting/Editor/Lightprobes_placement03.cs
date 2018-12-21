using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Collections;

public class CreateLightProbesInVolume : EditorWindow
{
    float HorizontalSpacing = 2.0f;
    float VerticalSpacing = 2.0f;
    float OffsetFomFloor = 0.5f;
    int numberOfLayers = 2;
    bool FillVolume = false;
    bool traceCollisions = true;

    [MenuItem("Lighting/Create Light Probes in Volume")]

    static void Init()
    {
        // Get existing open window or if none, make a new one:
        CreateLightProbesInVolume window = (CreateLightProbesInVolume)EditorWindow.GetWindow(typeof(CreateLightProbesInVolume), true, "Create Light Probes in Volume");
        window.position = new Rect((Screen.width / 2) - 125, Screen.height / 2 + 85, 300, 200 );
        window.Show();
        Debug.Log("Started Window", window);
    }

    void OnGUI()
    {
        EditorGUILayout.HelpBox("Adds a lightprobegroup as a child of the selected volume. Fills the volume with lightprobes placed on a grid based on Horizontal and Vertical Resolution.", MessageType.Info);
        HorizontalSpacing = EditorGUILayout.FloatField("Horizontal Resolution", HorizontalSpacing);
        VerticalSpacing = EditorGUILayout.FloatField("Vertical Resolution", VerticalSpacing);
        OffsetFomFloor = EditorGUILayout.FloatField("Offset From Floor", OffsetFomFloor);
        numberOfLayers = EditorGUILayout.IntField("Number of layers", numberOfLayers);
        FillVolume = EditorGUILayout.Toggle("Fill Volume",FillVolume);
        traceCollisions = EditorGUILayout.Toggle("Trace Collisions", traceCollisions);

        // Clamp values
        if (HorizontalSpacing < 0.1f) HorizontalSpacing = 0.1f;
        if (VerticalSpacing < 0.1f) VerticalSpacing = 0.1f;

        if (GUILayout.Button("Create Light Probes in Selected Volume"))
        {
            Bounds();
        }
    }

    void Bounds()
    {
        Debug.Log("Start light probe");
        // Get selection
        // TEMP disable multi selection
        //GameObject[] Select = Selection.gameObjects;
        //if (Select.Length < 1) Debug.Log("nothing selected");

        GameObject Select = Selection.activeGameObject;
        if ( Select == null ) Debug.Log("nothing selected");

        // Get total bounds for selected objects

        // First check that mesh has a collider attached to it
        // if it doesn't then we can't raycast so we should ignore this object
        Collider col = Select.GetComponent<Collider>();
        if (col == null) Debug.Log("Col not found", col);

        //Check if there is already a lightprobegroup component
        // if there is destroy it
        LightProbeGroup oldLightprobes = Select.GetComponent<LightProbeGroup>();
        if (oldLightprobes != null) DestroyImmediate(oldLightprobes);

        // Get the col bounds
        Bounds bbox = col.bounds;
        Select.GetComponent<BoxCollider>().enabled = false;

        // Update total bounds
        float minX = bbox.min.x;
        float minY = bbox.min.y;
        float minZ = bbox.min.z;
        float maxX = bbox.max.x;
        float maxY = bbox.max.y;
        float maxZ = bbox.max.z;

        // Now go through in a grid and attempt to place a light probe using raycasting
        float xCount = (maxX - minX)/HorizontalSpacing;
        float zCount = (maxZ - minZ) / HorizontalSpacing;
        float ycount = (maxY - minY) / VerticalSpacing;
        float startxoffset = ((maxX - minX) - (int)xCount * HorizontalSpacing) / 2;
        float startzoffset = ((maxZ - minZ) - (int)zCount * HorizontalSpacing) / 2;
        List<Vector3> VertPositions = new List<Vector3>();
        for (int z = 0; z < zCount; z++)
        {
            for (int x = 0; x < xCount; x++)
            {
                RaycastHit hit;
                    Ray ray = new Ray();
                    ray.origin = new Vector3(startxoffset + minX + x * HorizontalSpacing, maxY+1, startzoffset + minZ + z * HorizontalSpacing);
                    ray.direction = -Vector3.up;
                    Debug.DrawRay(ray.origin, ray.direction*5, Color.red, (maxY - minY));
                    if (Physics.Raycast(ray, out hit, (maxY - minY) * 2 ))
                    {
                        if ( hit.point.y + OffsetFomFloor < maxY && hit.point.y + OffsetFomFloor > minY) VertPositions.Add(hit.point + new Vector3(0 - Select.transform.position.x, OffsetFomFloor - Select.transform.position.y, 0 - Select.transform.position.z));

                        if (!FillVolume)
                        {
                            for (int i = 1; i < numberOfLayers; i++)
                            {
                                if (hit.point.y + OffsetFomFloor + i * VerticalSpacing < maxY && hit.point.y + OffsetFomFloor + VerticalSpacing > minY) VertPositions.Add(hit.point + new Vector3(0 - Select.transform.position.x, OffsetFomFloor + i * VerticalSpacing - Select.transform.position.y, 0 - Select.transform.position.z));
                            }
                        }
                        
                        if (FillVolume)
                        {
                            for (int j = 2; j < ycount; j++ )
                            {
                                if (hit.point.y + OffsetFomFloor + j*VerticalSpacing < maxY && hit.point.y + OffsetFomFloor + j * VerticalSpacing > minY ) VertPositions.Add(hit.point + new Vector3(0 - Select.transform.position.x, OffsetFomFloor + j*VerticalSpacing - Select.transform.position.y, 0 - Select.transform.position.z));
                            }
                        }
                    }
                    else Debug.Log("Miss");
            }
        }

        // Check if we have any hits
        if (VertPositions.Count < 1) Debug.Log("no valid hit");

        // Get _LightProbes game object
        //GameObject LightProbeGameObj = GameObject.Find("_LightProbes");
        //var LightProbeGameObj = new GameObject("_Lightprobes");
        Select.AddComponent<LightProbeGroup>();
        //LightProbeGameObj.transform.parent = Select.transform;
        if (Select == null) Debug.Log("Lightprobegroup not found");

        // Get light probe group component
        LightProbeGroup LPGroup = Select.GetComponent("LightProbeGroup") as LightProbeGroup;
        if (LPGroup == null) Debug.Log("Lightprobe component not found");

        // Create lightprobe positions
        Vector3[] ProbePos = new Vector3[VertPositions.Count];
        for (int i = 0; i < VertPositions.Count; i++)
        {
            ProbePos[i] = VertPositions[i];
        }

        // Set new light probes
        LPGroup.probePositions = ProbePos;
        Select.GetComponent<BoxCollider>().enabled = true;
        //Selection.activeGameObject = Select;
        Debug.Log("Finished Probe Calculations with: " + ProbePos.Length + "Probes.");
    }
}