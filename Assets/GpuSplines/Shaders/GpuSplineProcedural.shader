Shader "Unlit/GpuSplineProcedural"
{
    Properties
    {
        [NoScaleOffset] _MainTex ("Texture", 2D) = "white" {}
    }
    SubShader
    {
        Tags
        {
            "RenderType"="Opaque"
        }
        LOD 200

        Cull Off
        Lighting Off
        ZWrite On
        Fog
        {
            Mode Off
        }

        Pass
        {
            CGPROGRAM
            //#define DEBUG_DRAW

            // The device needs to support ComputeBuffer.
            // https://docs.unity3d.com/2019.4/Documentation/Manual/SL-ShaderCompileTargets.html
            #pragma target 4.5
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_local LINEAR CATMULLROM

            #include "UnityCG.cginc"

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 positionCS : SV_POSITION;
            };

            struct Segment
            {
                // index in the control point uniform.
                uint index;
                // data.x = spline interval t [0..1].
                // data.y = V texture coordinate.
                // data.z = isNotEnd. 0 = the end of the spline. 1 = Not end.
                half3 data;
            };

            StructuredBuffer<Segment> _SegmentBuffer;

            sampler2D _MainTex;
            float4 _ControlPoints[1000];
            // xyz = color, w = width
            half4 _ColorAndWidth;

            #ifdef DEBUG_DRAW
            static const float3 _tempPos[6] = {
                float3(0, 0, 0),
                float3(0, 1, 0),
                float3(1, 0, 0),
                float3(0, 1.1, 0),
                float3(1, 1.1, 0),
                float3(1, 0.1, 0)
            };
            #endif

            static const float vertex_leftOrRight[6] = {
                1, 0, 1, 0, 0, 1
            };
            static const float vertex_segment_offset[6] = {
                0, 0, 1, 0, 1, 1
            };

            v2f vert(uint vid : SV_VertexID)
            {
                uint seg_idx = vid / 6; // Segment (Quad) index.
                uint v_idx = vid - seg_idx * 6; // Vertex index in a segment.

                const half isNotEnd = _SegmentBuffer[seg_idx].data.z;
                const Segment segment = _SegmentBuffer[seg_idx + vertex_segment_offset[v_idx] * isNotEnd];
#ifdef DEBUG_DRAW
                 v2f v;
                 float3 pos = _tempPos[v_idx] + float3(1, 0, 0) * seg_idx;
                 //pos = _tempPos[vid];
                 v.positionCS = UnityObjectToClipPos(pos.xyz);
                 float leftOrRightVertex = vertex_leftOrRight[v_idx];
                 v.uv = float2(segment.tex_v, 0);
                 return v;
#else

                uint cp_idx = segment.index;
                float t = segment.data.x;
                float tex_v = segment.data.y;
                // left = 0, right = 1
                float leftOrRightVertex = vertex_leftOrRight[v_idx];
                
                float3 cp0 = _ControlPoints[cp_idx].xyz;
                float3 cp1 = _ControlPoints[cp_idx +1].xyz;
                float3 cp2 = _ControlPoints[cp_idx +2].xyz;
                float3 cp3 = _ControlPoints[cp_idx +3].xyz;
                
                #if CATMULLROM
                float3 base0 = -cp0 + cp3 + (cp1 - cp2) * 3;
                float3 base1 = 2*cp0 - 5*cp1 + 4*cp2 - cp3;
                
                float3 pos = 0.5 * (base0 * (t*t*t) + base1 * (t*t) + (-cp0+cp2)*t + 2 * cp1);
                float3 tang = base0 * (t * t * 1.5) + base1 * t + 0.5 * (cp2 - cp0);
                #else
                // LINEAR
                float3 pos = lerp(cp1, cp2, t);
                float3 tang = cp2 - cp1;
                #endif
                
                // extend half width.
                tang = normalize(tang) * _ColorAndWidth.w * 0.5;
                
                pos = pos + lerp(float3(-tang.y, tang.x, 0), float3(tang.y, -tang.x, 0), leftOrRightVertex);
                
                v2f o;
                o.positionCS = UnityObjectToClipPos(pos.xyz);
                o.uv = float2(leftOrRightVertex, tex_v);
                return o;
#endif
            }

            fixed4 frag(v2f i) : SV_Target
            {
                #ifdef DEBUG_DRAW
                fixed4 col = fixed4(i.uv.x, i.uv.y, 1, 1);
                #else

                // sample the texture
                fixed4 col = tex2D(_MainTex, i.uv);
                col.rgb *= _ColorAndWidth.rgb;
                #endif
                return col;
            }
            ENDCG
        }
    }
}