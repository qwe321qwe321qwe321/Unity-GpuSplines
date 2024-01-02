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

        Cull Off
        Lighting Off
        ZWrite On
        Fog
        {
            Mode Off
        }

        Pass
        {
            HLSLPROGRAM
            //#define DEBUG_DRAW

            // The device needs to support ComputeBuffer.
            // https://docs.unity3d.com/2019.4/Documentation/Manual/SL-ShaderCompileTargets.html
            #pragma target 4.5
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_local LINEAR CATMULLROM

            #include "UnityCG.cginc"
            #include "GpuSplineLib.hlsl"

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
            half4 _LineColor;

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
                 const float leftOrRight = vertex_leftOrRight[v_idx];
                 const float t = segment.data.x;
                 const float tex_v = segment.data.y;
                 v.uv = float2(tex_v, 0);
                 return v;
#else

                const uint cp0_index = segment.index;
                const float t = segment.data.x;
                const float tex_v = segment.data.y;
                // left = 0, right = 1
                const float leftOrRight = vertex_leftOrRight[v_idx];
                
                float3 pos = ComputeSplineVertex(cp0_index, t, leftOrRight);
                
                v2f o;
                o.positionCS = UnityObjectToClipPos(pos.xyz);
                o.uv = float2(leftOrRight, tex_v);
                return o;
#endif
            }

            half4 frag(v2f i) : SV_Target
            {
                #ifdef DEBUG_DRAW
                half4 col = half4(i.uv.x, i.uv.y, 1, 1);
                #else

                // sample the texture
                half4 col = tex2D(_MainTex, i.uv);
                col.rgb *= _LineColor.rgb;
                #endif
                return col;
            }
            ENDHLSL
        }
    }
}