using UnityEngine;
using UnityEditor;
using System.Collections;
using System;
using System.Linq;
using System.Reflection;

// https://gist.github.com/MattRix/9205bc62d558fef98045
namespace JBooth.VertexPainterPro
{
   [InitializeOnLoad]
   public class RXLookingGlass
   {
   	public static Type type_HandleUtility;
   	protected static MethodInfo meth_IntersectRayMesh;
   	
   	static RXLookingGlass() 
   	{
   		var editorTypes = typeof(Editor).Assembly.GetTypes();
   		
   		type_HandleUtility = editorTypes.FirstOrDefault(t => t.Name == "HandleUtility");
   		meth_IntersectRayMesh = type_HandleUtility.GetMethod("IntersectRayMesh",(BindingFlags.Static | BindingFlags.NonPublic));
   	}
   	
   	public static bool IntersectRayMesh(Ray ray, MeshFilter meshFilter, out RaycastHit hit)
   	{
   		return IntersectRayMesh(ray,meshFilter.sharedMesh,meshFilter.transform.localToWorldMatrix,out hit);
   	}
      static object[] parameters = new object[4];
   	public static bool IntersectRayMesh(Ray ray, Mesh mesh, Matrix4x4 matrix, out RaycastHit hit)
   	{
         parameters[0] = ray;
         parameters[1] = mesh;
         parameters[2] = matrix;
         parameters[3] = null;
   		bool result = (bool)meth_IntersectRayMesh.Invoke(null, parameters);
   		hit = (RaycastHit)parameters[3];
         return result;
      }
   }
}