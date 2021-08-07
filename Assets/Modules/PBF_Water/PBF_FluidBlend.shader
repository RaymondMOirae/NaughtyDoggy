Shader "Unlit/PBF_FluidBlend"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 100
        CGINCLUDE
            #include "UnityCG.cginc"
            #include "AutoLight.cginc"

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            sampler2D _CameraDepthTexture;
            float4 _CameraDepth_ST;
            sampler2D _FluidTex;
            float4 _FluidTex_ST;
            float _FluidBlendWeight;

            v2f PassThroughVert (appdata_img v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.texcoord;
                return o;
            }

            float4 BlendShadingFrag (v2f i) : SV_Target
            {
                // sample the texture

                float4 _backColor = tex2D(_MainTex, i.uv);
                float4 _fluidColor = tex2D(_FluidTex, i.uv).rgba;
                float curPixelDepth = tex2D(_CameraDepthTexture, i.uv);
                
                if(_fluidColor.w == 0 || curPixelDepth > _fluidColor.w)
                {
                    return _backColor;
                }
            
                float3 color = _fluidColor * _FluidBlendWeight + _backColor * (1 - _FluidBlendWeight);
                //return float4(curPixelDepth, 1, 1, 1);
                return float4(color, 1);
            }

        ENDCG

        Pass
        {
            CGPROGRAM
                #pragma vertex PassThroughVert
                #pragma fragment BlendShadingFrag
                    
            ENDCG
        }
    }
}
