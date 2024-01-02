Shader "Unlit/GpuSpline"
{
    Properties
    {
        [NoScaleOffset] _MainTex ("Texture", 2D) = "white" {}
    }
    SubShader
    {
        Tags
        {
            "IgnoreProjector" = "True"
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
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_local LINEAR CATMULLROM

            #include "UnityCG.cginc"
            #include "GpuSplineLib.hlsl"

            struct appdata
            {
                float4 vertex : POSITION;
                half4 color : COLOR;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 positionCS : SV_POSITION;
            };

            sampler2D _MainTex;
            half4 _LineColor;

            v2f vert(appdata v)
            {
                const int cp0_index = v.vertex.z;
                const float t = v.vertex.y;
                const float leftOrRight = v.color.r;

                float3 pos = ComputeSplineVertex(cp0_index, t, leftOrRight);

                v2f o;
                o.positionCS = UnityObjectToClipPos(pos.xyz);
                o.uv = float2(leftOrRight, v.vertex.x);
                return o;
            }

            half4 frag(v2f i) : SV_Target
            {
                // sample the texture
                half4 col = tex2D(_MainTex, i.uv);
                col.rgb *= _LineColor.rgb;
                return col;
            }
            ENDHLSL
        }
    }
}