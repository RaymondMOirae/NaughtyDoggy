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
					return float4x4(1, 0, 0, worldPos.x,
								   0, 1, 0, worldPos.y,
								   0, 0, 1, worldPos.z,
								   0, 0, 0, 1);
				}
			#endif
            
            struct appdata
            {
                float4 vertex : POSITION;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            
            v2f vert (appdata v, uint id : SV_INSTANCEID)
            {
                v2f o;
				#if SHADER_TARGET >=45
					v.vertex = mul(TMatrixToWorldPos(ParticleBuffer[id.x].curPosition), v.vertex);
				#endif
                o.vertex = UnityObjectToClipPos(v.vertex);
                return o;
            }
            
            fixed4 frag (v2f i) : SV_Target
            {
                return fixed4(i.vertex.z, i.vertex.z, i.vertex.z, 1);
                // return fixed4(1.0 , 1.0, 1.0, 1);
            }
            ENDCG
        }
    }
}
