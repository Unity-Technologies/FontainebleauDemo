using UnityEngine;
using System.Collections;
using UnityEditor;
using System.Collections.Generic;

namespace JBooth.VertexPainterPro
{
   public partial class VertexPainterWindow : EditorWindow
   {
      // for external tools
      public System.Action<PaintJob[]> OnBeginStroke;
      public System.Action<PaintJob, bool> OnStokeModified;  // bool is true when doing a fill or other non-bounded opperation
      public System.Action OnEndStroke;


      // C# doesn't have *& or **, so it's not easy to pass a reference to a value for changing.
      // instead, we wrap the setter into a templated lambda which allows us to pass a changable
      // reference around via a function which sets it. Pretty tricky sis, but I'd rather just
      // be able to pass the freaking reference already..
      // Note the ref object, which is there just to prevent boxing of Vector/Color structs. Also
      // note the complete lack of type safety, etc.. ugh..

      // whats worse- this could also be condensed down to a macro, which would actually be MORE
      // safe in terms of potential bugs than all this; and it would be like a dozen lines to boot.



      public delegate void Lerper(PaintJob j, int idx, ref object v, float strength);

      static void FlowColorRG(PaintJob j, int idx, ref object v, float r)
      {
         Vector2 vv = (Vector2)v;
         var s = j.stream;
         Color c = s.colors[idx];
         s.colors[idx].r = Mathf.Lerp(c.r, vv.x, r);
         s.colors[idx].g = Mathf.Lerp(c.g, vv.y, r); 
      }
      static void FlowColorBA(PaintJob j, int idx, ref object v, float r)
      {
         Vector2 vv = (Vector2)v;
         var s = j.stream;
         Color c = s.colors[idx];
         s.colors[idx].b = Mathf.Lerp(c.b, vv.x, r);
         s.colors[idx].a = Mathf.Lerp(c.a, vv.y, r); 
      }
      static void FlowUV0_XY(PaintJob j, int idx, ref object v, float r)
      {
         var s = j.stream;
         Vector4 o = s.uv0[idx];
         Vector2 t = (Vector2)v;
         o.x = Mathf.Lerp(o.x, t.x, r);
         o.y = Mathf.Lerp(o.y, t.y, r);
         s.uv0[idx] = o;
      }
      static void FlowUV0_ZW(PaintJob j, int idx, ref object v, float r)
      {
         var s = j.stream;
         Vector4 o = s.uv0[idx];
         Vector2 t = (Vector2)v;
         o.z = Mathf.Lerp(o.z, t.x, r);
         o.w = Mathf.Lerp(o.w, t.y, r);
         s.uv0[idx] = o;
      }
      static void FlowUV1_XY(PaintJob j, int idx, ref object v, float r)
      {
         var s = j.stream;
         Vector4 o = s.uv1[idx];
         Vector2 t = (Vector2)v;
         o.x = Mathf.Lerp(o.x, t.x, r);
         o.y = Mathf.Lerp(o.y, t.y, r);
         s.uv1[idx] = o;
      }
      static void FlowUV1_ZW(PaintJob j, int idx, ref object v, float r)
      {
         var s = j.stream;
         Vector4 o = s.uv1[idx];
         Vector2 t = (Vector2)v;
         o.z = Mathf.Lerp(o.z, t.x, r);
         o.w = Mathf.Lerp(o.w, t.y, r);
         s.uv1[idx] = o;
      }
      static void FlowUV2_XY(PaintJob j, int idx, ref object v, float r)
      {
         var s = j.stream;
         Vector4 o = s.uv2[idx];
         Vector2 t = (Vector2)v;
         o.x = Mathf.Lerp(o.x, t.x, r);
         o.y = Mathf.Lerp(o.y, t.y, r);
         s.uv2[idx] = o;
      }
      static void FlowUV2_ZW(PaintJob j, int idx, ref object v, float r)
      {
         var s = j.stream;
         Vector4 o = s.uv2[idx];
         Vector2 t = (Vector2)v;
         o.z = Mathf.Lerp(o.z, t.x, r);
         o.w = Mathf.Lerp(o.w, t.y, r);
         s.uv2[idx] = o;
      }
      static void FlowUV3_XY(PaintJob j, int idx, ref object v, float r)
      {
         var s = j.stream;
         Vector4 o = s.uv3[idx];
         Vector2 t = (Vector2)v;
         o.x = Mathf.Lerp(o.x, t.x, r);
         o.y = Mathf.Lerp(o.y, t.y, r);
         s.uv3[idx] = o;
      }
      static void FlowUV3_ZW(PaintJob j, int idx, ref object v, float r)
      {
         var s = j.stream;
         Vector4 o = s.uv3[idx];
         Vector2 t = (Vector2)v;
         o.z = Mathf.Lerp(o.z, t.x, r);
         o.w = Mathf.Lerp(o.w, t.y, r);
         s.uv3[idx] = o;
      }
      static void ColorRGBA(PaintJob j, int idx, ref object v, float r)
      {
         var s = j.stream;
         s.colors[idx] = Color.Lerp(s.colors[idx], (Color)v, r);
      }
      static void ColorRGBASaturate(PaintJob j, int idx, ref object v, float r)
      {
         var st = j.stream;
         float h, s, b;
         Color.RGBToHSV(st.colors[idx], out h, out s, out b);
         s = Mathf.Lerp(s, 1.0f, r);
         Color res = Color.HSVToRGB(h, s, b);
         st.colors[idx] = new Color(res.r, res.g, res.b, st.colors[idx].a);
      }
      static void ColorRGBADesaturate(PaintJob j, int idx, ref object v, float r)
      {
         var st = j.stream;
         float h, s, b;
         Color.RGBToHSV(st.colors[idx], out h, out s, out b);
         s = Mathf.Lerp(s, 0.0f, r);
         Color res = Color.HSVToRGB(h, s, b);
         st.colors[idx] = new Color(res.r, res.g, res.b, st.colors[idx].a);
      }
      static void ColorRGBALighten(PaintJob j, int idx, ref object v, float r)
      {
         var st = j.stream;
         float h, s, b;
         Color.RGBToHSV(st.colors[idx], out h, out s, out b);
         b = Mathf.Lerp(b, 1.0f, r);
         Color res = Color.HSVToRGB(h, s, b);
         st.colors[idx] = new Color(res.r, res.g, res.b, st.colors[idx].a);
      }
      static void ColorRGBADarken(PaintJob j, int idx, ref object v, float r)
      {
         var st = j.stream;
         float h, s, b;
         Color.RGBToHSV(st.colors[idx], out h, out s, out b);
         b = Mathf.Lerp(b, 0.0f, r);
         Color res = Color.HSVToRGB(h, s, b);
         st.colors[idx] = new Color(res.r, res.g, res.b, st.colors[idx].a);
      }
      static void ColorRGBAOverlay(PaintJob j, int idx, ref object v, float r)
      {
         var st = j.stream;
         Color c0 = st.colors[idx];
         Color t = (Color)v;
         c0.r = Mathf.Lerp(c0.r, c0.r < 0.5f ? (2.0f * c0.r * t.r) : (1.0f - 2.0f * (1.0f - c0.r) * (1.0f - t.r)), r);
         c0.g = Mathf.Lerp(c0.g, c0.g < 0.5f ? (2.0f * c0.g * t.g) : (1.0f - 2.0f * (1.0f - c0.g) * (1.0f - t.g)), r);
         c0.b = Mathf.Lerp(c0.b, c0.b < 0.5f ? (2.0f * c0.b * t.b) : (1.0f - 2.0f * (1.0f - c0.b) * (1.0f - t.b)), r);
         st.colors[idx] = c0;
      }
      static void ColorR(PaintJob j, int idx, ref object v, float r)
      {
         var s = j.stream;
         s.colors[idx].r = Mathf.Lerp(s.colors[idx].r, (float)v, r);
      }
      static void ColorG(PaintJob j, int idx, ref object v, float r)
      {
         var s = j.stream;
         s.colors[idx].g = Mathf.Lerp(s.colors[idx].g, (float)v, r);
      }
      static void ColorB(PaintJob j, int idx, ref object v, float r)
      {
         var s = j.stream;
         s.colors[idx].b = Mathf.Lerp(s.colors[idx].b, (float)v, r);
      }
      static void ColorA(PaintJob j, int idx, ref object v, float r)
      {
         var s = j.stream;
         s.colors[idx].a = Mathf.Lerp(s.colors[idx].a, (float)v, r);
      }
      static void UV0_X(PaintJob j, int idx, ref object v, float r)
      {
         var s = j.stream;
         Vector4 vec = s.uv0[idx];
         vec.x = Mathf.Lerp(vec.x, (float)v, r);
         s.uv0[idx] = vec;
      }
      static void UV0_Y(PaintJob j, int idx, ref object v, float r)
      {
         var s = j.stream;
         Vector4 vec = s.uv0[idx];
         vec.y = Mathf.Lerp(vec.y, (float)v, r);
         s.uv0[idx] = vec;
      }
      static void UV0_Z(PaintJob j, int idx, ref object v, float r)
      {
         var s = j.stream;
         Vector4 vec = s.uv0[idx];
         vec.z = Mathf.Lerp(vec.z, (float)v, r);
         s.uv0[idx] = vec;
      }
      static void UV0_W(PaintJob j, int idx, ref object v, float r)
      {
         var s = j.stream;
         Vector4 vec = s.uv0[idx];
         vec.w = Mathf.Lerp(vec.w, (float)v, r);
         s.uv0[idx] = vec;
      }
      static void UV1_X(PaintJob j, int idx, ref object v, float r)
      {
         var s = j.stream;
         Vector4 vec = s.uv1[idx];
         vec.x = Mathf.Lerp(vec.x, (float)v, r);
         s.uv1[idx] = vec;
      }
      static void UV1_Y(PaintJob j, int idx, ref object v, float r)
      {
         var s = j.stream;
         Vector4 vec = s.uv1[idx];
         vec.y = Mathf.Lerp(vec.y, (float)v, r);
         s.uv1[idx] = vec;
      }
      static void UV1_Z(PaintJob j, int idx, ref object v, float r)
      {
         var s = j.stream;
         Vector4 vec = s.uv1[idx];
         vec.z = Mathf.Lerp(vec.z, (float)v, r);
         s.uv1[idx] = vec;
      }
      static void UV1_W(PaintJob j, int idx, ref object v, float r)
      {
         var s = j.stream;
         Vector4 vec = s.uv1[idx];
         vec.w = Mathf.Lerp(vec.w, (float)v, r);
         s.uv1[idx] = vec;
      }
      static void UV2_X(PaintJob j, int idx, ref object v, float r)
      {
         var s = j.stream;
         Vector4 vec = s.uv2[idx];
         vec.x = Mathf.Lerp(vec.x, (float)v, r);
         s.uv2[idx] = vec;
      }
      static void UV2_Y(PaintJob j, int idx, ref object v, float r)
      {
         var s = j.stream;
         Vector4 vec = s.uv2[idx];
         vec.y = Mathf.Lerp(vec.y, (float)v, r);
         s.uv2[idx] = vec;
      }
      static void UV2_Z(PaintJob j, int idx, ref object v, float r)
      {
         var s = j.stream;
         Vector4 vec = s.uv2[idx];
         vec.z = Mathf.Lerp(vec.z, (float)v, r);
         s.uv2[idx] = vec;
      }
      static void UV2_W(PaintJob j, int idx, ref object v, float r)
      {
         var s = j.stream;
         Vector4 vec = s.uv2[idx];
         vec.w = Mathf.Lerp(vec.w, (float)v, r);
         s.uv2[idx] = vec;
      }
      static void UV3_X(PaintJob j, int idx, ref object v, float r)
      {
         var s = j.stream;
         Vector4 vec = s.uv3[idx];
         vec.x = Mathf.Lerp(vec.x, (float)v, r);
         s.uv3[idx] = vec;
      }
      static void UV3_Y(PaintJob j, int idx, ref object v, float r)
      {
         var s = j.stream;
         Vector4 vec = s.uv3[idx];
         vec.y = Mathf.Lerp(vec.y, (float)v, r);
         s.uv3[idx] = vec;
      }
      static void UV3_Z(PaintJob j, int idx, ref object v, float r)
      {
         var s = j.stream;
         Vector4 vec = s.uv3[idx];
         vec.z = Mathf.Lerp(vec.z, (float)v, r);
         s.uv3[idx] = vec;
      }
      static void UV3_W(PaintJob j, int idx, ref object v, float r)
      {
         var s = j.stream;
         Vector4 vec = s.uv3[idx];
         vec.w = Mathf.Lerp(vec.w, (float)v, r);
         s.uv3[idx] = vec;
      }

