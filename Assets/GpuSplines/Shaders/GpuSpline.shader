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
        LOD 100

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
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_local LINEAR CATMULLROM

            #include "UnityCG.cginc"

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
            float4 _ControlPoints[1000];
            // xyz = color, w = width
            half4 _ColorAndWidth;

            v2f vert(appdata v)
            {
                int index = v.vertex.z;
                float t = v.vertex.y;


                float3 cp0 = _ControlPoints[index].xyz;
                float3 cp1 = _ControlPoints[index + 1].xyz;
                float3 cp2 = _ControlPoints[index + 2].xyz;
                float3 cp3 = _ControlPoints[index + 3].xyz;

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

                pos = pos + lerp(float3(-tang.y, tang.x, 0), float3(tang.y, -tang.x, 0), v.color.r);

                v2f o;
                o.positionCS = UnityObjectToClipPos(pos.xyz);
                o.uv = float2(v.color.r, v.vertex.x);
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                // sample the texture
                fixed4 col = tex2D(_MainTex, i.uv);
                col.rgb *= _ColorAndWidth.rgb;
                return col;
            }
            ENDCG
        }
    }
}