using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;

/* Holds streams of data to override the colors or UVs on a mesh without making the mesh unique. This is more
 * memory efficient than burning the color data into many copies of a mesh, and much easier to manage. 
 * 
*/
namespace JBooth.VertexPainterPro
{
   [ExecuteInEditMode]
   public class VertexInstanceStream : MonoBehaviour
   {
      public bool keepRuntimeData = false;


      [HideInInspector]
      [SerializeField]
      private Color[] _colors;

      [HideInInspector]
      [SerializeField]
      private List<Vector4> _uv0;

      [HideInInspector]
      [SerializeField]
      private List<Vector4> _uv1;

      [HideInInspector]
      [SerializeField]
      private List<Vector4> _uv2;

      [HideInInspector]
      [SerializeField]
      private List<Vector4> _uv3;

      [HideInInspector]
      [SerializeField]
      private Vector3[] _positions;

      [HideInInspector]
      [SerializeField]
      private Vector3[] _normals;

      [HideInInspector]
      [SerializeField]
      private Vector4[] _tangents;

      public Color[] colors 
      { 
         get 
         { 
            return _colors; 
         }
         set
         {
            enforcedColorChannels = (! (_colors == null || (value != null && _colors.Length != value.Length)));
            _colors = value;
            Apply();
         }
      }

      public List<Vector4> uv0 { get { return _uv0; } set { _uv0 = value; Apply(); } }
      public List<Vector4> uv1 { get { return _uv1; } set { _uv1 = value; Apply(); } }
      public List<Vector4> uv2 { get { return _uv2; } set { _uv2 = value; Apply(); } }
      public List<Vector4> uv3 { get { return _uv3; } set { _uv3 = value; Apply(); } }
      public Vector3[] positions { get { return _positions; } set { _positions = value; Apply(); } }
      public Vector3[] normals { get { return _normals; } set { _normals = value; Apply(); } }
      public Vector4[] tangents { get { return _tangents; } set { _tangents = value; Apply(); } }

      #if UNITY_EDITOR
      Vector3[] cachedPositions;
      public Vector3 GetSafePosition(int index)
      {
         if (_positions != null && index < _positions.Length)
         {
            return _positions[index];
         }
         if (cachedPositions == null)
         {
            MeshFilter mf = GetComponent<MeshFilter>();
            if (mf == null || mf.sharedMesh == null)
            {
               Debug.LogError("No Mesh Filter or Mesh available");
               return Vector3.zero;
            }
            cachedPositions = mf.sharedMesh.vertices;
         }
         if (index < cachedPositions.Length)
         {
            return cachedPositions[index];
         }
         return Vector3.zero;
      }

      Vector3[] cachedNormals;
      public Vector3 GetSafeNormal(int index)
      {
         if (_normals != null && index < _normals.Length)
         {
            return _normals[index];
         }
         if (cachedPositions == null)
         {
            MeshFilter mf = GetComponent<MeshFilter>();
            if (mf == null || mf.sharedMesh == null)
            {
               Debug.LogError("No Mesh Filter or Mesh available");
               return Vector3.zero;
            }
            cachedNormals = mf.sharedMesh.normals;
         }
         if (cachedNormals != null && index < cachedNormals.Length)
         {
            return cachedNormals[index];
         }
         return new Vector3(0, 0, 1);
      }

      Vector4[] cachedTangents;
      public Vector4 GetSafeTangent(int index)
      {
         if (_tangents != null && index < _tangents.Length)
         {
            return _tangents[index];
         }
         if (cachedTangents == null)
         {
            MeshFilter mf = GetComponent<MeshFilter>();
            if (mf == null || mf.sharedMesh == null)
            {
               Debug.LogError("No Mesh Filter or Mesh available");
               return Vector3.zero;
            }
            cachedTangents = mf.sharedMesh.tangents;
         }
         if (cachedTangents != null && index < cachedTangents.Length)
         {
            return cachedTangents[index];
         }
         return new Vector4(0, 1, 0, 1);
      }