      static void UV0_AsColor(PaintJob j, int idx, ref object v, float r)
      {
         var s = j.stream;
         Color c = (Color)v;
         Vector4 asVector = new Vector4(c.r, c.g, c.b, c.a);
         s.uv0[idx] = Vector4.Lerp(s.uv0[idx], asVector, r);
      }

      static void UV1_AsColor(PaintJob j, int idx, ref object v, float r)
      {
         var s = j.stream;
         Color c = (Color)v;
         Vector4 asVector = new Vector4(c.r, c.g, c.b, c.a);
         s.uv1[idx] = Vector4.Lerp(s.uv1[idx], asVector, r);
      }

      static void UV2_AsColor(PaintJob j, int idx, ref object v, float r)
      {
         var s = j.stream;
         Color c = (Color)v;
         Vector4 asVector = new Vector4(c.r, c.g, c.b, c.a);
         s.uv2[idx] = Vector4.Lerp(s.uv2[idx], asVector, r);
      }

      static void UV3_AsColor(PaintJob j, int idx, ref object v, float r)
      {
         var s = j.stream;
         Color c = (Color)v;
         Vector4 asVector = new Vector4(c.r, c.g, c.b, c.a);
         s.uv3[idx] = Vector4.Lerp(s.uv3[idx], asVector, r);
      }
      static void UV0_AsColorRGBASaturate(PaintJob j, int idx, ref object v, float r)
      {
         var st = j.stream;
         float h, s, b;
         Vector4 vec = st.uv0[idx];
         Color c = new Color(vec.x, vec.y, vec.z);
         Color.RGBToHSV(c, out h, out s, out b);
         s = Mathf.Lerp(s, 1.0f, r);
         c = Color.HSVToRGB(h, s, b);
         st.uv0[idx] = new Vector4(c.r, c.g, c.b, vec.w);
      }
      static void UV0_AsColorRGBADesaturate(PaintJob j, int idx, ref object v, float r)
      {
         var st = j.stream;
         float h, s, b;
         Vector4 vec = st.uv0[idx];
         Color c = new Color(vec.x, vec.y, vec.z);
         Color.RGBToHSV(c, out h, out s, out b);
         s = Mathf.Lerp(s, 0.0f, r);
         c = Color.HSVToRGB(h, s, b);
         st.uv0[idx] = new Vector4(c.r, c.g, c.b, vec.w);
      }
      static void UV0_AsColorRGBALighten(PaintJob j, int idx, ref object v, float r)
      {
         var st = j.stream;
         float h, s, b;
         Vector4 vec = st.uv0[idx];
         Color c = new Color(vec.x, vec.y, vec.z);
         Color.RGBToHSV(c, out h, out s, out b);
         b = Mathf.Lerp(b, 1.0f, r);
         c = Color.HSVToRGB(h, s, b);
         st.uv0[idx] = new Vector4(c.r, c.g, c.b, vec.w);
      }
      static void UV0_AsColorRGBADarken(PaintJob j, int idx, ref object v, float r)
      {
         var st = j.stream;
         float h, s, b;
         Vector4 vec = st.uv0[idx];
         Color c = new Color(vec.x, vec.y, vec.z);
         Color.RGBToHSV(c, out h, out s, out b);
         b = Mathf.Lerp(b, 0.0f, r);
         c = Color.HSVToRGB(h, s, b);
         st.uv0[idx] = new Vector4(c.r, c.g, c.b, vec.w);
      }
      static void UV0_AsColorRGBAOverlay(PaintJob j, int idx, ref object v, float r)
      {
         var st = j.stream;
         Vector4 vec = st.uv0[idx];
         Color c0 = new Color(vec.x, vec.y, vec.z);
         Color t = (Color)v;
         c0.r = Mathf.Lerp(c0.r, c0.r < 0.5f ? (2.0f * c0.r * t.r) : (1.0f - 2.0f * (1.0f - c0.r) * (1.0f - t.r)), r);
         c0.g = Mathf.Lerp(c0.g, c0.g < 0.5f ? (2.0f * c0.g * t.g) : (1.0f - 2.0f * (1.0f - c0.g) * (1.0f - t.g)), r);
         c0.b = Mathf.Lerp(c0.b, c0.b < 0.5f ? (2.0f * c0.b * t.b) : (1.0f - 2.0f * (1.0f - c0.b) * (1.0f - t.b)), r);
         st.uv0[idx] = new Vector4(c0.r, c0.g, c0.b, vec.w);
      }

      static void UV1_AsColorRGBASaturate(PaintJob j, int idx, ref object v, float r)
      {
         var st = j.stream;
         float h, s, b;
         Vector4 vec = st.uv1[idx];
         Color c = new Color(vec.x, vec.y, vec.z);
         Color.RGBToHSV(c, out h, out s, out b);
         s = Mathf.Lerp(s, 1.0f, r);
         c = Color.HSVToRGB(h, s, b);
         st.uv1[idx] = new Vector4(c.r, c.g, c.b, vec.w);
      }
      static void UV1_AsColorRGBADesaturate(PaintJob j, int idx, ref object v, float r)
      {
         var st = j.stream;
         float h, s, b;
         Vector4 vec = st.uv1[idx];
         Color c = new Color(vec.x, vec.y, vec.z);
         Color.RGBToHSV(c, out h, out s, out b);
         s = Mathf.Lerp(s, 0.0f, r);
         c = Color.HSVToRGB(h, s, b);
         st.uv1[idx] = new Vector4(c.r, c.g, c.b, vec.w);
      }
      static void UV1_AsColorRGBALighten(PaintJob j, int idx, ref object v, float r)
      {
         var st = j.stream;
         float h, s, b;
         Vector4 vec = st.uv1[idx];
         Color c = new Color(vec.x, vec.y, vec.z);
         Color.RGBToHSV(c, out h, out s, out b);
         b = Mathf.Lerp(b, 1.0f, r);
         c = Color.HSVToRGB(h, s, b);
         st.uv1[idx] = new Vector4(c.r, c.g, c.b, vec.w);
      }
      static void UV1_AsColorRGBADarken(PaintJob j, int idx, ref object v, float r)
      {
         var st = j.stream;
         float h, s, b;
         Vector4 vec = st.uv1[idx];
         Color c = new Color(vec.x, vec.y, vec.z);
         Color.RGBToHSV(c, out h, out s, out b);
         b = Mathf.Lerp(b, 0.0f, r);
         c = Color.HSVToRGB(h, s, b);
         st.uv1[idx] = new Vector4(c.r, c.g, c.b, vec.w);
      }
      static void UV1_AsColorRGBAOverlay(PaintJob j, int idx, ref object v, float r)
      {
         var st = j.stream;
         Vector4 vec = st.uv1[idx];
         Color c0 = new Color(vec.x, vec.y, vec.z);
         Color t = (Color)v;
         c0.r = Mathf.Lerp(c0.r, c0.r < 0.5f ? (2.0f * c0.r * t.r) : (1.0f - 2.0f * (1.0f - c0.r) * (1.0f - t.r)), r);
         c0.g = Mathf.Lerp(c0.g, c0.g < 0.5f ? (2.0f * c0.g * t.g) : (1.0f - 2.0f * (1.0f - c0.g) * (1.0f - t.g)), r);
         c0.b = Mathf.Lerp(c0.b, c0.b < 0.5f ? (2.0f * c0.b * t.b) : (1.0f - 2.0f * (1.0f - c0.b) * (1.0f - t.b)), r);
         st.uv1[idx] = new Vector4(c0.r, c0.g, c0.b, vec.w);
      }

