using System;
using System.Collections.Generic;
using System.Security.AccessControl;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.Rendering;

[ExecuteInEditMode]
public class OcclusionRenderer : MonoBehaviour
{
    [SerializeField]
    Transform[] m_OccludingRoots;
    
    [SerializeField]
    Mesh[] m_OccludingMeshes;

    [SerializeField]
    Vector2Int m_RenderTargetSize;
    
    Material m_Material;

    [SerializeField]
    bool m_ShowDebugUI;
    
    RenderTexture m_ColorBuffer;
    RenderTexture m_DepthBuffer;
    CommandBuffer m_CmdBuffer;
    
    struct InstancedDrawArgs
    {
        public Mesh mesh;
        public List<Matrix4x4> transforms; // TODO use arrays if we use CommandBuffer
    }
    
    List<InstancedDrawArgs> m_RenderingData = new List<InstancedDrawArgs>();

    class InstancingDataGenerationVisitor
    {
        Dictionary<int, Mesh> m_Meshes = new Dictionary<int, Mesh>();
        Dictionary<int, List<Matrix4x4>> m_Transforms = new Dictionary<int, List<Matrix4x4>>();
        HashSet<int> m_Filter = new HashSet<int>();
        
        public void Reset()
        {
            m_Meshes.Clear();
            m_Transforms.Clear();
        }

        public void SetAllowedMeshes(Mesh[] meshes)
        {
            m_Filter.Clear();
            foreach (var mesh in meshes)
                m_Filter.Add(mesh.GetInstanceID());
        }

        public void Visit(Transform trs)
        {
            var filter = trs.GetComponent<MeshFilter>();
            if (filter == null)
                return;
            
            var mesh = filter.sharedMesh;
            if (mesh == null)
                return;

            AppendIfAllowed(mesh, trs.localToWorldMatrix);
        }

        public void PopulateRenderingData(List<InstancedDrawArgs> renderingData)
        {
            renderingData.Clear();

            foreach (var entry in m_Meshes)
            {
                renderingData.Add(new InstancedDrawArgs
                {
                    mesh = entry.Value,
                    transforms = m_Transforms[entry.Key]
                });
            }
        }

        void AppendIfAllowed(Mesh mesh, Matrix4x4 transform)
        {
            var meshId = mesh.GetInstanceID();

            if (!m_Filter.Contains(meshId))
                return;

            if (m_Meshes.ContainsKey(meshId))
            {
                m_Transforms[meshId].Add(transform);
            }
            else 
            {
                m_Meshes.Add(meshId, mesh);
                var list = new List<Matrix4x4>();
                list.Add(transform);
                m_Transforms.Add(meshId, list);
            }
        }
    }
    
    InstancingDataGenerationVisitor m_InstancingDataGenerationVisitor = new InstancingDataGenerationVisitor();

    void OnEnable()
    {
        var shader = Shader.Find("HDRP/Lit");
        m_Material = new Material(shader);
        m_Material.enableInstancing = true;
        m_CmdBuffer = new CommandBuffer();
    }

    void OnDisable()
    {
        DestroyImmediate(m_Material);
        m_CmdBuffer.Release();
        if (m_DepthBuffer != null)
            m_DepthBuffer.Release();
        m_DepthBuffer = null;
        if (m_ColorBuffer != null)
            m_ColorBuffer.Release();
        m_ColorBuffer = null;
    }

    void Update()
    {
        if (m_Material == null)
            return;

        var camera = Camera.main;
        if (camera != null && camera.cameraType == CameraType.Game)
            Render(camera);
    }

    void OnGUI()
    {
        if (m_ShowDebugUI && m_DepthBuffer != null) 
            GUI.DrawTexture(new Rect(0, 0, m_DepthBuffer.width, m_DepthBuffer.height), m_DepthBuffer);        
    }

    // TODO optim only render if camera moved
    void Render(Camera camera)
    {
        if (m_DepthBuffer == null || m_DepthBuffer.width != m_RenderTargetSize.x || m_DepthBuffer.height != m_RenderTargetSize.y)
        {
            if (m_DepthBuffer != null)
                m_DepthBuffer.Release();
            if (m_ColorBuffer != null)
                m_ColorBuffer.Release();
            
            m_DepthBuffer = new RenderTexture(m_RenderTargetSize.x, m_RenderTargetSize.y, 0, RenderTextureFormat.Depth);
            m_ColorBuffer = new RenderTexture(m_RenderTargetSize.x, m_RenderTargetSize.y, 0, RenderTextureFormat.Default);
        }

        m_CmdBuffer.Clear();
        m_CmdBuffer.SetRenderTarget(m_ColorBuffer.colorBuffer, m_DepthBuffer.colorBuffer);
        m_CmdBuffer.SetViewProjectionMatrices(camera.worldToCameraMatrix, camera.projectionMatrix);
        m_CmdBuffer.ClearRenderTarget(true, true, Color.black);
        
        foreach (var args in m_RenderingData)
            m_CmdBuffer.DrawMeshInstanced(args.mesh, 0, m_Material, 0, args.transforms.ToArray());
        
        Graphics.ExecuteCommandBuffer(m_CmdBuffer);
    }

    [ContextMenu("Update Rendering Data")]
    void UpdateRenderingData()
    {
        if (m_OccludingRoots == null)
            return;
        
        m_InstancingDataGenerationVisitor.Reset();
        m_InstancingDataGenerationVisitor.SetAllowedMeshes(m_OccludingMeshes);

        foreach (var occluder in m_OccludingRoots)
        {
            UpdateRenderingDataRecursive(occluder.transform, m_InstancingDataGenerationVisitor);
        }
        
        m_InstancingDataGenerationVisitor.PopulateRenderingData(m_RenderingData);
        
        // TMP DEBUG
        Debug.Log("INSTANCING RENDERING DATA:");
        foreach (var data in m_RenderingData)
        {
            Debug.Log($"Mesh name[{data.mesh.name}] id[{data.mesh.GetInstanceID()}] count[{data.transforms.Count}]");
        }
    }
    
    static void UpdateRenderingDataRecursive(Transform parent, InstancingDataGenerationVisitor visitor) 
    {
        foreach (Transform child in parent)
        {
            visitor.Visit(child);
            UpdateRenderingDataRecursive(child, visitor);
        }
    }
}