      #endif

      #if UNITY_EDITOR
      // Stored here to make copy/save behaviour work better - basically, if you copy a mesh around, you want to also
      // clone the original material otherwise it may have the preview material stuck on it forever. 
      [HideInInspector]
      public Material[]       originalMaterial;
      public static Material  vertexShaderMat;


      void Awake()
      {
         // restore original material if we got saved with the preview material. 
         // I tried to do this in a number of ways; using the pre/post serialization callbacks seemed
         // like the best, but is actually not possible because they don't always both get called. In editor,
         // sometimes only the pre-serialization callback gets called. WTF..

         MeshRenderer mr = GetComponent<MeshRenderer>();
         if (mr != null)
         {
            if (mr.sharedMaterials != null && mr.sharedMaterial == vertexShaderMat && originalMaterial != null
               && originalMaterial.Length == mr.sharedMaterials.Length && originalMaterial.Length > 1)
            {
               Material[] mats = new Material[mr.sharedMaterials.Length];
               for (int i = 0; i < mr.sharedMaterials.Length; ++i)
               {
                  if (originalMaterial[i] != null)
                  {
                     mats[i] = originalMaterial[i];
                  }
               }
               mr.sharedMaterials = mats;
            }
            else if (originalMaterial != null && originalMaterial.Length > 0)
            {
               if (originalMaterial[0] != null)
               {
                  mr.sharedMaterial = originalMaterial[0];
               }
            }
         }
      }
      #endif

   	void Start()
      {
         Apply(!keepRuntimeData);
         if (keepRuntimeData)
         {
            var mf = GetComponent<MeshFilter>();
            _positions = mf.sharedMesh.vertices;
         }
      }

      void OnDestroy()
      {
         if (!Application.isPlaying)
         {
            MeshRenderer mr = GetComponent<MeshRenderer> ();
            if ( mr != null )
               mr.additionalVertexStreams = null;
         }
      }

      bool enforcedColorChannels = false;
      void EnforceOriginalMeshHasColors(Mesh stream)
      {
         if (enforcedColorChannels == true)
            return;
         enforcedColorChannels = true;
         MeshFilter mf = GetComponent<MeshFilter>();
         Color[] origColors = mf.sharedMesh.colors;
         if (stream != null && stream.colors.Length > 0 && (origColors == null || origColors.Length == 0))
         {
            // workaround for unity bug; dispite docs claim, color channels must exist on the original mesh
            // for the additionalVertexStream to work. Which is, sad...
            mf.sharedMesh.colors = stream.colors;
         }
      }

      #if UNITY_EDITOR
      public void SetColor(Color c, int count) { _colors = new Color[count]; for (int i = 0; i < count; ++i) { _colors[i] = c; } Apply(); }
      public void SetUV0(Vector4 uv, int count) { _uv0 = new List<Vector4>(count); for (int i = 0; i < count; ++i) { _uv0.Add(uv); } Apply(); }
      public void SetUV1(Vector4 uv, int count) { _uv1 = new List<Vector4>(count); for (int i = 0; i < count; ++i) { _uv1.Add(uv); } Apply(); }
      public void SetUV2(Vector4 uv, int count) { _uv2 = new List<Vector4>(count); for (int i = 0; i < count; ++i) { _uv2.Add(uv); } Apply(); }
      public void SetUV3(Vector4 uv, int count) { _uv3 = new List<Vector4>(count); for (int i = 0; i < count; ++i) { _uv3.Add(uv); } Apply(); }

      public void SetUV0_XY(Vector2 uv, int count)
      {
         if (_uv0 == null || _uv0.Count != count)
         {
            _uv0 = new List<Vector4>(count);
            for (int i = 0; i < count; ++i)
            {
               _uv0[i] = Vector4.zero;
            }
         }

         for (int i = 0; i < count; ++i) 
         { 
            Vector4 v = _uv0[i];
            v.x = uv.x;
            v.y = uv.y;
            _uv0[i] = v;
         }
         Apply();
      }

