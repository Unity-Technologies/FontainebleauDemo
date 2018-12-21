// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

// I highly recomend you look in the example shaders rather than this one if your wondering how to best use
// these tools. This shader is designed to not use textures and show visual information, which makes it rather
// expensive since it has lots of procedural functions and a ton of display options in a single shader.


Shader "Hidden/VertexPainterPro_Preview"
{
   Properties
   {

   }
   SubShader
   {
      Tags { "RenderType"="Opaque" }


      Pass
      {
         CGPROGRAM
         #pragma vertex vert
         #pragma fragment frag

         #include "UnityCG.cginc"

         struct appdata
         {
            float4 vertex : POSITION;
            float4 color  : COLOR;
            float4 uv0    : TEXCOORD0;
            float4 uv1    : TEXCOORD1;
            float4 uv2    : TEXCOORD2;
            float4 uv3    : TEXCOORD3;
         };

         struct v2f
         {
            float4 vertex : SV_POSITION;
            float4 color  : COLOR0;
            float2 uv     : TexCoord0;
            float2 flow   : TexCoord1;
         };

         int  _channel;
         float2 _uvRange;
         int _flowVisualization;
         int _flowTarget;
         int _tab;
         float _time;   // because _Time won't work in edit mode..

         float Range(float f)
         {
            f -= _uvRange.x;
            f /= max(0.0001f, _uvRange.y);
            return f;
         }


         // 2d noise
         float Hash( float2 p )
         {
            float h = dot(p, float2(127.1,311.7));
            return frac(sin(h)*43758.5453123);
         }

         float Noise2D(float2 p )
         {
            float2 i = floor( p );
            float2 f = frac( p );

            float2 u = f*f*(3.0-2.0*f);
       
            float cLowerLeft = Hash( i + float2(0.0,0.0));
            float cLowerRight = Hash( i + float2(1.0,0.0));
            float cUpperLeft = Hash( i + float2(0.0,1.0));
            float cUpperRight = Hash( i + float2(1.0,1.0));
            
   
            float nLower = lerp(cLowerLeft, cLowerRight, u.x);
            float nUpper = lerp(cUpperLeft, cUpperRight, u.x);
            float noise = lerp(nLower, nUpper, u.y);
            return noise;
         }
         
         // 3d noise for water
         float mod289(float x) { return x - floor(x * (1.0 / 289.0)) * 289.0; }
         float4 mod289(float4 x) { return x - floor(x * (1.0 / 289.0)) * 289.0; }
         float4 perm(float4 x) { return mod289(((x * 34.0) + 1.0) * x);  }

         float Noise3d(float3 p)
         {
             float3 a = floor(p);
             float3 d = p - a;
             d = d * d * (3.0 - 2.0 * d);

             float4 b = a.xxyy + float4(0.0, 1.0, 0.0, 1.0);
             float4 k1 = perm(b.xyxy);
             float4 k2 = perm(k1.xyxy + b.zzww);

             float4 c = k2 + a.zzzz;
             float4 k3 = perm(c);
             float4 k4 = perm(c + 1.0);

             float4 o1 = frac(k3 * (1.0 / 41.0));
             float4 o2 = frac(k4 * (1.0 / 41.0));

             float4 o3 = o2 * d.z + o1 * (1.0 - d.z);
             float2 o4 = o3.yw * d.x + o3.xz * (1.0 - d.x);

             return o4.y * d.y + o4.x * (1.0 - d.y);
         }
         
         float4 Water(float2 uv)
         {
            float3x3 r = float3x3(0.36, 0.48, -0.8, -0.8, 0.60, 0.0, 0.48, 0.64, 0.60);
            float3 pos = mul(r, float3(uv * float2(16.0, 9.0), 0.0));
            float3 time = mul(r, float3(0.0, 0.0, _time));
         
            float4 n = float4(Noise3d(pos + time), Noise3d(pos/2.02 + time), Noise3d(pos/4.01 + time), Noise3d(pos/8.12 + time));
            float p = dot(abs(2.0 * n - 1.0), float4(0.5, 0.25, 0.125, 0.125));
            float q = sqrt(p);

            return float4(1.0 - q, 1.0 - 0.5 * q, 1.0, 1.0);
         }
         
         // flowmap water
         float4 FlowWater(float2 uv, float2 flow)
         {
            float scale = 5;
               
            float n = Noise2D(uv*9);
            
            float2 flowVector = (flow * 2.0 - 1.0);
            
            float timeScale = _time * 0.05 * scale + n;
            float2 phase;
            
            phase.x = frac(timeScale);
            phase.y = frac(timeScale + 0.5);
     
            fixed4 color0;
            fixed4 color1;

            color0 = Water(uv * scale - flowVector * half2(phase.x, phase.x));
            color1 = Water(uv * scale - flowVector * half2(phase.y, phase.y));
        
         
            fixed flowInterp = abs(0.5 - phase.x) / 0.5;
            return lerp(color0, color1, flowInterp);
         }
         
         v2f vert (appdata v)
         {
            v2f o;
            o.uv = v.uv0.xy;
            o.vertex = UnityObjectToClipPos(v.vertex); 
            if (_tab > 1.9 && _tab < 2.1)
            {
               if (_flowTarget < 1)
               {
                  o.flow = v.color.rg;
               }
               else if (_flowTarget < 2)
               {
                  o.flow = v.color.ba;
               }
               else if (_flowTarget < 3)
               {
                  o.flow = v.uv0.xy;
               }
               else if (_flowTarget < 4)
               {
                  o.flow = v.uv0.zw;
               }
               else if (_flowTarget < 5)
               {
                  o.flow = v.uv1.xy;
               }
               else if (_flowTarget < 6)
               {
                  o.flow = v.uv1.zw;
               }
               else if (_flowTarget < 7)
               {
                  o.flow = v.uv2.xy;
               }
               else if (_flowTarget < 8)
               {
                  o.flow = v.uv2.zw;
               }
               else if (_flowTarget < 9)
               {
                  o.flow = v.uv3.xy;
               }
               else if (_flowTarget < 10)
               {
                  o.flow = v.uv3.zw;
               }
               return o;
            }
            else if (_channel < 1)
            {              
               o.color = v.color;
            }
            else if (_channel < 2)
            {
               o.color = v.color.rrrr;
            }
            else if (_channel < 3)
            {
               o.color = v.color.gggg;
            }
            else if (_channel < 4)
            {
               o.color = v.color.bbbb;
            }
            else if (_channel < 5)
            {
               o.color = v.color.aaaa;
            }
            else if (_channel < 6)
            {
               float f = Range(v.uv0.x); o.color = fixed4(f,f,f,1);
            }
            else if (_channel < 7)
            {
               float f = Range(v.uv0.y); o.color = fixed4(f,f,f,1);
            }
            else if (_channel < 8)
            {
               float f = Range(v.uv0.z); o.color = fixed4(f,f,f,1);
            }
            else if (_channel < 9)
            {
               float f = Range(v.uv0.w); o.color = fixed4(f,f,f,1);
            }
            else if (_channel < 10)
            {
               float f = Range(v.uv1.x); o.color = fixed4(f,f,f,1);
            }
            else if (_channel < 11)
            {
               float f = Range(v.uv1.y); o.color = fixed4(f,f,f,1);
            }
            else if (_channel < 12)
            {
               float f = Range(v.uv1.z); o.color = fixed4(f,f,f,1);
            }
            else if (_channel < 13)
            {
               float f = Range(v.uv1.w); o.color = fixed4(f,f,f,1);
            }
            else if (_channel < 14)
            {
               float f = Range(v.uv2.x); o.color = fixed4(f,f,f,1);
            }
            else if (_channel < 15)
            {
               float f = Range(v.uv2.y); o.color = fixed4(f,f,f,1);
            }
            else if (_channel < 16)
            {
               float f = Range(v.uv2.z); o.color = fixed4(f,f,f,1);
            }
            else if (_channel < 17)
            {
               float f = Range(v.uv2.w); o.color = fixed4(f,f,f,1);
            }
            else if (_channel < 18)
            {
               float f = Range(v.uv3.x); o.color = fixed4(f,f,f,1);
            }
            else if (_channel < 19)
            {
               float f = Range(v.uv3.y); o.color = fixed4(f,f,f,1);
            }
            else if (_channel < 20)
            {
               float f = Range(v.uv3.z); o.color = fixed4(f,f,f,1);
            }
            else if (_channel < 21)
            {
               float f = Range(v.uv3.w); o.color = fixed4(f,f,f,1);
            }
            else if (_channel < 22)
            {
               o.color = v.uv0;
            }
            else if (_channel < 23)
            {
               o.color = v.uv1;
            }
            else if (_channel < 24)
            {
               o.color = v.uv2;
            }
            else if (_channel < 25)
            {
               o.color = v.uv3;
            }
            return o;
         }


         // Line SDF
         float Line(float2 p, float2 p1, float2 p2) 
         {
            float2 center = (p1 + p2) * 0.5;
            float len = length(p2 - p1);
            float2 dir = (p2 - p1) / len;
            float2 rel_p = p - center;
            float dist1 = abs(dot(rel_p, float2(dir.y, -dir.x)));
            float dist2 = abs(dot(rel_p, dir)) - 0.5 * len;
            return max(dist1, dist2);
         }

         float Arrow(float2 p, float2 v) 
         {
            float arrow_scale = 40;
            v *= arrow_scale;
            p -= (floor(p / arrow_scale) + 0.5) * arrow_scale;
               
            float mv = length(v);
            float mp = length(p);
            
            if (mv > 0.0) 
            {
               float2 dir_v = v / mv;
               
               mv = clamp(mv, 5.0, arrow_scale * 0.5);
               v = dir_v * mv;
               float shaft = Line(p, v, -v);
               float head = min(Line(p, v, 0.4 * v + 0.2 * float2(-v.y, v.x)), Line(p, v, 0.4 * v + 0.2 * float2(v.y, -v.x)));

               return min(shaft, head);
            } 
            else 
            {
               return mp;
            }
         }


         fixed4 frag (v2f i) : SV_Target
         {
            if (_tab > 1.9 && _tab < 2.1) // flow
            {
               if (_flowVisualization < 0.1)
               {
                  float2 flow = i.flow * 2 - 1;
                  float arrow_dist = Arrow(i.uv*500, flow * 0.4);
                  return lerp(fixed4(0,0,0,1), fixed4(i.uv, 0, 1), saturate(arrow_dist));
               }
               else
               {
                  return FlowWater(i.uv, i.flow);
               }
            }
         
            return i.color;
         }
         ENDCG
      }
   }
}
