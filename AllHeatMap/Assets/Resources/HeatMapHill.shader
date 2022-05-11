
Shader "HeatMap/Hill"
{
    Properties
    {
        _HeatMapTex("HeatMapTex",2D) = "white"{}
       //_Alpha("Alpha",range(0,1)) = 0.8
    }

    SubShader
    {
        Tags{"Queue"="Transparent"}
        Blend SrcAlpha OneMinusSrcAlpha

        Pass
        {
            CGPROGRAM

            #pragma vertex vert
            #pragma fragment frag
			//#pragma only_renderers d3d9 d3d11 glcore gles gles3 metal
            #include "unitycg.cginc"

            sampler2D _HeatMapTex;
            //half _Alpha;
            uniform int _FactorCount;
			uniform float _Radius;
			uniform float _MaxHeight;
            uniform float4 _Factors[1000];

            struct a2v
			{
				float4 pos : POSITION;
			};

			struct v2f
			{
				float4 pos : POSITION;
				fixed3 worldPos : TEXCOORD1;
			};

			v2f vert(a2v input)
			{
				v2f o;
                half3 worldPos = mul(unity_ObjectToWorld,input.pos).xyz;
				float height = 0.0;
				for( int i = 0 ; i < _FactorCount;i++ )
				{
					float2 tmp = float2(worldPos.x, worldPos.z);
					float2 tmp1 = float2(_Factors[i].x, _Factors[i].z);
					float dis = distance(tmp, tmp1);
					float intensity = float(_Factors[i].w);
					float centerheight = _MaxHeight * intensity;					
					if (dis < _Radius) {
						float releaseDis = 1.0 - (cos(radians(180.0 * (_Radius - dis) / _Radius)) + 1.0) / 2.0;
						float nowheight = centerheight * releaseDis;
						if (nowheight != 0.0) {
							height = height + nowheight / (nowheight + height) * nowheight;
						}
					}
				}
				o.pos = UnityObjectToClipPos(input.pos + half3(0.0, height, 0.0));
				o.worldPos = mul(unity_ObjectToWorld,input.pos).xyz;
				return o;
			}

			half4 frag(v2f input):COLOR
			{
				half heat = 0.0;
				for( int i = 0 ; i < _FactorCount;i++ )
				{
					float2 tmp = float2(input.worldPos.x, input.worldPos.z);
					float2 tmp1 = float2(_Factors[i].x, _Factors[i].z);
					float dis = distance(tmp, tmp1);
					float intensity = float(_Factors[i].w);
					if (dis < _Radius) {
						float releaseDis = 1.0 - (cos(radians(180.0 * (_Radius - dis) / _Radius)) + 1.0) / 2.0;
						float nowheat = intensity * releaseDis;
						if (nowheat != 0.0) {
							heat = heat + nowheat / (nowheat + heat) * nowheat;
						}
					}				
				}
				heat = clamp(heat, 0.01, 0.99);
				half4 color = tex2D(_HeatMapTex,fixed2(heat,0.5));
				//color.a = _Alpha;
				//color.a = heat;
				return color;
			}
            ENDCG
        }
    }
    Fallback "Diffuse"
}