      public void SetUV0_ZW(Vector2 uv, int count)
      {
         if (_uv0 == null || _uv0.Count != count)
         {
            _uv0 = new List<Vector4>(count);
            for (int i = 0; i < count; ++i)
            {
               _uv0[i] = Vector4.zero;
            }
         }

         for (int i = 0; i < count; ++i) 
         { 
            Vector4 v = _uv0[i];
            v.z = uv.x;
            v.w = uv.y;
            _uv0[i] = v;
         }
         Apply();
      }

      public void SetUV1_XY(Vector2 uv, int count)
      {
         if (_uv1 == null || _uv1.Count != count)
         {
            _uv1 = new List<Vector4>(count);
            for (int i = 0; i < count; ++i)
            {
               _uv1[i] = Vector4.zero;
            }
         }

         for (int i = 0; i < count; ++i) 
         { 
            Vector4 v = _uv1[i];
            v.x = uv.x;
            v.y = uv.y;
            _uv1[i] = v;
         }
         Apply();
      }

      public void SetUV1_ZW(Vector2 uv, int count)
      {
         if (_uv1 == null || _uv1.Count != count)
         {
            _uv1 = new List<Vector4>(count);
            for (int i = 0; i < count; ++i)
            {
               _uv1[i] = Vector4.zero;
            }
         }

         for (int i = 0; i < count; ++i) 
         { 
            Vector4 v = _uv1[i];
            v.z = uv.x;
            v.w = uv.y;
            _uv1[i] = v;
         }
         Apply();
      }

      public void SetUV2_XY(Vector2 uv, int count)
      {
         if (_uv2 == null || _uv2.Count != count)
         {
            _uv2 = new List<Vector4>(count);
            for (int i = 0; i < count; ++i)
            {
               _uv2[i] = Vector4.zero;
            }
         }

         for (int i = 0; i < count; ++i) 
         { 
            Vector4 v = _uv2[i];
            v.x = uv.x;
            v.y = uv.y;
            _uv2[i] = v;
         }
         Apply();
      }

      public void SetUV2_ZW(Vector2 uv, int count)
      {
         if (_uv2 == null || _uv2.Count != count)
         {
            _uv2 = new List<Vector4>(count);
            for (int i = 0; i < count; ++i)
            {
               _uv2[i] = Vector4.zero;
            }
         }

         for (int i = 0; i < count; ++i) 
         { 
            Vector4 v = _uv2[i];
            v.z = uv.x;
            v.w = uv.y;
            _uv2[i] = v;
         }
         Apply();
      }

      public void SetUV3_XY(Vector2 uv, int count)
      {
         if (_uv3 == null || _uv3.Count != count)
         {
            _uv3 = new List<Vector4>(count);
            for (int i = 0; i < count; ++i)
            {
               _uv3[i] = Vector4.zero;
            }
         }

         for (int i = 0; i < count; ++i) 
         { 
            Vector4 v = _uv3[i];
            v.x = uv.x;
            v.y = uv.y;
            _uv3[i] = v;
         }
         Apply();
      }

      public void SetUV3_ZW(Vector2 uv, int count)
      {
         if (_uv3 == null || _uv3.Count != count)
         {
            _uv3 = new List<Vector4>(count);
            for (int i = 0; i < count; ++i)
            {
               _uv3[i] = Vector4.zero;
            }
         }

         for (int i = 0; i < count; ++i) 
         { 
            Vector4 v = _uv3[i];
            v.z = uv.x;
            v.w = uv.y;
            _uv3[i] = v;
         }
         Apply();
      }

      public void SetColorRG(Vector2 rg, int count) 
      { 
         if (_colors == null || _colors.Length != count)
         {
            _colors = new Color[count];
            enforcedColorChannels = false;
         }
         for (int i = 0; i < count; ++i)
         {
            _colors[i].r = rg.x;
            _colors[i].g = rg.y;
         }
         Apply();
      }