      static void UV2_AsColorRGBASaturate(PaintJob j, int idx, ref object v, float r)
      {
         var st = j.stream;
         float h, s, b;
         Vector4 vec = st.uv2[idx];
         Color c = new Color(vec.x, vec.y, vec.z);
         Color.RGBToHSV(c, out h, out s, out b);
         s = Mathf.Lerp(s, 1.0f, r);
         c = Color.HSVToRGB(h, s, b);
         st.uv2[idx] = new Vector4(c.r, c.g, c.b, vec.w);
      }
      static void UV2_AsColorRGBADesaturate(PaintJob j, int idx, ref object v, float r)
      {
         var st = j.stream;
         float h, s, b;
         Vector4 vec = st.uv2[idx];
         Color c = new Color(vec.x, vec.y, vec.z);
         Color.RGBToHSV(c, out h, out s, out b);
         s = Mathf.Lerp(s, 0.0f, r);
         c = Color.HSVToRGB(h, s, b);
         st.uv2[idx] = new Vector4(c.r, c.g, c.b, vec.w);
      }
      static void UV2_AsColorRGBALighten(PaintJob j, int idx, ref object v, float r)
      {
         var st = j.stream;
         float h, s, b;
         Vector4 vec = st.uv2[idx];
         Color c = new Color(vec.x, vec.y, vec.z);
         Color.RGBToHSV(c, out h, out s, out b);
         b = Mathf.Lerp(b, 1.0f, r);
         c = Color.HSVToRGB(h, s, b);
         st.uv2[idx] = new Vector4(c.r, c.g, c.b, vec.w);
      }
      static void UV2_AsColorRGBADarken(PaintJob j, int idx, ref object v, float r)
      {
         var st = j.stream;
         float h, s, b;
         Vector4 vec = st.uv2[idx];
         Color c = new Color(vec.x, vec.y, vec.z);
         Color.RGBToHSV(c, out h, out s, out b);
         b = Mathf.Lerp(b, 0.0f, r);
         c = Color.HSVToRGB(h, s, b);
         st.uv2[idx] = new Vector4(c.r, c.g, c.b, vec.w);
      }
      static void UV2_AsColorRGBAOverlay(PaintJob j, int idx, ref object v, float r)
      {
         var st = j.stream;
         Vector4 vec = st.uv2[idx];
         Color c0 = new Color(vec.x, vec.y, vec.z);
         Color t = (Color)v;
         c0.r = Mathf.Lerp(c0.r, c0.r < 0.5f ? (2.0f * c0.r * t.r) : (1.0f - 2.0f * (1.0f - c0.r) * (1.0f - t.r)), r);
         c0.g = Mathf.Lerp(c0.g, c0.g < 0.5f ? (2.0f * c0.g * t.g) : (1.0f - 2.0f * (1.0f - c0.g) * (1.0f - t.g)), r);
         c0.b = Mathf.Lerp(c0.b, c0.b < 0.5f ? (2.0f * c0.b * t.b) : (1.0f - 2.0f * (1.0f - c0.b) * (1.0f - t.b)), r);
         st.uv2[idx] = new Vector4(c0.r, c0.g, c0.b, vec.w);
      }

      static void UV3_AsColorRGBASaturate(PaintJob j, int idx, ref object v, float r)
      {
         var st = j.stream;
         float h, s, b;
         Vector4 vec = st.uv3[idx];
         Color c = new Color(vec.x, vec.y, vec.z);
         Color.RGBToHSV(c, out h, out s, out b);
         s = Mathf.Lerp(s, 1.0f, r);
         c = Color.HSVToRGB(h, s, b);
         st.uv3[idx] = new Vector4(c.r, c.g, c.b, vec.w);
      }
      static void UV3_AsColorRGBADesaturate(PaintJob j, int idx, ref object v, float r)
      {
         var st = j.stream;
         float h, s, b;
         Vector4 vec = st.uv3[idx];
         Color c = new Color(vec.x, vec.y, vec.z);
         Color.RGBToHSV(c, out h, out s, out b);
         s = Mathf.Lerp(s, 0.0f, r);
         c = Color.HSVToRGB(h, s, b);
         st.uv3[idx] = new Vector4(c.r, c.g, c.b, vec.w);
      }
      static void UV3_AsColorRGBALighten(PaintJob j, int idx, ref object v, float r)
      {
         var st = j.stream;
         float h, s, b;
         Vector4 vec = st.uv3[idx];
         Color c = new Color(vec.x, vec.y, vec.z);
         Color.RGBToHSV(c, out h, out s, out b);
         b = Mathf.Lerp(b, 1.0f, r);
         c = Color.HSVToRGB(h, s, b);
         st.uv3[idx] = new Vector4(c.r, c.g, c.b, vec.w);
      }
      static void UV3_AsColorRGBADarken(PaintJob j, int idx, ref object v, float r)
      {
         var st = j.stream;
         float h, s, b;
         Vector4 vec = st.uv3[idx];
         Color c = new Color(vec.x, vec.y, vec.z);
         Color.RGBToHSV(c, out h, out s, out b);
         b = Mathf.Lerp(b, 0.0f, r);
         c = Color.HSVToRGB(h, s, b);
         st.uv3[idx] = new Vector4(c.r, c.g, c.b, vec.w);
      }
      static void UV3_AsColorRGBAOverlay(PaintJob j, int idx, ref object v, float r)
      {
         var st = j.stream;
         Vector4 vec = st.uv3[idx];
         Color c0 = new Color(vec.x, vec.y, vec.z);
         Color t = (Color)v;
         c0.r = Mathf.Lerp(c0.r, c0.r < 0.5f ? (2.0f * c0.r * t.r) : (1.0f - 2.0f * (1.0f - c0.r) * (1.0f - t.r)), r);
         c0.g = Mathf.Lerp(c0.g, c0.g < 0.5f ? (2.0f * c0.g * t.g) : (1.0f - 2.0f * (1.0f - c0.g) * (1.0f - t.g)), r);
         c0.b = Mathf.Lerp(c0.b, c0.b < 0.5f ? (2.0f * c0.b * t.b) : (1.0f - 2.0f * (1.0f - c0.b) * (1.0f - t.b)), r);
         st.uv3[idx] = new Vector4(c0.r, c0.g, c0.b, vec.w);
      }

      // I really wish I could Lerper[] and just return FlowLerpers[(int)flowTarget]..
      public Lerper GetLerper()
      {
         if (tab == Tab.Custom)
         {
            if (customBrush != null)
            {
               return customBrush.GetLerper();
            }
            else
            {
               Debug.LogError("No Custom Brush selected");
               return null;
            }
         }
         if (tab == Tab.Flow)
         {
            switch (flowTarget)
            {
               case FlowTarget.ColorRG:
                  return FlowColorRG;
               case FlowTarget.ColorBA:
                  return FlowColorBA;
               case FlowTarget.UV0_XY:
                  return FlowUV0_XY;
               case FlowTarget.UV0_ZW:
                  return FlowUV0_ZW;
               case FlowTarget.UV1_XY:
                  return FlowUV1_XY;
               case FlowTarget.UV1_ZW:
                  return FlowUV1_ZW;
               case FlowTarget.UV2_XY:
                  return FlowUV2_XY;
               case FlowTarget.UV2_ZW:
                  return FlowUV2_ZW;
               case FlowTarget.UV3_XY:
                  return FlowUV3_XY;
               case FlowTarget.UV3_ZW:
                  return FlowUV3_ZW;
            }
            return null;
         }
         switch (brushMode)
         {
            case BrushTarget.Color:
               {
                  switch (brushColorMode)
                  {
                     case BrushColorMode.Normal:
                        return ColorRGBA;   
                     case BrushColorMode.Overlay:
                        return ColorRGBAOverlay;
                     case BrushColorMode.Lighten:
                        return ColorRGBALighten;
                     case BrushColorMode.Darken:
                        return ColorRGBADarken;
                     case BrushColorMode.Saturate:
                        return ColorRGBASaturate;
                     case BrushColorMode.Desaturate:
                        return ColorRGBADesaturate;
                  }
               }
               return ColorRGBA;  
            case BrushTarget.ValueR:
               return ColorR;
            case BrushTarget.ValueG:
               return ColorG;
            case BrushTarget.ValueB:
               return ColorB;
            case BrushTarget.ValueA:
               return ColorA;
            case BrushTarget.UV0_X:
               return UV0_X;
            case BrushTarget.UV0_Y:
               return UV0_Y;
            case BrushTarget.UV0_Z:
               return UV0_Z;
            case BrushTarget.UV0_W:
               return UV0_W;
            case BrushTarget.UV1_X:
               return UV1_X;
            case BrushTarget.UV1_Y:
               return UV1_Y;
            case BrushTarget.UV1_Z:
               return UV1_Z;
            case BrushTarget.UV1_W:
               return UV1_W;
            case BrushTarget.UV2_X:
               return UV2_X;
            case BrushTarget.UV2_Y:
               return UV2_Y;
            case BrushTarget.UV2_Z:
               return UV2_Z;
            case BrushTarget.UV2_W:
               return UV2_W;
            case BrushTarget.UV3_X:
               return UV3_X;
            case BrushTarget.UV3_Y:
               return UV3_Y;
            case BrushTarget.UV3_Z:
               return UV3_Z;
            case BrushTarget.UV3_W:
               return UV3_W;
            case BrushTarget.UV0_AsColor:
            {
               switch (brushColorMode)
               {
                  case BrushColorMode.Normal:
                     return UV0_AsColor;   
                  case BrushColorMode.Overlay:
                     return UV0_AsColorRGBAOverlay;
                  case BrushColorMode.Lighten:
                     return UV0_AsColorRGBALighten;
                  case BrushColorMode.Darken:
                     return UV0_AsColorRGBADarken;
                  case BrushColorMode.Saturate:
                     return UV0_AsColorRGBASaturate;
                  case BrushColorMode.Desaturate:
                     return UV0_AsColorRGBADesaturate;
               }
               return UV0_AsColor; 
            }
            case BrushTarget.UV1_AsColor:
               {
                  switch (brushColorMode)
                  {
                     case BrushColorMode.Normal:
                        return UV1_AsColor;   
                     case BrushColorMode.Overlay:
                        return UV1_AsColorRGBAOverlay;
                     case BrushColorMode.Lighten:
                        return UV1_AsColorRGBALighten;
                     case BrushColorMode.Darken:
                        return UV1_AsColorRGBADarken;
                     case BrushColorMode.Saturate:
                        return UV1_AsColorRGBASaturate;
                     case BrushColorMode.Desaturate:
                        return UV1_AsColorRGBADesaturate;
                  }
               }
               return UV1_AsColor; 
            case BrushTarget.UV2_AsColor:
               {
                  switch (brushColorMode)
                  {
                     case BrushColorMode.Normal:
                        return UV2_AsColor;   
                     case BrushColorMode.Overlay:
                        return UV2_AsColorRGBAOverlay;
                     case BrushColorMode.Lighten:
                        return UV2_AsColorRGBALighten;
                     case BrushColorMode.Darken:
                        return UV2_AsColorRGBADarken;
                     case BrushColorMode.Saturate:
                        return UV2_AsColorRGBASaturate;
                     case BrushColorMode.Desaturate:
                        return UV2_AsColorRGBADesaturate;
                  }
               }
               return UV2_AsColor; 
            case BrushTarget.UV3_AsColor:
               {
                  switch (brushColorMode)
                  {
                     case BrushColorMode.Normal:
                        return UV3_AsColor;   
                     case BrushColorMode.Overlay:
                        return UV3_AsColorRGBAOverlay;
                     case BrushColorMode.Lighten:
                        return UV3_AsColorRGBALighten;
                     case BrushColorMode.Darken:
                        return UV3_AsColorRGBADarken;
                     case BrushColorMode.Saturate:
                        return UV3_AsColorRGBASaturate;
                     case BrushColorMode.Desaturate:
                        return UV3_AsColorRGBADesaturate;
                  }
               }
               return UV3_AsColor; 
               
         }
         return null;
      }

