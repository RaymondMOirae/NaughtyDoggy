Shader "Unlit/PBF_FluidShading"
{
    Properties
    {
		_MainTex ("Texture", 2D) = "white" {}
    	_Cubemap ("Skybox", Cube) = "_Skybox" {}
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 100
    	
    	CGINCLUDE
			#include "UnityCG.cginc"
    		#include "Lighting.cginc"
    		#include "AutoLight.cginc"
			
			sampler2D _MainTex;
			half4 _MainTex_TexelSize;
    		samplerCUBE _Cubemap;
			
			float BlurRange;
			float BlurScale;
			float BlurDepthFallOff;
    	
    		float4 _SpecularColor;
    		float4 _DiffuseColor;
    		float4 _ReflectionColor;
    		float _Gloss;
    		float _Reflectance;

			struct v2f
			{
				float2 uv : TEXCOORD0;
				float4 pos: SV_POSITION;
			};

			v2f BilateralFilterCommonVert(appdata_img v)
			{
				v2f o;
				o.pos= UnityObjectToClipPos(v.vertex);
				o.uv = v.texcoord;
				return o;
			}

			float4 PassThroughFrag(v2f i) : SV_Target
			{
				float3 curDepth = tex2D(_MainTex, i.uv).rgb;
				return float4(curDepth.x, curDepth.y, curDepth.z, 0.0f);
			}


			float4 BilateralFilterVerticalFrag(v2f i) : SV_Target
			{
				float4 curPixel = tex2D(_MainTex, i.uv);
				if(curPixel.w == 0)
					return float4(0, 0, 0, 0);
				float sum = 0;
				float wsum = 0;
				[loop]
				for(int j = -BlurRange; j <= BlurRange; j++)
				{
					float2 sample = tex2D(_MainTex, i.uv + float2(j, 0) * _MainTex_TexelSize.xy).zw;

					if(sample.y > 0)
					{
						float r = j * BlurScale;
						float w = exp(-r*r);

						float r2 = (sample.x - curPixel.x) * BlurDepthFallOff;
						float g = exp(-r2*r2);

						sum += sample.x * w * g;
						wsum += w * g;
					
					}
					
				}

				if(wsum > 0.0f)
				{
					sum /= wsum;
				}

				return float4(sum, sum, sum, curPixel.w);
			}

			float4 BilateralFilterHorizontalFrag(v2f i) : SV_Target
			{
				
				float4 curPixel = tex2D(_MainTex, i.uv);
				if(curPixel.w == 0)
					return float4(0, 0, 0, 0);
				float sum = 0;
				float wsum = 0;
				[loop]
				for(int j = -BlurRange; j <= BlurRange; j++)
				{
					float2 sample = tex2D(_MainTex, i.uv + float2(0, j) * _MainTex_TexelSize.xy).zw;

					if(sample.y > 0)
					{
						float r = j * BlurScale;
						float w = exp(-r*r);

						float r2 = (sample.x - curPixel.x) * BlurDepthFallOff;
						float g = exp(-r2*r2);

						sum += sample.x * w * g;
						wsum += w * g;
					
					}
					
				}

				if(wsum > 0.0f)
				{
					sum /= wsum;
				}

				return float4(sum, sum, sum, curPixel.w);
			}
			
			float3 DepthToViewPos(float curPixel, float2 uv)
			{
				uv *= _MainTex_TexelSize.xy;
				float3 clip_Vec = float3(uv * 2 - 1 , 1.0) * _ProjectionParams.z;
				float3 view_Vec = mul(unity_CameraInvProjection, clip_Vec.xyzz).xyz;
				float3 view_Pos = view_Vec * Linear01Depth(curPixel);
				
				return view_Pos;
			}

			float4 NormalCalculation(v2f i) : SV_Target
			{
				float4 curPixel = tex2D(_MainTex, i.uv);
				
				if(curPixel.w == 0.0)
				{
					return float4(0, 0, 0, 0);
				}

				float rightPixel = tex2D(_MainTex, i.uv + float2(1, 0) * _MainTex_TexelSize.xy).r;
				float leftPixel = tex2D(_MainTex, i.uv + float2(-1, 0) * _MainTex_TexelSize.xy).r;
				float upperPixel = tex2D(_MainTex, i.uv + float2(0, 1) * _MainTex_TexelSize.xy).r;
				float downsidePixel = tex2D(_MainTex, i.uv + float2(0, -1) * _MainTex_TexelSize.xy).r;
				
				float3 curPixelPos = DepthToViewPos(curPixel.x, i.pos.xy);
				float3 rightPixelPos = DepthToViewPos(rightPixel, i.pos.xy + float2(1, 0));
				float3 leftPixelPos = DepthToViewPos(leftPixel, i.pos.xy + float2(-1, 0));
				float3 upperPixelPos = DepthToViewPos(upperPixel, i.pos.xy + float2(0, 1));
				float3 downsidePixelPos = DepthToViewPos(downsidePixel, i.pos.xy + float2(0, -1));
				
				float3 ddx = rightPixelPos - curPixelPos;
				float3 ddx2 = curPixelPos - leftPixelPos;
				
				if(abs(ddx.z) > abs(ddx2.z))
				{
					ddx = ddx2;
				}
				
				float3 ddy = upperPixelPos - curPixelPos;
				float3 ddy2 = curPixelPos - downsidePixelPos;
				
				if(abs(ddy.z) > abs(ddy2.z))
				{
					ddy = ddy2;
				}

				float3 normal = cross(ddx, ddy);
				normal = normalize(normal);
				
				float4 worldNormal = mul(unity_MatrixInvV, normal);
				float4 curPixelWorldPos = mul(unity_MatrixInvV, curPixelPos);
				
                float3 worldLightDir = normalize(UnityWorldSpaceLightDir(curPixelWorldPos));
                float3 worldViewDir = normalize(UnityWorldSpaceViewDir(curPixelWorldPos));
				
                float3 ambient = UNITY_LIGHTMODEL_AMBIENT.xyz;
                float3 diffuse = _LightColor0.rgb * _DiffuseColor.xyz * max(0, dot(worldNormal, worldLightDir));
				diffuse = diffuse * 0.5 + 0.5;
				
                float3 halfDir = normalize(worldViewDir + worldLightDir);
                float3 specular = _LightColor0.rgb * _SpecularColor.xyz * pow(max(0, dot(worldNormal, halfDir)), _Gloss);

				float3 worldReflect = reflect(-worldViewDir, worldNormal);
				float3 reflection = texCUBE(_Cubemap, worldReflect).rgb * _ReflectionColor.xyz;
				float3 fresnel = _Reflectance + (1 - _Reflectance) * pow(1 - dot(worldNormal, worldViewDir), 5);
				
                float3 color = ambient + specular + lerp(diffuse, reflection, saturate(fresnel));
				
				return float4(color, curPixel.r);
			}
			

    	ENDCG

        Pass
        {
        	NAME "TEST_BLIT"
        	
            CGPROGRAM
			#pragma vertex BilateralFilterCommonVert
			#pragma fragment PassThroughFrag
            ENDCG
        }
    	
    	Pass
    	{
    		NAME "BilateralFilterVertical"
    		
    		CGPROGRAM
    		#pragma vertex BilateralFilterCommonVert
    		#pragma fragment BilateralFilterVerticalFrag
    		ENDCG
        }
    	
    	Pass
    	{
    		NAME "BilateralFilterHorizontal"
    		
    		CGPROGRAM
			#pragma vertex BilateralFilterCommonVert
    		#pragma fragment BilateralFilterHorizontalFrag
    		ENDCG
		}
    	
    	Pass
    	{
	        NAME "CalculateNormal"
    		
    		CGPROGRAM
			#pragma vertex BilateralFilterCommonVert
    		#pragma fragment NormalCalculation
    		ENDCG
		}
    }
}
