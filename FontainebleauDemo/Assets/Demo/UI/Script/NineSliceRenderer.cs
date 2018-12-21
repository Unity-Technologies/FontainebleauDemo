using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
public class NineSliceRenderer : MonoBehaviour
{
    public float MinWidth { get { return Border.x + Border.y;  } }
    public float MinHeight { get { return Border.z + Border.w;  } }

    // Order Left Right Top Bottom
    public Vector4 NineSlices = new Vector4(0.1f, 0.1f, 0.1f, 0.1f);
    public Vector4 Border = new Vector4(0.1f, 0.1f, 0.1f, 0.1f);

    public Color PanelColor = Color.white;

	// Use this for initialization
	void Start ()
    {
		UpdateMesh();
	}
	
	// Update is called once per frame
	void Update ()
    {
        UpdateMesh();
    }

    void UpdateMesh()
    {
        var filter = GetComponent<MeshFilter>();

        Mesh mesh;
        if (filter.sharedMesh == null)
        {
            mesh = new Mesh();
            filter.sharedMesh = mesh;
        }
        else
            mesh = filter.sharedMesh;

        var panelsize = new Vector2(1, 1);
        var bordersize = new Vector4(Border.x / transform.localScale.x, Border.y / transform.localScale.x, Border.z / transform.localScale.y, Border.w / transform.localScale.y);

        var vertices = new List<Vector3>();
        vertices.Add(new Vector3(0.0f,                           0.0f));
        vertices.Add(new Vector3(bordersize.x,                  0.0f));
        vertices.Add(new Vector3(panelsize.x - bordersize.y,    0.0f));
        vertices.Add(new Vector3(panelsize.x,                    0.0f));

        vertices.Add(new Vector3(0.0f,                           -bordersize.z));
        vertices.Add(new Vector3(bordersize.x,                  -bordersize.z));
        vertices.Add(new Vector3(panelsize.x - bordersize.y,    -bordersize.z));
        vertices.Add(new Vector3(panelsize.x,                    -bordersize.z));

        vertices.Add(new Vector3(0.0f,                           -panelsize.y + bordersize.w));
        vertices.Add(new Vector3(bordersize.x,                  -panelsize.y + bordersize.w));
        vertices.Add(new Vector3(panelsize.x - bordersize.y,    -panelsize.y + bordersize.w));
        vertices.Add(new Vector3(panelsize.x,                    -panelsize.y + bordersize.w));

        vertices.Add(new Vector3(0.0f,                           -panelsize.y));
        vertices.Add(new Vector3(bordersize.x,                  -panelsize.y));
        vertices.Add(new Vector3(panelsize.x - bordersize.y,    -panelsize.y));
        vertices.Add(new Vector3(panelsize.x,                   -panelsize.y));

        var uvs = new List<Vector2>();
        uvs.Add(new Vector2(0.0f,               1.0f));
        uvs.Add(new Vector2(NineSlices.x,       1.0f));
        uvs.Add(new Vector2(1.0f-NineSlices.y,  1.0f));
        uvs.Add(new Vector2(1.0f,               1.0f));

        uvs.Add(new Vector2(0.0f,               1.0f-NineSlices.w));
        uvs.Add(new Vector2(NineSlices.x,       1.0f-NineSlices.w));
        uvs.Add(new Vector2(1.0f-NineSlices.y,  1.0f-NineSlices.w));
        uvs.Add(new Vector2(1.0f,               1.0f-NineSlices.w));

        uvs.Add(new Vector2(0.0f,               NineSlices.z));
        uvs.Add(new Vector2(NineSlices.x,       NineSlices.z));
        uvs.Add(new Vector2(1.0f-NineSlices.y,  NineSlices.z));
        uvs.Add(new Vector2(1.0f,               NineSlices.z));

        uvs.Add(new Vector2(0.0f,               0.0f));
        uvs.Add(new Vector2(NineSlices.x,       0.0f));
        uvs.Add(new Vector2(1.0f-NineSlices.y,  0.0f));
        uvs.Add(new Vector2(1.0f,               0.0f));

        var colors = new List<Color>();
        for (int i = 0; i < 16; i++)
            colors.Add(this.PanelColor);

        int[] indices = new int[54]
        { 0, 1, 4, 4, 1, 5,
          1, 2, 5, 5, 2, 6,
          2, 3, 6, 6, 3, 7,
          4, 5, 8, 8, 5, 9,
          5, 6, 9, 9, 6, 10,
          6, 7, 10, 10, 7, 11,
          8, 9, 12, 12, 9, 13,
          9, 10, 13, 13, 10, 14,
          10, 11, 14, 14, 11, 15
        };

        mesh.SetVertices(vertices);
        mesh.SetColors(colors);
        mesh.SetUVs(0,uvs);
        mesh.SetIndices(indices, MeshTopology.Triangles, 0);

        mesh.MarkDynamic();
        mesh.RecalculateBounds();
    }

}