      // only really used for AO, seems like a bit overkill, but it makes things easier..
      public delegate void Multiplier(VertexInstanceStream s, int idx, ref object x);
      public Multiplier GetMultiplier()
      {
         if (tab == Tab.Flow)
         {
            switch (flowTarget)
            {
               case FlowTarget.ColorRG:
                  return delegate(VertexInstanceStream s, int idx, ref object v)
                  {
                     Vector2 vv = (Vector2)v;
                     s.colors[idx].r *= vv.x;
                     s.colors[idx].g *= vv.y;
                  }; 
               case FlowTarget.ColorBA:
                  return delegate(VertexInstanceStream s, int idx, ref object v)
                  {
                     Vector2 vv = (Vector2)v;
                     s.colors[idx].b *= vv.x;
                     s.colors[idx].a *= vv.y;
                  }; 
               case FlowTarget.UV0_XY:
                  return delegate(VertexInstanceStream s, int idx, ref object v)
                  {
                     Vector4 vec = s.uv0[idx];
                     Vector2 iv = (Vector2)v;
                     vec.x *= iv.x;
                     vec.y *= iv.y;
                     s.uv0[idx] = vec;
                  }; 
               case FlowTarget.UV0_ZW:
                  return delegate(VertexInstanceStream s, int idx, ref object v)
                  {
                     Vector4 vec = s.uv0[idx];
                     Vector2 iv = (Vector2)v;
                     vec.z *= iv.x;
                     vec.w *= iv.y;
                     s.uv0[idx] = vec;
                  }; 
               case FlowTarget.UV1_XY:
                  return delegate(VertexInstanceStream s, int idx, ref object v)
                  {
                     Vector4 vec = s.uv1[idx];
                     Vector2 iv = (Vector2)v;
                     vec.x *= iv.x;
                     vec.y *= iv.y;
                     s.uv1[idx] = vec;
                  }; 
               case FlowTarget.UV1_ZW:
                  return delegate(VertexInstanceStream s, int idx, ref object v)
                  {
                     Vector4 vec = s.uv1[idx];
                     Vector2 iv = (Vector2)v;
                     vec.z *= iv.x;
                     vec.w *= iv.y;
                     s.uv1[idx] = vec;
                  }; 
               case FlowTarget.UV2_XY:
                  return delegate(VertexInstanceStream s, int idx, ref object v)
                  {
                     Vector4 vec = s.uv2[idx];
                     Vector2 iv = (Vector2)v;
                     vec.x *= iv.x;
                     vec.y *= iv.y;
                     s.uv2[idx] = vec;
                  }; 
               case FlowTarget.UV2_ZW:
                  return delegate(VertexInstanceStream s, int idx, ref object v)
                  {
                     Vector4 vec = s.uv2[idx];
                     Vector2 iv = (Vector2)v;
                     vec.z *= iv.x;
                     vec.w *= iv.y;
                     s.uv2[idx] = vec;
                  }; 
               case FlowTarget.UV3_XY:
                  return delegate(VertexInstanceStream s, int idx, ref object v)
                  {
                     Vector4 vec = s.uv3[idx];
                     Vector2 iv = (Vector2)v;
                     vec.x *= iv.x;
                     vec.y *= iv.y;
                     s.uv3[idx] = vec;
                  }; 
               case FlowTarget.UV3_ZW:
                  return delegate(VertexInstanceStream s, int idx, ref object v)
                  {
                     Vector4 vec = s.uv3[idx];
                     Vector2 iv = (Vector2)v;
                     vec.z *= iv.x;
                     vec.w *= iv.y;
                     s.uv3[idx] = vec;
                  }; 
            }
            return null;
         }
         switch (brushMode)
         {
            case BrushTarget.Color:
               return delegate(VertexInstanceStream s, int idx, ref object v)
               {
                  s.colors[idx] *= (Color)v;
               };     
            case BrushTarget.ValueR:
               return delegate(VertexInstanceStream s, int idx, ref object v)
               {
                  s.colors[idx].r *= (float)v;
               };
            case BrushTarget.ValueG:
               return delegate(VertexInstanceStream s, int idx, ref object v)
               {
                  s.colors[idx].g *= (float)v;
               };
            case BrushTarget.ValueB:
               return delegate(VertexInstanceStream s, int idx, ref object v)
               {
                  s.colors[idx].b *= (float)v;
               };
            case BrushTarget.ValueA:
               return delegate(VertexInstanceStream s, int idx, ref object v)
               {
                  s.colors[idx].a *= (float)v;
               }; 
            case BrushTarget.UV0_X:
               return delegate(VertexInstanceStream s, int idx, ref object v)
               {
                  Vector4 vec = s.uv0[idx];
                  vec.x *= (float)v;
                  s.uv0[idx] = vec;
               }; 
            case BrushTarget.UV0_Y:
               return delegate(VertexInstanceStream s, int idx, ref object v)
               {
                  Vector4 vec = s.uv0[idx];
                  vec.y *= (float)v;
                  s.uv0[idx] = vec;
               }; 
            case BrushTarget.UV0_Z:
               return delegate(VertexInstanceStream s, int idx, ref object v)
               {
                  Vector4 vec = s.uv0[idx];
                  vec.z *= (float)v;
                  s.uv0[idx] = vec;
               }; 
            case BrushTarget.UV0_W:
               return delegate(VertexInstanceStream s, int idx, ref object v)
               {
                  Vector4 vec = s.uv0[idx];
                  vec.w *= (float)v;
                  s.uv0[idx] = vec;
               }; 
            case BrushTarget.UV1_X:
               return delegate(VertexInstanceStream s, int idx, ref object v)
               {
                  Vector4 vec = s.uv1[idx];
                  vec.x *= (float)v;
                  s.uv1[idx] = vec;
               }; 
            case BrushTarget.UV1_Y:
               return delegate(VertexInstanceStream s, int idx, ref object v)
               {
                  Vector4 vec = s.uv1[idx];
                  vec.y *= (float)v;
                  s.uv1[idx] = vec;
               }; 
            case BrushTarget.UV1_Z:
               return delegate(VertexInstanceStream s, int idx, ref object v)
               {
                  Vector4 vec = s.uv1[idx];
                  vec.z *= (float)v;
                  s.uv1[idx] = vec;
               }; 
            case BrushTarget.UV1_W:
               return delegate(VertexInstanceStream s, int idx, ref object v)
               {
                  Vector4 vec = s.uv1[idx];
                  vec.w *= (float)v;
                  s.uv1[idx] = vec;
               }; 
            case BrushTarget.UV2_X:
               return delegate(VertexInstanceStream s, int idx, ref object v)
               {
                  Vector4 vec = s.uv2[idx];
                  vec.x *= (float)v;
                  s.uv2[idx] = vec;
               }; 
            case BrushTarget.UV2_Y:
               return delegate(VertexInstanceStream s, int idx, ref object v)
               {
                  Vector4 vec = s.uv2[idx];
                  vec.y *= (float)v;
                  s.uv2[idx] = vec;
               }; 
            case BrushTarget.UV2_Z:
               return delegate(VertexInstanceStream s, int idx, ref object v)
               {
                  Vector4 vec = s.uv2[idx];
                  vec.z *= (float)v;
                  s.uv2[idx] = vec;
               }; 
            case BrushTarget.UV2_W:
               return delegate(VertexInstanceStream s, int idx, ref object v)
               {
                  Vector4 vec = s.uv2[idx];
                  vec.w *= (float)v;
                  s.uv2[idx] = vec;
               }; 
            case BrushTarget.UV3_X:
               return delegate(VertexInstanceStream s, int idx, ref object v)
               {
                  Vector4 vec = s.uv3[idx];
                  vec.x *= (float)v;
                  s.uv3[idx] = vec;
               }; 
            case BrushTarget.UV3_Y:
               return delegate(VertexInstanceStream s, int idx, ref object v)
               {
                  Vector4 vec = s.uv3[idx];
                  vec.y *= (float)v;
                  s.uv3[idx] = vec;
               };
            case BrushTarget.UV3_Z:
               return delegate(VertexInstanceStream s, int idx, ref object v)
               {
                  Vector4 vec = s.uv3[idx];
                  vec.z *= (float)v;
                  s.uv3[idx] = vec;
               }; 
            case BrushTarget.UV3_W:
               return delegate(VertexInstanceStream s, int idx, ref object v)
               {
                  Vector4 vec = s.uv3[idx];
                  vec.w *= (float)v;
                  s.uv3[idx] = vec;
               };
            case BrushTarget.UV0_AsColor:
               return delegate(VertexInstanceStream s, int idx, ref object v)
               {
                  Color c = (Color)v;
                  Vector4 asV = new Vector4(c.r, c.g, c.b, c.a);
                  Vector4 uv = s.uv0[idx];
                  uv.x *= asV.x;
                  uv.y *= asV.y;
                  uv.z *= asV.z;
                  uv.w *= asV.w;
                  s.uv0[idx] = uv;
               }; 
            case BrushTarget.UV1_AsColor:
               return delegate(VertexInstanceStream s, int idx, ref object v)
               {
                  Color c = (Color)v;
                  Vector4 asV = new Vector4(c.r, c.g, c.b, c.a);
                  Vector4 uv = s.uv1[idx];
                  uv.x *= asV.x;
                  uv.y *= asV.y;
                  uv.z *= asV.z;
                  uv.w *= asV.w;
                  s.uv1[idx] = uv;
               };   
            case BrushTarget.UV2_AsColor:
               return delegate(VertexInstanceStream s, int idx, ref object v)
               {
                  Color c = (Color)v;
                  Vector4 asV = new Vector4(c.r, c.g, c.b, c.a);
                  Vector4 uv = s.uv2[idx];
                  uv.x *= asV.x;
                  uv.y *= asV.y;
                  uv.z *= asV.z;
                  uv.w *= asV.w;
                  s.uv2[idx] = uv;
               };   
            case BrushTarget.UV3_AsColor:
               return delegate(VertexInstanceStream s, int idx, ref object v)
               {
                  Color c = (Color)v;
                  Vector4 asV = new Vector4(c.r, c.g, c.b, c.a);
                  Vector4 uv = s.uv3[idx];
                  uv.x *= asV.x;
                  uv.y *= asV.y;
                  uv.z *= asV.z;
                  uv.w *= asV.w;
                  s.uv3[idx] = uv;
               };   
         }
         return null;
      }

