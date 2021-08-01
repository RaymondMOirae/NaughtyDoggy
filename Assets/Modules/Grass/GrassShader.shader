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
        _PushDownRadius ("PushDownRadius", float) = 1.0
        _PushDownWeight ("PushDownWeight", float) = 1.0
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 100
        
        CGINCLUDE
            #include "UnityCG.cginc"

            sampler2D _MainTex;
            float4 _MainTex_ST;

            float4 _PlayerPos;
            float _PushDownWeight;
            float _PushDownRadius;
            
            float4 _TipColor;
            float4 _RootColor;
            float _XSamplingMultiplier;
            float _WaveStrength; 
       

            float4 CalcVertexPushDownNWavingToClipPos(float4 objSpacePos)
            {
                float4 worldPos = mul(unity_ObjectToWorld, objSpacePos);
                float4 samplePos = float4(worldPos.xz, 0, 0);
                
                samplePos.x += _Time.x * _XSamplingMultiplier;
                worldPos.x += objSpacePos.y * (tex2Dlod(_MainTex, samplePos * _MainTex_ST).r  - 0.4f)  * _WaveStrength;

                // calculate collision offset
                float4 pushDownDir = worldPos - _PlayerPos;
                float offsetLen = length(pushDownDir);
                pushDownDir = normalize(pushDownDir);
                pushDownDir.y *= 0.2f;
                worldPos += pushDownDir * saturate((_PushDownRadius - offsetLen) * objSpacePos.y)* _PushDownWeight ;

                return UnityWorldToClipPos(worldPos);
            }
        ENDCG
        
        Pass
        {
            Tags{ "LightMode" = "ShadowCaster" }

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 2.0
            #pragma multi_compile_shadowcaster
            #pragma multi_compile_instancing
            
            struct v2f
            {
                V2F_SHADOW_CASTER;
                UNITY_VERTEX_OUTPUT_STEREO
            };
            
            v2f vert(appdata_base v)
            {
                v2f o;
 
                UNITY_SETUP_INSTANCE_ID(v);
                
                // UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
                // TRANSFER_SHADOW_CASTER_NORMALOFFSET(o);
                o.pos = CalcVertexPushDownNWavingToClipPos(v.vertex);
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
                SHADOW_COORDS(2)
            };

            float _AmbientWeight;
            
            v2f vert (appdata v)
            {
                v2f o;

                o.objHeight = v.vertex.y;
                o.pos = CalcVertexPushDownNWavingToClipPos(v.vertex);
                o.worldPos = mul(unity_ObjectToWorld, v.vertex);
                TRANSFER_VERTEX_TO_FRAGMENT(o)
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                UNITY_LIGHT_ATTENUATION(atten, i, i.worldPos);
                fixed4 colorBlend =  _TipColor * i.objHeight + _RootColor * (1 - i.objHeight);
                atten = saturate(atten + _AmbientWeight);
                return colorBlend * atten;
            }
            ENDCG
        }
    }
    
    Fallback "VertexLit"
}
