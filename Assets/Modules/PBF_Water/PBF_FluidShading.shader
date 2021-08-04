Shader "Unlit/PBF_FluidShading"
{
    Properties
    {
		_MainTex ("Texture", 2D) = "white" {}
    	_BlurSize ("Blur Size", Float) = 1.0
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 100
    	
    	CGINCLUDE
			 #include "UnityCG.cginc"
			sampler2D _MainTex;
			half4 _MainTex_TexelSize;
			float _FilterRadius;

			struct v2f
			{
				float2 uv : TEXCOORD0;
				float4 vertex : SV_POSITION;
			};

			v2f BilateralFilterVert(appdata_img v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv = v.texcoord;
				return o;
			}

			float4 BilateralFilterFrag(v2f i) : SV_Target
			{
				float3 curDepth = tex2D(_MainTex, i.uv).rgb;
				return float4(curDepth.x, curDepth.y, curDepth.z, 1.0f);
			}
			
    	ENDCG

        Pass
        {
        	NAME "TEST_BLIT"
            CGPROGRAM
				#pragma vertex BilateralFilterVert
				#pragma fragment BilateralFilterFrag
            ENDCG
        }
    }
}
