Shader "Unlit/PBF_ParticleDepth"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
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
            #pragma target 4.5
            #include "UnityCG.cginc"

            sampler2D _MainTex;
            float4 _MainTex_ST;
            
            float ParticleScale;
            float3 ParentPos;
            float3 ParentScale;
			// float4x4 LocalToWorldMatrix;

			#if SHADER_TARGET >=45

				struct ParticleData
				{
					float3 curPosition;
					float3 nextPosition;
					float3 deltaPosition;
					float3 extForce;
					float3 velocity;
					float lambda;
				};

				StructuredBuffer<ParticleData> ParticleBuffer;
				

				float4x4 TMatrixToWorldPos(float3 worldPos)
				{
					return float4x4(ParticleScale, 0, 0, worldPos.x,
								   0, ParticleScale, 0, worldPos.y,
								   0, 0, ParticleScale, worldPos.z,
								   0, 0, 0, 1);
				}

				float4x4 ScaleToParent(float3 parentPos, float3 parentScale)
				{
					return float4x4(parentScale.x, 0, 0, parentPos.x,
									0, parentScale.y, 0, parentPos.y,
									0, 0, parentScale.z, parentPos.z,
									0, 0, 0 ,1);
				}
			#endif
            
            struct appdata
            {
                float4 vertex : POSITION;
            };

            struct v2f
            {
				float zDepth : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            v2f vert (appdata v, uint id : SV_INSTANCEID)
            {
                v2f o;
            	
				#if SHADER_TARGET >=45
					v.vertex = mul(TMatrixToWorldPos(ParticleBuffer[id.x].curPosition), v.vertex);
            		v.vertex = mul(ScaleToParent(ParentPos, ParentScale), v.vertex);
				#endif

                o.vertex = UnityObjectToClipPos(v.vertex);
            	
				o.zDepth = o.vertex.z / o.vertex.w;
            	
                return o;
            }
            
            fixed4 frag (v2f i) : SV_Target
            {
            	float zDepth = i.zDepth;
            	#if !defined(UNITY_REVERSED_Z)
					zDepth = zDepth * 0.5 + 0.5;
            	#endif
				
                // return fixed4(i.vertex.z, i.vertex.z, i.vertex.z, 1);
                return fixed4(zDepth, zDepth, zDepth, 1);
            }
            ENDCG
        }
    }
}
