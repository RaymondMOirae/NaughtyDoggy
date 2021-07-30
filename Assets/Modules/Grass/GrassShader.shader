Shader "Unlit/GrassShader"
{
    Properties
    {
        _MainTex ("NoiseTexture", 2D) = "white" {}
        _TipColor ("GrassTipColor", Color) = (1, 1, 1, 1)
        _RootColor ("GrassRootColor", Color) = (0, 0, 0, 0)
        _XSamplingMultiplier ("XSamplingMultiplier", float) = 0.5
        _WaveStrength ("WaveStrenth", float) = 1.0
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 100

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            struct appdata
            {
                float objHeight : TEXCOORD0;
                float4 vertex : POSITION;
            };

            struct v2f
            {
                float objHeight : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            float4 _TipColor;
            float4 _RootColor;
            float _XSamplingMultiplier;
            float _WaveStrength;
            v2f vert (appdata v)
            {
                v2f o;
                float4 worldPos = mul(unity_ObjectToWorld, v.vertex);
                float4 samplerOffset = float4(worldPos.xz, 0, 0);
                
                samplerOffset.x += _Time.x * _XSamplingMultiplier;
                worldPos.x += v.vertex.y * (tex2Dlod(_MainTex, samplerOffset * _MainTex_ST).r  - 0.4f)  * _WaveStrength;
               
                o.objHeight = v.vertex.y;
                o.vertex = UnityWorldToClipPos(worldPos);
                
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                // return float4(i.uv, 0, 0);
                return _TipColor * i.objHeight + _RootColor * (1 - i.objHeight);
            }
            ENDCG
        }
    }
}
