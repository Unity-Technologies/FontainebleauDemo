using System.Collections;
using UnityEditor;
using System.Collections.Generic;

/*  VertexPainterWindow
 *    - Jason Booth
 * 
 *    Uses Unity 5.0+ MeshRenderer.additionalVertexStream so that you can paint per-instance vertex colors on your meshes.
 * A component is added to your mesh to serialize this data and set it at load time. This is more effecient than making
 * duplicate meshes, and certainly less painful than saving them as separate asset files on disk. However, if you only have
 * one copy of the vertex information in your game and want to burn it into the original mesh, you can use the save feature
 * to save a new version of your mesh with the data burned into the verticies, avoiding the need for the runtime component. 
 * 
 * In other words, bake it if you need to instance the paint job - however, if you want tons of the same instances painted
 * uniquely in your scene, keep the component version and skip the baking..
 * 
 * One possible optimization is to have the component free the array after updating the mesh when in play mode..
 * 
 * Also supports burning data into the UV channels, in case you want some additional channels to work with, which also
 * happen to be full 32bit floats. You can set a viewable range; so if your floats go from 0-120, it will remap it to
 * 0-1 for display in the shader. That way you can always see your values, even when they go out of color ranges.
 * 
 * Note that as of this writing Unity has a bug in the additionalVertexStream function. The docs claim the data applied here
 * will supply or overwrite the data in the mesh, however, this is not true. Rather, it will only replace the data that's 
 * there - if your mesh has no color information, it will not upload the color data in additionalVertexStream, which is sad
 * because the original mesh doesn't need this data. As a workaround, if your mesh does not have color channels on the verts,
 * they will be created for you. 
 * 
 * There is another bug in additionalVertexStream, in that the mesh keeps disapearing in edit mode. So the component
 * which holds the data caches the mesh and keeps assigning it in the Update call, but only when running in the editor
 * and not in play mode. 
 * 
 * Really, the additionalVertexStream mesh should be owned by the MeshRenderer and saved as part of the objects instance
 * data. That's essentially what the VertexInstaceStream component does, but it's annoying and wasteful of memory to do
 * it this way since it doesn't need to be on the CPU at all. Enlighten somehow does this with the UVs it generates
 * this way, but appears to be handled specially. Oh, Unity..
*/



namespace JBooth.VertexPainterPro
{
   public partial class VertexPainterWindow : EditorWindow 
   {
      [MenuItem("Window/Vertex Painter Pro")]
      public static void ShowWindow()
      {
         var window = GetWindow<JBooth.VertexPainterPro.VertexPainterWindow>();
         window.InitMeshes();
         window.Show();
      }
   }
}