      public object GetBrushValue()
      {
         if (tab == Tab.Custom)
         {
            if (customBrush != null)
               return customBrush.GetBrushObject();
            Debug.Log("Please assign a custom brush");
            return null;
         }
         if (tab == Tab.Flow)
         {
            return strokeDir;
         }
         switch (brushMode)
         {
            case BrushTarget.Color:
               return brushColor;  
            case BrushTarget.ValueR:
               return brushValue / 255.0f;
            case BrushTarget.ValueG:
               return brushValue / 255.0f;
            case BrushTarget.ValueB:
               return brushValue / 255.0f;
            case BrushTarget.ValueA:
               return brushValue / 255.0f;
            case BrushTarget.UV0_AsColor:
               return brushColor;  
            case BrushTarget.UV1_AsColor:
               return brushColor;  
            case BrushTarget.UV2_AsColor:
               return brushColor;  
            case BrushTarget.UV3_AsColor:
               return brushColor;   
            default:
               return floatBrushValue;
         }
      }


      public enum FlowTarget
      {
         ColorRG = 0,
         ColorBA,
         UV0_XY,
         UV0_ZW,
         UV1_XY,
         UV1_ZW,
         UV2_XY,
         UV2_ZW,
         UV3_XY,
         UV3_ZW
      }
      
      public enum FlowBrushType
      {
         Direction = 0,
         Soften
      }

      public enum FlowVisualization
      {
         Arrows = 0,
         Water,
      }
      
      public enum BrushTarget
      {
         Color = 0,
         ValueR,
         ValueG,
         ValueB,
         ValueA,
         UV0_X,
         UV0_Y,
         UV0_Z,
         UV0_W,
         UV1_X,
         UV1_Y,
         UV1_Z,
         UV1_W,
         UV2_X,
         UV2_Y,
         UV2_Z,
         UV2_W,
         UV3_X,
         UV3_Y,
         UV3_Z,
         UV3_W,
         UV0_AsColor,
         UV1_AsColor,
         UV2_AsColor,
         UV3_AsColor
      }

      public enum BrushColorMode
      {
         Normal,
         Overlay,
         Lighten,
         Darken,
         Saturate,
         Desaturate
      }
      
      public enum VertexMode
      {
         Adjust,
         Smear,
         Smooth, 
         HistoryEraser,
      }
      
      public enum VertexContraint
      {
         Camera,
         Normal,
         X,
         Y,
         Z,
      }

      public bool            enabled;
      public Vector3         oldpos = Vector3.zero;
      public float           brushSize = 1;
      public float           brushFlow = 8;
      public float           brushFalloff = 1; // linear
      public Color           brushColor = Color.red;
      public int             brushValue = 255;
      public float           floatBrushValue = 1.0f;
      public Vector2         uvVisualizationRange = new Vector2(0, 1);
      public BrushTarget     brushMode = BrushTarget.Color;
      public BrushColorMode  brushColorMode = BrushColorMode.Normal;
      public VertexMode      vertexMode = VertexMode.Adjust;
      public FlowTarget      flowTarget = FlowTarget.ColorRG;
      public FlowBrushType   flowBrushType = FlowBrushType.Direction;
      public FlowVisualization flowVisualization = FlowVisualization.Water;
      public bool            flowRemap01 = true;
      public bool            pull = false;
      public VertexContraint vertexContraint = VertexContraint.Normal;
      public bool            showVertexShader = false;
      public bool            showVertexPoints = false;
      public float           showVertexSize = 1;
      public Color           showVertexColor = Color.white;
      public bool            showNormals = false;
      public bool            showTangents = false;

      public VertexPainterCustomBrush customBrush;

      public enum BrushVisualization
      {
         Sphere,
         Disk
      }
      public BrushVisualization brushVisualization = BrushVisualization.Sphere;
      public PaintJob[]      jobs = new PaintJob[0];
      // bool used to know if we've registered an undo with this object or not
      public bool[] jobEdits = new bool[0];

      public void RevertMat()
      {
         // revert old materials
         for (int i = 0; i < jobs.Length; ++i)
         {
            if (jobs[i].renderer != null && jobs[i].HasStream() && jobs[i].stream.originalMaterial != null && jobs[i].stream.originalMaterial.Length > 0)
            {
               var j = jobs[i];
               if (j.renderer.sharedMaterials != null && j.stream.originalMaterial != null &&
                   j.renderer.sharedMaterials.Length == j.stream.originalMaterial.Length &&
                   j.stream.originalMaterial.Length > 1)
               {
                  Material[] mats = new Material[j.stream.originalMaterial.Length];
                  for (int x = 0; x < jobs[i].renderer.sharedMaterials.Length; ++x)
                  {
                     mats[x] = j.stream.originalMaterial[x];
                  }
                  j.renderer.sharedMaterials = mats;
               }
               else
               {
                  jobs[i].renderer.sharedMaterial = jobs[i].stream.originalMaterial[0];
               }
            }
            SetWireframeDisplay(jobs[i].renderer, true);
         }
      }

      void InitMeshes()
      {
         RevertMat();

         List<PaintJob> pjs = new List<PaintJob>();
         Object[] objs = Selection.GetFiltered(typeof(GameObject), SelectionMode.Editable | SelectionMode.OnlyUserModifiable | SelectionMode.Deep);
         for (int i = 0; i < objs.Length; ++i)
         {
            GameObject go = objs[i] as GameObject;
            if (go != null)
            {
               MeshFilter mf = go.GetComponent<MeshFilter>();
               Renderer r = go.GetComponent<Renderer>();
               if (mf != null && r != null && mf.sharedMesh != null && mf.sharedMesh.isReadable)
               {
                  pjs.Add(new PaintJob(mf, r));
               }
            }
         }

         jobs = pjs.ToArray();
         jobEdits = new bool[jobs.Length];
         UpdateDisplayMode();
      }

      void SetWireframeDisplay(Renderer r, bool hidden)
      {
         #if UNITY_5_5_OR_NEWER
         EditorUtility.SetSelectedRenderState(r, hidden ? 
         EditorSelectedRenderState.Hidden : EditorSelectedRenderState.Wireframe);
         #else
         EditorUtility.SetSelectedWireframeHidden(r, hidden);
         #endif
      }

      void UpdateDisplayMode(bool endPainting = true)
      {
         if (painting && endPainting)
         {
            EndStroke();
         }
         if (VertexInstanceStream.vertexShaderMat == null)
         {
            VertexInstanceStream.vertexShaderMat = new Material(Shader.Find("Hidden/VertexPainterPro_Preview"));
            VertexInstanceStream.vertexShaderMat.hideFlags = HideFlags.HideAndDontSave;
         }
         for (int i = 0; i < jobs.Length; ++i)
         {
            var job = jobs[i];
            SetWireframeDisplay(job.renderer, hideMeshWireframe);
            if (job.renderer != null && job.HasStream())
            {
               if (!showVertexShader || !enabled)
               {
                  // restore..
                  if (job.stream.originalMaterial != null && job.stream.originalMaterial.Length > 0 &&
                      job.renderer.sharedMaterial == VertexInstanceStream.vertexShaderMat)
                  {
                     if (job.renderer.sharedMaterials != null && job.renderer.sharedMaterials.Length > 1 &&
                         job.renderer.sharedMaterials.Length == job.stream.originalMaterial.Length)
                     {
                        Material[] mats = new Material[jobs[i].renderer.sharedMaterials.Length];

                        for (int x = 0; x < job.renderer.sharedMaterials.Length; ++x)
                        {
                           mats[x] = job.stream.originalMaterial[x];
                        }
                        job.renderer.sharedMaterials = mats;
                     }
                     else
                     {
                        job.renderer.sharedMaterial = job.stream.originalMaterial[0];
                     }
                  }
                  else if (job.renderer.sharedMaterial != VertexInstanceStream.vertexShaderMat)
                  {
                     job.CaptureMat();
                  }
               }
               else if (showVertexShader)
               {
                  if (job.renderer.sharedMaterial != VertexInstanceStream.vertexShaderMat)
                  {
                     job.CaptureMat();
                  }
                  if (job.stream.originalMaterial != null && job.stream.originalMaterial.Length > 0)
                  {
                     if (job.renderer.sharedMaterials != null && job.renderer.sharedMaterials.Length > 1)
                     {
                        Material[] mats = new Material[job.renderer.sharedMaterials.Length];
                        for (int x = 0; x < job.renderer.sharedMaterials.Length; ++x)
                        {
                           mats[x] = VertexInstanceStream.vertexShaderMat;
                        }
                        job.renderer.sharedMaterials = mats;
                     }
                     else
                     {
                        job.renderer.sharedMaterial = VertexInstanceStream.vertexShaderMat;
                     }
                     VertexInstanceStream.vertexShaderMat.SetInt("_flowVisualization", (int)flowVisualization);
                     VertexInstanceStream.vertexShaderMat.SetInt("_tab", (int)tab);
                     VertexInstanceStream.vertexShaderMat.SetInt("_flowTarget", (int)flowTarget);
                     VertexInstanceStream.vertexShaderMat.SetInt("_channel", (int)brushMode);
                     VertexInstanceStream.vertexShaderMat.SetVector("_uvRange", uvVisualizationRange);
                  }
               }
            }
         }
      }

      void OnUndo()
      {
         for (int i = 0; i < jobs.Length; ++i)
         {
            if (jobs[i].stream != null)
            {
               jobs[i].stream.Apply(false);
            }
         }
      }

      public void FillMesh(PaintJob job)
      {
         
         PrepBrushMode(job);
         var lerper = GetLerper();
         var val = GetBrushValue();
         for (int i = 0; i < job.verts.Length; ++i)
         {
            lerper.Invoke(job, i, ref val, 1);
         }
         job.stream.Apply();
         if (OnStokeModified != null)
         {
            OnStokeModified(job, true);
         }
      }

      void RandomMesh(PaintJob job)
      {
         Color oldColor = brushColor;
         int oldVal = brushValue;
         float oldFloat = floatBrushValue;
         PrepBrushMode(job);
         var lerper = GetLerper();
         for (int i = 0; i < job.verts.Length; ++i)
         {
            brushColor = new Color(UnityEngine.Random.Range(0.0f, 1.0f), 
                                   UnityEngine.Random.Range(0.0f, 1.0f), 
                                   UnityEngine.Random.Range(0.0f, 1.0f), 
                                   UnityEngine.Random.Range(0.0f, 1.0f));
            brushValue = UnityEngine.Random.Range(0, 255);
            floatBrushValue = UnityEngine.Random.Range(uvVisualizationRange.x, uvVisualizationRange.y);
            object v = GetBrushValue();
            lerper(job, i, ref v, 1);
         }
         job.stream.Apply();
         brushColor = oldColor;
         brushValue = oldVal;
         floatBrushValue = oldFloat;
      }

      public void InitColors(PaintJob j)
      {
         Color[] colors = j.stream.colors;
         if (colors == null || colors.Length != j.verts.Length)
         {
            Color[] orig = j.meshFilter.sharedMesh.colors;
            if (j.meshFilter.sharedMesh.colors != null && j.meshFilter.sharedMesh.colors.Length > 0)
            {
               j.stream.colors = orig;
            }
            else
            {
               j.stream.SetColor(Color.white, j.verts.Length);
            }
         }
      }