      public void SetColorBA(Vector2 ba, int count) 
      { 
         if (_colors == null || _colors.Length != count)
         {
            _colors = new Color[count];
            enforcedColorChannels = false;
         }
         for (int i = 0; i < count; ++i)
         {
            _colors[i].r = ba.x;
            _colors[i].g = ba.y;
         }
         Apply();
      }
      #endif

      public Mesh Apply(bool markNoLongerReadable = true)
      {
         MeshRenderer mr = GetComponent<MeshRenderer>();
         MeshFilter mf = GetComponent<MeshFilter>();

         if (mr != null && mf != null && mf.sharedMesh != null)
         {
            int vertexCount = mf.sharedMesh.vertexCount;
            Mesh stream = meshStream;
            if (stream == null || vertexCount != stream.vertexCount)
            {
               if (stream != null)
               {
                  DestroyImmediate(stream);
               }
               stream = new Mesh();

               // even though the docs say you don't need to set the positions on your avs, you do.. 
               stream.vertices = new Vector3[mf.sharedMesh.vertexCount];

               // wtf, copy won't work?
               // so, originally I did a copyTo here, but with a unity patch release the behavior changed and
               // the verticies would all become 0. This seems a funny thing to change in a patch release, but
               // since getting the data from the C++ side creates a new array anyway, we don't really need
               // to copy them anyway since they are a unique copy already.
               stream.vertices = mf.sharedMesh.vertices;
               // another Unity bug, when in editor, the paint job will just disapear sometimes. So we have to re-assign
               // it every update (even though this doesn't get called each frame, it appears to loose the data during
               // the editor update call, which only happens occationaly. 
               stream.MarkDynamic();
               stream.triangles = mf.sharedMesh.triangles;
               meshStream = stream;

               stream.hideFlags = HideFlags.HideAndDontSave;
            }
            if (_positions != null && _positions.Length == vertexCount) { stream.vertices = _positions; }
            if (_normals != null && _normals.Length == vertexCount) { stream.normals = _normals; } else { stream.normals = null; }
            if (_tangents != null && _tangents.Length == vertexCount) { stream.tangents = _tangents; } else { stream.tangents = null; }
            if (_colors != null && _colors.Length == vertexCount) { stream.colors = _colors; } else { stream.colors = null; }
            if (_uv0 != null && _uv0.Count == vertexCount) { stream.SetUVs(0, _uv0);  }  else { stream.uv = null; }
            if (_uv1 != null && _uv1.Count == vertexCount) { stream.SetUVs(1, _uv1); }  else { stream.uv2 = null; }
            if (_uv2 != null && _uv2.Count == vertexCount) { stream.SetUVs(2, _uv2); }  else { stream.uv3 = null; }
            if (_uv3 != null && _uv3.Count == vertexCount) { stream.SetUVs(3, _uv3); }  else { stream.uv4 = null; }

            EnforceOriginalMeshHasColors(stream);
 
            if (!Application.isPlaying || Application.isEditor)
            {
               // only mark no longer readable in game..
               markNoLongerReadable = false;
            }

            stream.UploadMeshData(markNoLongerReadable);
            mr.additionalVertexStreams = stream;
            return stream;
         }
         return null;
      }

      // keep this around for updates..
      private Mesh meshStream;
      // In various versions of unity, you have to set .additionalVertexStreams every frame. 
      // This was broken in 5.3, then broken again in 5.5..

   #if UNITY_EDITOR

      public Mesh GetModifierMesh() { return meshStream; }
      private MeshRenderer meshRend = null;
      void Update()
      {
         // turns out this happens in play mode as well.. grrr..
         if (meshRend == null)
         {
            meshRend = GetComponent<MeshRenderer>();
         }
         //if (!Application.isPlaying)
         {
            meshRend.additionalVertexStreams = meshStream;
         }
      }
   #endif
   }
}
