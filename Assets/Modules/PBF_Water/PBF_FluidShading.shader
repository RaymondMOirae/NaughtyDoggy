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
    	
    	float BlurRange;
    	float BlurScale;
    	float BlurDepthFallOff;

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

		float4 PassThroughFrag(v2f i) : SV_Target
		{
			float3 curDepth = tex2D(_MainTex, i.uv).rgb;
			return float4(curDepth.x, curDepth.y, curDepth.z, 1.0f);
		}


		float4 BilateralFilterVerticalFrag(v2f i) : SV_Target
    	{
			float curDepth = tex2D(_MainTex, i.uv).r;
			float curAlpha = tex2D(_MainTex, i.uv).w;
    		float sum = 0;
    		float wsum = 0;
			for(int j = -BlurRange; j <= BlurRange; j++)
			{
				float sample = tex2D(_MainTex, i.uv + float2(j, 0) * _MainTex_TexelSize.xy).x;	
				float alpha = tex2D(_MainTex, i.uv + float2(j, 0) * _MainTex_TexelSize.xy).w;

				if(alpha > 0)
				{
					float r = j * BlurScale;
					float w = exp(-r*r);

					float r2 = (sample - curDepth) * BlurDepthFallOff;
					float g = exp(-r2*r2);

					sum += sample * w * g;
					wsum += w * g;
				
				}
			}

    		if(wsum > 0.0f)
    		{
    			sum /= wsum;
    		}

    		if(curAlpha > 0)
				return float4(sum, sum, sum, curAlpha);
    		else
    			return tex2D(_MainTex, i.uv);
		}

    	float4 BilateralFilterHorizontalFrag(v2f i) : SV_Target
    	{
			float curDepth = tex2D(_MainTex, i.uv).r;
    		float curAlpha = tex2D(_MainTex, i.uv).w;
    		float sum = 0;
    		float wsum = 0;
			for(int j = -BlurRange; j <= BlurRange; j++)
			{
				float sample = tex2D(_MainTex, i.uv + float2(0, j) * _MainTex_TexelSize.xy).x;
				float alpha = tex2D(_MainTex, i.uv + float2(0, j) * _MainTex_TexelSize.xy).w;

				if(alpha > 0)
				{
					float r = j * BlurScale;
					float w = exp(-r*r);

					float r2 = (sample - curDepth) * BlurDepthFallOff;
					float g = exp(-r2*r2);

					sum += sample * w * g;
					wsum += w * g;
				
				}
				
			}

    		if(wsum > 0.0f)
    		{
    			sum /= wsum;
    		}

    		if(curAlpha > 0)
				return float4(sum, sum, sum, curAlpha);
    		else
    			return tex2D(_MainTex, i.uv);
    	}
		
    	
			
    	ENDCG

        Pass
        {
        	NAME "TEST_BLIT"
        	
            CGPROGRAM
			#pragma vertex BilateralFilterVert
			#pragma fragment PassThroughFrag
            ENDCG
        }
    	
    	Pass
    	{
    		NAME "BilateralFilterVertical"
    		
    		CGPROGRAM
    		#pragma vertex BilateralFilterVert
    		#pragma fragment BilateralFilterVerticalFrag
    		ENDCG
        }
    	
    	Pass
    	{
    		NAME "BilateralFilterHorizontal"
    		
    		CGPROGRAM
			#pragma vertex BilateralFilterVert
    		#pragma fragment BilateralFilterHorizontalFrag
    		ENDCG
		}
    }
}