      public void InitUV0(PaintJob j)
      {
         List<Vector4> uvs = j.stream.uv0;
         if (uvs == null || uvs.Count != j.verts.Length)
         {
            if (j.meshFilter.sharedMesh.uv != null && j.meshFilter.sharedMesh.uv.Length == j.verts.Length)
            {
               List<Vector4> nuv = new List<Vector4>(j.meshFilter.sharedMesh.vertices.Length);
               j.meshFilter.sharedMesh.GetUVs(0, nuv);
               j.stream.uv0 = nuv;
            }
            else
            {
               j.stream.SetUV0(Vector4.zero, j.verts.Length);
            }
         }
      }

      public void InitUV1(PaintJob j)
      {
         var uvs = j.stream.uv1;
         if (uvs == null || uvs.Count != j.verts.Length)
         {
            if (j.meshFilter.sharedMesh.uv2 != null && j.meshFilter.sharedMesh.uv2.Length == j.verts.Length)
            {
               List<Vector4> nuv = new List<Vector4>(j.meshFilter.sharedMesh.vertices.Length);
               j.meshFilter.sharedMesh.GetUVs(1, nuv);
               j.stream.uv1 = nuv;
            }
            else
            {
               j.stream.SetUV1(Vector2.zero, j.verts.Length);
            }
         }
      }

      public void InitUV2(PaintJob j)
      {
         var uvs = j.stream.uv2;
         if (uvs == null || uvs.Count != j.verts.Length)
         {
            if (j.meshFilter.sharedMesh.uv3 != null && j.meshFilter.sharedMesh.uv3.Length == j.verts.Length)
            {
               List<Vector4> nuv = new List<Vector4>(j.meshFilter.sharedMesh.vertices.Length);
               j.meshFilter.sharedMesh.GetUVs(2, nuv);
               j.stream.uv2 = nuv;
            }
            else
            {
               j.stream.SetUV2(Vector2.zero, j.verts.Length);
            }
         }
      }

      public void InitUV3(PaintJob j)
      {
         var uvs = j.stream.uv3;
         if (uvs == null || uvs.Count != j.verts.Length)
         {
            if (j.meshFilter.sharedMesh.uv4 != null && j.meshFilter.sharedMesh.uv4.Length == j.verts.Length)
            {
               List<Vector4> nuv = new List<Vector4>(j.meshFilter.sharedMesh.vertices.Length);
               j.meshFilter.sharedMesh.GetUVs(3, nuv);
               j.stream.uv3 = nuv;
            }
            else
            {
               j.stream.SetUV3(Vector2.zero, j.verts.Length);
            }
         }
      }

      public void InitPositions(PaintJob j)
      {
         Vector3[] pos = j.stream.positions;
         if (pos == null || pos.Length != j.verts.Length)
         {
            int vc = j.meshFilter.sharedMesh.vertexCount;
            if (j.stream.positions == null || j.stream.positions.Length != vc)
            {
               j.stream.positions = new Vector3[j.meshFilter.sharedMesh.vertices.Length];
               j.meshFilter.sharedMesh.vertices.CopyTo(j.stream.positions, 0);
            }
         }
         return;
      }

      public void InitNormalTangent(PaintJob j)
      {
         Vector3[] norms = j.stream.normals;
         if (norms == null || norms.Length != j.verts.Length)
         {
            int vc = j.meshFilter.sharedMesh.vertexCount;
            if (j.stream.normals == null || j.stream.normals.Length != vc)
            {
               j.stream.normals = new Vector3[j.meshFilter.sharedMesh.vertices.Length];
               j.meshFilter.sharedMesh.normals.CopyTo(j.stream.normals, 0);
            }
            if (j.stream.tangents == null || j.stream.tangents.Length != vc)
            {
               j.stream.tangents = new Vector4[j.meshFilter.sharedMesh.vertices.Length];
               j.meshFilter.sharedMesh.tangents.CopyTo(j.stream.tangents, 0);
            }
         }
         return;
      }

      public void PrepBrushMode(PaintJob j)
      {
         if (tab == Tab.Custom)
         {
            if (customBrush == null)
            {
               Debug.Log("Custom Brush not set");
               return;
            }
            var channels = customBrush.GetChannels();
            if ((channels & VertexPainterCustomBrush.Channels.Colors) != 0)
            {
               InitColors(j);
            }
            if ((channels & VertexPainterCustomBrush.Channels.UV0) != 0)
            {
               InitUV0(j);
            }
            if ((channels & VertexPainterCustomBrush.Channels.UV1) != 0)
            {
               InitUV1(j);
            }
            if ((channels & VertexPainterCustomBrush.Channels.UV2) != 0)
            {
               InitUV2(j);
            }
            if ((channels & VertexPainterCustomBrush.Channels.UV3) != 0)
            {
               InitUV3(j);
            }
            if ((channels & VertexPainterCustomBrush.Channels.Positions) != 0)
            {
               InitPositions(j);
            }
            if ((channels & VertexPainterCustomBrush.Channels.Normals) != 0)
            {
               InitNormalTangent(j);
            }
         }
         else if (tab == Tab.Deform)
         {
            InitPositions(j);
            InitNormalTangent(j);
            UpdateDisplayMode(false);
            return;
         }
         if (tab == Tab.Flow)
         {
            switch (flowTarget)
            {
               case FlowTarget.ColorRG:
                  goto case FlowTarget.ColorBA;
               case FlowTarget.ColorBA:
                  {
                     InitColors(j);
                     break;
                  }
               case FlowTarget.UV0_XY:
                  {
                     InitUV0(j);
                     break;
                  }
               case FlowTarget.UV1_XY:
                  {
                     InitUV1(j);
                     break;
                  }
               case FlowTarget.UV2_XY:
                  {
                     InitUV2(j);
                     break;
                  }
               case FlowTarget.UV3_XY:
                  {
                     InitUV3(j);
                     break;
                  }
            }
            UpdateDisplayMode(false);
            return;
         }

         // make sure the instance data is initialized
         switch (brushMode)
         {
            case BrushTarget.Color:
               goto case BrushTarget.ValueA;
            case BrushTarget.ValueR:
               goto case BrushTarget.ValueA;
            case BrushTarget.ValueG:
               goto case BrushTarget.ValueA;
            case BrushTarget.ValueB:
               goto case BrushTarget.ValueA;
            case BrushTarget.ValueA:
               {
                  InitColors(j);
                  break;
               }
            case BrushTarget.UV0_X:
               goto case BrushTarget.UV0_W;
            case BrushTarget.UV0_Y:
               goto case BrushTarget.UV0_W;
            case BrushTarget.UV0_Z:
               goto case BrushTarget.UV0_W;
            case BrushTarget.UV0_W:
               {
                  InitUV0(j);
                  break;
               }
            case BrushTarget.UV1_X:
               goto case BrushTarget.UV1_W;
            case BrushTarget.UV1_Y:
               goto case BrushTarget.UV1_W;
            case BrushTarget.UV1_Z:
               goto case BrushTarget.UV1_W;
            case BrushTarget.UV1_W:
               {
                  InitUV1(j);
                  break;
               }
            case BrushTarget.UV2_X:
               goto case BrushTarget.UV2_W;
            case BrushTarget.UV2_Y:
               goto case BrushTarget.UV2_W;
            case BrushTarget.UV2_Z:
               goto case BrushTarget.UV2_W;
            case BrushTarget.UV2_W:
               {
                  InitUV2(j);
                  break;
               }
            case BrushTarget.UV3_X:
               goto case BrushTarget.UV3_W;
            case BrushTarget.UV3_Y:
               goto case BrushTarget.UV3_W;
            case BrushTarget.UV3_Z:
               goto case BrushTarget.UV3_W;
            case BrushTarget.UV3_W:
               {
                  InitUV3(j);
                  break;
               }
            case BrushTarget.UV0_AsColor:
               {
                  InitUV0(j);
                  break;
               }
            case BrushTarget.UV1_AsColor:
               {
                  InitUV1(j);
                  break;
               }
            case BrushTarget.UV2_AsColor:
               {
                  InitUV2(j);
                  break;
               }
            case BrushTarget.UV3_AsColor:
               {
                  InitUV3(j);
                  break;
               }

         }
         UpdateDisplayMode(false);
      }


      void DrawVertexPoints(PaintJob j, Vector3 point)
      {
         if (j.HasStream() && j.HasData())
         {
            PrepBrushMode(j);
         }
         if (j.renderer == null)
         {
            return;
         }
         // convert point into local space, so we don't have to convert every point
         var mtx = j.renderer.transform.localToWorldMatrix;
         point = j.renderer.transform.worldToLocalMatrix.MultiplyPoint3x4(point);
         // for some reason this doesn't handle scale, seems like it should
         // we handle it poorly until I can find a better solution
         float scale = 1.0f / Mathf.Abs(j.renderer.transform.lossyScale.x);

         float bz = scale * brushSize;
         bz *= bz;

         for (int i = 0; i < j.verts.Length; ++i)
         {
            //float d = Vector3.Distance(point, j.verts[i]);
            var p = j.verts[i];
            float x = point.x - p.x;
            float y = point.y - p.y;
            float z = point.z - p.z;
            float dist = x * x + y * y + z * z;

            if (dist < bz)
            {
               Handles.color = showVertexColor;
               Vector3 wp = mtx.MultiplyPoint(j.verts[i]);
               Handles.SphereHandleCap(0, wp, Quaternion.identity, HandleUtility.GetHandleSize(wp) * 0.02f * showVertexSize, EventType.Repaint);

               if (showNormals)
               {
                  Handles.color = Color.blue;
                  Handles.DrawLine(wp, wp + mtx.MultiplyVector(j.stream.GetSafeNormal(i)));
               }
               if (showTangents)
               {
                  Handles.color = Color.yellow;
                  var tang = j.stream.GetSafeTangent(i);
                  var t2 = new Vector3(tang.x, tang.y, tang.z);
                  t2 *= tang.w;

                  Handles.DrawLine(wp, wp + mtx.MultiplyVector(t2));
               }
            }
         }
      }


