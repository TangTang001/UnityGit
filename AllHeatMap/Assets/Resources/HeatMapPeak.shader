
Shader "HeatMap/Peak"
{
	Properties
	{
		_HeatMapTex("HeatMapTex",2D) = "white"{}
	}

	SubShader
	{
		Cull Off
		Tags{ "Queue" = "Transparent" }
		Blend SrcAlpha OneMinusSrcAlpha

		Pass
		{	
			CGPROGRAM

			#pragma vertex vert
			#pragma fragment frag
			#include "unitycg.cginc"

			sampler2D _HeatMapTex;
			half _Alpha;
			uniform int _FactorCount;
			uniform float _MaxHeight;
			uniform float4 _Factors[100];
			//uniform float2 _FactorsProperties[100];

			struct a2v
			{
				float4 pos : POSITION;
			};

			struct v2f
			{
				float4 pos : POSITION;
				fixed3 worldPos : TEXCOORD1;
			};


			float calrcpDisAll(float2 vertex) {
				float rcpDisAll = 0.0;
				for (int i = 0; i < _FactorCount; i++) {
					float2 tmp = float2(vertex.x, vertex.y);
					float2 tmp1 = float2(_Factors[i].x, _Factors[i].z);
					float dis = distance(tmp, tmp1);
					rcpDisAll += pow(rcp(dis), 2);   //rcp(dis) = 1/ dis
				}
				return rcpDisAll;
			}


			float calMulHeat(float2 vertex) {
				float heat = 0.0;
				float rcpDisAll = calrcpDisAll(vertex);
				float ratio = 0.0;
				for (int i = 0; i < _FactorCount; i++) {
					float intensity = _Factors[i].w;
					float2 tmp = float2(vertex.x, vertex.y);
					float2 tmp1 = float2(_Factors[i].x, _Factors[i].z);
					float dis = distance(tmp, tmp1);
					ratio = pow(rcp(dis), 2) / rcpDisAll;
					intensity = clamp(intensity, 0.01, 0.99); //intensity<=0的值取0，intensity>=1的值取1，其他保持不变
					heat = heat +  ratio * intensity;
				}
				return heat;
			}

			v2f vert(a2v input)
			{
				v2f o;
				float3 worldPos = mul(unity_ObjectToWorld, input.pos).xyz;

				float heat = 0.0;
				heat = calMulHeat(worldPos.xz);
				heat = clamp(heat, 0.01, 0.99);
				input.pos.y = _MaxHeight * (heat - 0.5);

				o.pos = UnityObjectToClipPos(input.pos);				
				o.worldPos = mul(unity_ObjectToWorld,input.pos).xyz;
				return o;
			}

			half4 frag(v2f input) :COLOR
			{
				float heat = 0.0;
				heat = calMulHeat(input.worldPos.xz);
				heat = clamp(heat, 0.01, 0.99);
				half4 color = tex2D(_HeatMapTex, fixed2(heat, 1));
				//color.a = 0.8;
				return color;
			}

			ENDCG
		}
	}

	Fallback "Diffuse"
}