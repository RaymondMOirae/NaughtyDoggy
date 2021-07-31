Shader "Unlit/GrassShader"
{
    Properties
    {
        _MainTex ("NoiseTexture", 2D) = "white" {}
        _AmbientWeight ("AmbientWeight", float) = 0.2
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
            Tags{ "LightMode" = "ShadowCaster" }
            
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 2.0
            #pragma multi_compile_shadowcaster
            #pragma multi_compile_instancing
            #include "UnityCG.cginc"

            struct v2f
            {
                V2F_SHADOW_CASTER;
                UNITY_VERTEX_OUTPUT_STEREO
            };

            v2f vert(appdata_base v)
            {
                v2f o;
                UNITY_SETUP_INSTANCE_ID(v);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
                TRANSFER_SHADOW_CASTER_NORMALOFFSET(o);
                return o;
            }

            float4 frag (v2f i) : SV_Target
            {
                SHADOW_CASTER_FRAGMENT(i);
            }
            ENDCG
        }

        Pass
        {
            Tags { "LightMode" = "ForwardBase" }
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_fwdbase

            #include "UnityCG.cginc"
            #include "AutoLight.cginc"

            struct appdata
            {
                float objHeight : TEXCOORD0;
                float4 vertex : POSITION;
            };

            struct v2f
            {
                float objHeight : TEXCOORD0;
                float4 worldPos: TEXCOORD1;
                float4 pos : SV_POSITION;
                LIGHTING_COORDS(2,3)
                //SHADOW_COORDS(2)
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            float4 _TipColor;
            float4 _RootColor;
            float _XSamplingMultiplier;
            float _WaveStrength;
            float _AmbientWeight;
            
            v2f vert (appdata v)
            {
                v2f o;

                // calculate vertex's swaying offset
                float4 worldPos = mul(unity_ObjectToWorld, v.vertex);
                float4 samplerOffset = float4(worldPos.xz, 0, 0);
                
                samplerOffset.x += _Time.x * _XSamplingMultiplier;
                worldPos.x += v.vertex.y * (tex2Dlod(_MainTex, samplerOffset * _MainTex_ST).r  - 0.4f)  * _WaveStrength;

                o.objHeight = v.vertex.y;
                o.pos= UnityWorldToClipPos(worldPos);
                o.worldPos = worldPos;
                TRANSFER_VERTEX_TO_FRAGMENT(o)
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                UNITY_LIGHT_ATTENUATION(atten, i, i.worldPos);
                fixed4 colorBlend =  _TipColor * i.objHeight + _RootColor * (1 - i.objHeight);
                //fixed4 ambient = UNITY_LIGHTMODEL_AMBIENT * colorBlend * _AmbientWeight;
                atten = saturate(atten + _AmbientWeight);
                return colorBlend * atten;
            }
            ENDCG
        }
    }
    
    Fallback "VertexLit"
}