      void PaintMesh(PaintJob j, Vector3 point, Lerper lerper, object value)
      {
         bool affected = false;
         PrepBrushMode(j);
         // convert point into local space, so we don't have to convert every point
         point = j.renderer.transform.worldToLocalMatrix.MultiplyPoint3x4(point);
         // for some reason this doesn't handle scale, seems like it should
         // we handle it poorly until I can find a better solution
         float scale = 1.0f / Mathf.Abs(j.renderer.transform.lossyScale.x);

         float bz = scale * brushSize;
         bz *= bz;

         float pressure = Event.current.pressure > 0 ? Event.current.pressure : 1.0f;

         bool modPos = !(j.stream.positions == null || j.stream.positions.Length == 0);
         if (tab == Tab.Flow)
         {
            float strength = strokeDir.magnitude;
            Vector3 sd = strokeDir.normalized;
            Vector2 target = new Vector2(0.5f, 0.5f);
            for (int i = 0; i < j.verts.Length; ++i)
            {
               Vector3 p = modPos ? j.stream.positions[i] : j.verts[i];
               float x = point.x - p.x;
               float y = point.y - p.y;
               float z = point.z - p.z;
               float dist = x * x + y * y + z * z;

               //float d = Vector3.Distance(point, modPos ? j.stream.positions[i] : j.verts[i]);
               if (dist < bz)
               {
                  Vector3 n = j.normals[i];
                  Vector4 t = j.tangents[i];
                  
                  if (j.stream.normals != null && j.stream.normals.Length == j.verts.Length)
                  {
                     n = j.stream.normals[i];
                  }
                  if (j.stream.tangents != null && j.stream.tangents.Length == j.verts.Length)
                  {
                     t = j.stream.tangents[i];
                  }

                  var mtx = j.meshFilter.transform.localToWorldMatrix;
                  n = mtx.MultiplyVector(n);
                  Vector3 tg = new Vector3(t.x, t.y, t.z);
                  tg = mtx.MultiplyVector(tg);
                  t.x = tg.x;
                  t.y = tg.y;
                  t.z = tg.z;

                  target.x = 0.5f;
                  target.y = 0.5f;
                  if (flowBrushType == FlowBrushType.Direction)
                  {
                     Vector3 b = Vector3.Cross(n, new Vector3(t.x, t.y, t.z) * t.w);


                     float dx = Vector3.Dot(t, sd);
                     float dy = Vector3.Dot(b, sd);
                     
                     target = new Vector2(dx, dy);
                     target.Normalize();

                     if (flowTarget == FlowTarget.ColorBA || flowTarget == FlowTarget.ColorRG || flowRemap01)
                     {
                        target.x = target.x * 0.5f + 0.5f;
                        target.y = target.y * 0.5f + 0.5f;
                     }
                  }

                  float str = 1.0f - dist / bz;
                  str *= strength;  // take brush speed into account..
                  str = Mathf.Pow(str, brushFalloff);

                  object obj = target;
                  float finalStr = str * (float)deltaTime * brushFlow * pressure;
                  if (finalStr > 0)
                  {
                     affected = true;
                     lerper.Invoke(j, i, ref obj, finalStr);
                  }
               }
            }
         } 
         else if (tab == Tab.Deform)
         {
            for (int i = 0; i < j.verts.Length; ++i)
            {
               Vector3 p = modPos ? j.stream.positions[i] : j.verts[i];
               float x = point.x - p.x;
               float y = point.y - p.y;
               float z = point.z - p.z;
               float dist = x * x + y * y + z * z;
               //float d = Vector3.Distance(point, j.verts[i]);
               if (dist < bz)
               {
                  float str = 1.0f - dist / bz;
                  str = Mathf.Pow(str, brushFalloff);
                  affected = true;
                  PaintVertPosition(j, i,  str * (float)deltaTime * brushFlow * pressure);
               }
            }
         }
         else
         {
            for (int i = 0; i < j.verts.Length; ++i)
            {
               Vector3 p = modPos ? j.stream.positions[i] : j.verts[i];
               float x = point.x - p.x;
               float y = point.y - p.y;
               float z = point.z - p.z;
               float dist = x * x + y * y + z * z;
               //float d = Vector3.Distance(point, j.verts[i]);
               //float d = Vector3.Distance(point, j.verts[i]);
               if (dist < bz)
               {
                  float str = 1.0f - dist / bz;
                  str = Mathf.Pow(str, brushFalloff);
                  float finalStr = str * (float)deltaTime * brushFlow * pressure;
                  if (finalStr > 0)
                  {
                     affected = true;
                     lerper.Invoke(j, i, ref value, finalStr);
                  }
               }
            }
         }
         if (affected)
         {
            j.stream.Apply();
            if (OnStokeModified != null)
            {
               OnStokeModified(j, false);
            }
         }
      }

      void EndStroke()
      {
         if (OnEndStroke != null)
         {
            OnEndStroke();
         }
         painting = false;
        
         // could possibly make this faster by avoiding the double apply..
         if (tab == Tab.Deform)
         {
            // This used to recalculate the normals, but this introduced tearing, since non-shared overlapping vertices
            // would get slightly different normals with each stroke and slowly tear the mesh appart. For now, disable,
            // until I have time to re-enable with some fast-spacial hash over the mesh..
            /* 
            for (int i = 0; i < jobs.Length; ++i)
            {
               PaintJob j = jobs[i];
               if (j.stream.positions != null && j.stream.normals != null && j.stream.tangents != null)
               {
                  Mesh m = j.stream.Apply(false);
                  m.triangles = j.meshFilter.sharedMesh.triangles;
                  m.normals = j.stream.normals;
                  m.tangents = j.stream.tangents;
                  m.uv = j.meshFilter.sharedMesh.uv;
                  m.RecalculateNormals();
                  CalculateMeshTangents(m);
                  j.stream.normals = m.normals;
                  j.stream.tangents = m.tangents;
                  m.RecalculateBounds();

                  j.stream.Apply();
               }
            }
            */
         }
         for (int i = 0; i < jobs.Length; ++i)
         {
            PaintJob j = jobs[i];
            if (j.HasStream())
            {
               EditorUtility.SetDirty(j.stream);
               EditorUtility.SetDirty(j.stream.gameObject);
            }
         }
      }

      void CalculateMeshTangents(Mesh mesh)
      {
         //speed up math by copying the mesh arrays
         int[] triangles = mesh.triangles;
         Vector3[] vertices = mesh.vertices;
         Vector2[] uv = mesh.uv;
         Vector3[] normals = mesh.normals;
         
         //variable definitions
         int triangleCount = triangles.Length;
         int vertexCount = vertices.Length;
         
         Vector3[] tan1 = new Vector3[vertexCount];
         Vector3[] tan2 = new Vector3[vertexCount];
         
         Vector4[] tangents = new Vector4[vertexCount];
         
         for (long a = 0; a < triangleCount; a += 3)
         {
            long i1 = triangles[a + 0];
            long i2 = triangles[a + 1];
            long i3 = triangles[a + 2];
            
            Vector3 v1 = vertices[i1];
            Vector3 v2 = vertices[i2];
            Vector3 v3 = vertices[i3];
            
            Vector2 w1 = uv[i1];
            Vector2 w2 = uv[i2];
            Vector2 w3 = uv[i3];
            
            float x1 = v2.x - v1.x;
            float x2 = v3.x - v1.x;
            float y1 = v2.y - v1.y;
            float y2 = v3.y - v1.y;
            float z1 = v2.z - v1.z;
            float z2 = v3.z - v1.z;
            
            float s1 = w2.x - w1.x;
            float s2 = w3.x - w1.x;
            float t1 = w2.y - w1.y;
            float t2 = w3.y - w1.y;
            
            float div = s1 * t2 - s2 * t1;
            float r = div == 0.0f ? 0.0f : 1.0f / div;
            
            Vector3 sdir = new Vector3((t2 * x1 - t1 * x2) * r, (t2 * y1 - t1 * y2) * r, (t2 * z1 - t1 * z2) * r);
            Vector3 tdir = new Vector3((s1 * x2 - s2 * x1) * r, (s1 * y2 - s2 * y1) * r, (s1 * z2 - s2 * z1) * r);
            
            tan1[i1] += sdir;
            tan1[i2] += sdir;
            tan1[i3] += sdir;
            
            tan2[i1] += tdir;
            tan2[i2] += tdir;
            tan2[i3] += tdir;
         }
         
         
         for (long a = 0; a < vertexCount; ++a)
         {
            Vector3 n = normals[a];
            Vector3 t = tan1[a];
            
            //Vector3 tmp = (t - n * Vector3.Dot(n, t)).normalized;
            //tangents[a] = new Vector4(tmp.x, tmp.y, tmp.z);
            Vector3.OrthoNormalize(ref n, ref t);
            tangents[a].x = t.x;
            tangents[a].y = t.y;
            tangents[a].z = t.z;
            
            tangents[a].w = (Vector3.Dot(Vector3.Cross(n, t), tan2[a]) < 0.0f) ? -1.0f : 1.0f;
         }
         
         mesh.tangents = tangents;
      }

      void ConstrainAxis(ref Vector3 cur, Vector3 orig)
      {
         if (vertexContraint == VertexContraint.X)
         {
            cur.y = orig.y;
            cur.z = orig.z;
         }
         else if (vertexContraint == VertexContraint.Y)
         {
            cur.x = orig.x;
            cur.z = orig.z;
         }
         else if (vertexContraint == VertexContraint.Z)
         {
            cur.x = orig.x;
            cur.y = orig.y;
         }
      }

      void PaintVertPosition(PaintJob j, int i, float strength)
      {
         switch (vertexMode)
         {
            case VertexMode.Adjust:
               {
                  switch (vertexContraint)
                  {
                     case VertexContraint.Normal:
                        {
                           Vector3 cur = j.stream.positions[i];
                           Vector3 dir = j.stream.normals[i];
                           dir *= strength;
                           cur += pull ? dir : -dir;
                           j.stream.positions[i] = cur;
                           break;
                        }
                     case VertexContraint.Camera:
                        {
                           Vector3 cur = j.stream.positions[i];
                           Vector3 dir = strokeDir;
                           dir *= strength;
                           cur += pull ? dir : -dir;
                           j.stream.positions[i] = cur;
                           break;
                        }
                     case VertexContraint.X:
                        {
                           Vector3 cur = j.stream.positions[i];
                           Vector3 dir = new Vector3(1, 0, 0);
                           dir *= strength;
                           cur += pull ? dir : -dir;
                           j.stream.positions[i] = cur;
                           break;
                        }
                     case VertexContraint.Y:
                        {
                           Vector3 cur = j.stream.positions[i];
                           Vector3 dir = new Vector3(0, 1, 0);
                           dir *= strength;
                           cur += pull ? dir : -dir;
                           j.stream.positions[i] = cur;
                           break;
                        }
                     case VertexContraint.Z:
                        {
                           Vector3 cur = j.stream.positions[i];
                           Vector3 dir = new Vector3(0, 0, 1);
                           dir *= strength;
                           cur += pull ? dir : -dir;
                           j.stream.positions[i] = cur;
                           break;
                        }
                  }
                  break;
               }
            case VertexMode.Smooth:
               {
                  Vector3 cur = j.stream.positions[i];
                  var con = j.vertexConnections[i];
                  for (int x = 0; x < con.Count; ++x)
                  {
                     cur += j.stream.positions[con[x]];
                  }
                  cur /= (con.Count + 1);
                  ConstrainAxis(ref cur, j.stream.positions[i]);

                  j.stream.positions[i] = Vector3.Lerp(j.stream.positions[i], cur, Mathf.Clamp01(strength));
                  break;
               }
            case VertexMode.Smear:
               {
                  Vector3 cur = j.stream.positions[i];
                  Vector3 dir = strokeDir;
                  dir *= strength;
                  cur += pull ? dir : -dir;
                  j.stream.positions[i] = cur;
                  break;
               }
            case VertexMode.HistoryEraser:
               {
                  Vector3 cur = j.stream.positions[i];
                  Vector3 orig = j.verts[i];
                  ConstrainAxis(ref orig, cur);
                  j.stream.positions[i] = Vector3.Lerp(cur, orig, Mathf.Clamp01(strength));
                  break;
               }
         }
      }
      
