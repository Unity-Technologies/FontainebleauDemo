using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
public class QuadRenderer : MonoBehaviour
{
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
       
        var vertices = new List<Vector3>();
        vertices.Add(new Vector3(-0.5f, -0.5f));
        vertices.Add(new Vector3(-0.5f, 0.5f));
        vertices.Add(new Vector3(0.5f, 0.5f));
        vertices.Add(new Vector3(0.5f, -0.5f));


        var uvs = new List<Vector2>();
        uvs.Add(new Vector2(0.0f, 0.0f));
        uvs.Add(new Vector2(0.0f, 1.0f));
        uvs.Add(new Vector2(1.0f, 1.0f));
        uvs.Add(new Vector2(1.0f, 0.0f));

        var colors = new List<Color>();
        for (int i = 0; i < 4; i++)
            colors.Add(this.PanelColor);

        int[] indices = new int[6]
        {
            0, 1, 2, 0 ,2 , 3
        };

        mesh.SetVertices(vertices);
        mesh.SetColors(colors);
        mesh.SetUVs(0,uvs);
        mesh.SetIndices(indices, MeshTopology.Triangles, 0);

        mesh.MarkDynamic();
        mesh.RecalculateBounds();
    }

}