      double deltaTime = 0;
      double lastTime = 0;
      bool painting = false;
      Vector3 oldMousePosition;
      Vector3 strokeDir = Vector3.zero;

      void DoShortcuts()
      {
         // wish I could make this global! but can't find a way...
         if (Event.current.type == EventType.KeyDown && Event.current.keyCode == KeyCode.Escape)
         {
            enabled = !enabled;
            if (enabled)
            {
               InitMeshes();
               UpdateDisplayMode();
               Event.current.Use();
            }
         }
   
         // brush adjustments
         const float adjustSpeed = 0.3f;
         if (Event.current.isKey && Event.current.type == EventType.KeyDown)
         {
            if (Event.current.keyCode == KeyCode.LeftBracket)
            {
               brushSize -= adjustSpeed;
               Repaint();
            }
            else if (Event.current.keyCode == KeyCode.RightBracket)
            {
               brushSize += adjustSpeed;
               Repaint();
            }
            else if (Event.current.keyCode == KeyCode.Semicolon)
            {
               brushFlow -= adjustSpeed;
               Repaint();
            }
            else if (Event.current.keyCode == KeyCode.Quote)
            {
               brushFlow += adjustSpeed;
               Repaint();
            }
            else if (Event.current.keyCode == KeyCode.Period)
            {
               brushFalloff -= adjustSpeed;
               Repaint();
            }
            else if (Event.current.keyCode == KeyCode.Slash)
            {
               brushFlow += adjustSpeed;
               Repaint();
            }
         }
      }

      void OnSceneGUI(SceneView sceneView)
      {
         DoShortcuts();

         deltaTime = EditorApplication.timeSinceStartup - lastTime;
         lastTime = EditorApplication.timeSinceStartup;

         if (jobs.Length == 0 && Selection.activeGameObject != null)
         {
            InitMeshes();
         }

         if (!enabled || jobs.Length == 0 || Selection.activeGameObject == null)
         {
            return;
         }

         if (tab == Tab.Utility)
         {
            return;
         }

         if (VertexInstanceStream.vertexShaderMat != null)
         {
            VertexInstanceStream.vertexShaderMat.SetFloat("_time", (float)EditorApplication.timeSinceStartup);
         }

         RaycastHit hit;
         float distance = float.MaxValue;
         Vector3 mousePosition = Event.current.mousePosition;

         // So, in 5.4, Unity added this value, which is basically a scale to mouse coordinates for retna monitors.
         // Not all monitors, just some of them.
         // What I don't get is why the fuck they don't just pass me the correct fucking value instead. I spent hours
         // finding this, and even the paid Unity support my company pays many thousands of dollars for had no idea
         // after several weeks of back and forth. If your going to fake the coordinates for some reason, please do
         // it everywhere to not just randomly break things everywhere you don't multiply some new value in. 
         float mult = EditorGUIUtility.pixelsPerPoint;

         mousePosition.y = sceneView.camera.pixelHeight - mousePosition.y * mult;
         mousePosition.x *= mult;
         Vector3 fakeMP = mousePosition;
         fakeMP.z = 20;
         Vector3 point = sceneView.camera.ScreenToWorldPoint(fakeMP);
         Vector3 normal = Vector3.forward;
         Ray ray = sceneView.camera.ScreenPointToRay(mousePosition);

         bool registerUndo = (Event.current.type == EventType.MouseDown && Event.current.button == 0 && Event.current.alt == false);
         bool toggleWireframe = (Event.current.type == EventType.KeyUp && Event.current.control);

         for (int i = 0; i < jobs.Length; ++i)
         {
            if (jobs[i] == null || jobs[i].meshFilter == null)
               continue;

            // Early out if we're not in the area..
            Bounds b = jobs[i].renderer.bounds;
            b.Expand(brushSize*2);
            if (!b.IntersectRay(ray))
            {
               continue;
            }

            if (registerUndo)
            {
               painting = true;
               // clear job edits
               for (int x = 0; x < jobEdits.Length; ++x)
               {
                  jobEdits[x] = false;
               }
               if (OnBeginStroke != null)
               {
                  OnBeginStroke(jobs);
               }
            }
            if (toggleWireframe)
            {
               SetWireframeDisplay(jobs[i].renderer, hideMeshWireframe);
            }

            Matrix4x4 mtx = jobs[i].meshFilter.transform.localToWorldMatrix;
            Mesh msh = jobs[i].meshFilter.sharedMesh;

            if (jobs[i].HasStream())
            {
               msh = jobs[i].stream.GetModifierMesh(); 
            }
            if (msh == null)
            {
               msh = jobs[i].meshFilter.sharedMesh;
            }
            if (RXLookingGlass.IntersectRayMesh(ray, msh, mtx, out hit))
            {
               if (Event.current.shift == false) 
               {
                  if (hit.distance < distance) 
                  {
                     distance = hit.distance;
                     point = hit.point;
                     oldpos = hit.point;
                     normal = hit.normal;
                     // if we don't have normal overrides, we have to recast against the shared mesh to get it's normal
                     // This could get a little strange if you modify the mesh, then delete the normal data, but in that
                     // case there's no real correct answer anyway without knowing the index of the vertex we're hitting.
                     if (normal.magnitude < 0.1f)
                     {
                        RXLookingGlass.IntersectRayMesh(ray, jobs[i].meshFilter.sharedMesh, mtx, out hit);
                        normal = hit.normal;
                     }
                  }
               } 
               else 
               {
                  point = oldpos;
               }
            } 
            else 
            {
               if (Event.current.shift == true) 
               {
                  point = oldpos;
               }
            }  
         }

         if (Event.current.type == EventType.KeyUp && Event.current.control && Event.current.keyCode == KeyCode.V)
         {
            showVertexShader = !showVertexShader;
            UpdateDisplayMode();
         }
         strokeDir = Vector3.zero;
         if (tab == Tab.Flow || vertexMode == VertexMode.Smear)
         {
            if (Event.current.isMouse)
            {
               strokeDir = (point - oldMousePosition);
               strokeDir.x *= Event.current.delta.magnitude;
               strokeDir.y *= Event.current.delta.magnitude;
               strokeDir.z *= Event.current.delta.magnitude;
               oldMousePosition = point;
            }
         }
         else if (vertexMode == VertexMode.Adjust)
         {
            strokeDir = -sceneView.camera.transform.forward;
         }
            
         if (Event.current.type == EventType.MouseMove && Event.current.shift) 
         {
            brushSize += Event.current.delta.x * (float)deltaTime * 6.0f;
            brushFalloff -= Event.current.delta.y * (float)deltaTime * 48.0f;
         }

         if (Event.current.rawType == EventType.MouseUp)
         {
            EndStroke();
         }
         if (Event.current.type == EventType.MouseMove && Event.current.alt)
         {
            brushSize += Event.current.delta.y * (float)deltaTime;
         }

         // set brush color
         if (tab == Tab.Custom && customBrush != null)
         {
            Handles.color = customBrush.GetPreviewColor();
         }
         else if (brushMode == BrushTarget.Color || brushMode == BrushTarget.UV0_AsColor || brushMode == BrushTarget.UV1_AsColor
            || brushMode == BrushTarget.UV2_AsColor || brushMode == BrushTarget.UV3_AsColor)
         {
            Handles.color = new Color(brushColor.r, brushColor.g, brushColor.b, 0.4f);
         }
         else if (brushMode == BrushTarget.ValueR || brushMode == BrushTarget.ValueG ||
                  brushMode == BrushTarget.ValueB || brushMode == BrushTarget.ValueA)
         {
            float v = (float)brushValue / 255.0f;
            Handles.color = new Color(v, v, v, 0.4f);
         }
         else
         {
            float v = (floatBrushValue - uvVisualizationRange.x) / Mathf.Max(0.00001f, uvVisualizationRange.y);
            Handles.color = new Color(v, v, v, 0.4f);
         }

         if (brushVisualization == BrushVisualization.Sphere)
         {
            Handles.SphereHandleCap(0, point, Quaternion.identity, brushSize * 2, EventType.Repaint);
         }
         else
         {
            Handles.color = new Color(0.8f, 0, 0, 1.0f);
            float r = Mathf.Pow(0.5f, brushFalloff);
            Handles.DrawWireDisc(point, normal, brushSize * r);
            Handles.color = new Color(0.9f, 0, 0, 0.8f);
            Handles.DrawWireDisc(point, normal, brushSize);
         }
         // eat current event if mouse event and we're painting
         if (Event.current.isMouse && painting)
         {
            Event.current.Use();
         } 

         if (Event.current.type == EventType.Layout)
         {
            HandleUtility.AddDefaultControl(GUIUtility.GetControlID(GetHashCode(), FocusType.Passive));
         }

         // only paint once per frame
         if (tab != Tab.Flow && Event.current.type != EventType.Repaint)
         {
            return;
         }


         if (jobs.Length > 0 && painting)
         {
            if (tab == Tab.Custom)
            {
               if (customBrush != null)
               {
                  customBrush.BeginApplyStroke(ray);
               }
            }
            var lerper = GetLerper();
            var value = GetBrushValue();
            for (int i = 0; i < jobs.Length; ++i)
            {
               Bounds b = jobs[i].renderer.bounds;
               b.Expand(brushSize*2);
               if (!b.IntersectRay(ray))
               {
                  continue;
               }
               if (jobEdits[i] == false)
               {
                  jobEdits[i] = true;
                  Undo.RegisterCompleteObjectUndo(jobs[i].stream, "Vertex Painter Stroke");
               }

               PaintMesh(jobs[i], point, lerper, value);
               Undo.RecordObject(jobs[i].stream, "Vertex Painter Stroke");

            }
         }

         if (jobs.Length > 0 && showVertexPoints)
         {
            for (int i = 0; i < jobs.Length; ++i)
            {
               DrawVertexPoints(jobs[i], point);
            }
         }

         // update views
         sceneView.Repaint();
         HandleUtility.Repaint();
      }
   }
}
