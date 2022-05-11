Shader "MyShader/thermograph3dobject"
{
	//随高度渐变shader, 相同高度颜色一致
	Properties
	{
		_MainTex("Texture", 2D) = "white" {}

		_Color1("_Color1" , Color) = (1,0,0,1.0)
		_Threshold1("_Threshold1", Float) = 0.8
		_Color2("_Color2" , Color) = (1,1,0,0.75)
		_Threshold2("_Threshold2", Float) = 0.6
		_Color3("_Color3" , Color) = (0,0,1,0.5)
		_Threshold3("_Threshold3", Float) = 0.4
		_Color4("_Color4" , Color) = (0,0,1,0.5)
		_Threshold4("_Threshold4", Float) = 0.2

		_PreHeight("_PreHeight", Float) = 10   //预设最高高度
	}
		SubShader
		{
			Tags{ "RenderType" = "Transparent" "Queue" = "Transparent" }

			Blend SrcAlpha OneMinusSrcAlpha

			Pass
			{
				CGPROGRAM
				// Upgrade NOTE: excluded shader from DX11 because it uses wrong array syntax (type[size] name)
				//#pragma exclude_renderers d3d11
				#pragma vertex vert
				#pragma fragment frag
				#pragma multi_compile_instancing

				#include "UnityCG.cginc"


				struct appdata
				{
					float4 vertex : POSITION;
					float2 uv : TEXCOORD0;
					UNITY_VERTEX_INPUT_INSTANCE_ID
				};

				struct v2f
				{
					float2 uv : TEXCOORD0;
					float4 vertex : SV_POSITION;
					float temp : TEXCOORD1;
					UNITY_VERTEX_INPUT_INSTANCE_ID
				};

				sampler2D _MainTex;
				float4 _MainTex_ST;
				float _PreHeight;

				float _Threshold1;
				float _Threshold2;
				float _Threshold3;
				float _Threshold4;
				fixed4 _Color1;
				fixed4 _Color2;
				fixed4 _Color3;
				fixed4 _Color4;

				fixed4 Temp2Color(float temp, fixed4 color)
				{
					fixed4 tempcolor = fixed4(1, 1, 1, 1);
					if (temp <= _Threshold1) {
						tempcolor = _Color1;
					}else if (temp > _Threshold1 && temp <= _Threshold2) {
						tempcolor = lerp(_Color1, _Color2, (temp - _Threshold1) / (_Threshold2 - _Threshold1));
					}
					else if (temp > _Threshold2 && temp <= _Threshold3) {
						tempcolor = lerp(_Color2, _Color3, (temp - _Threshold2) / (_Threshold3 - _Threshold2));
					}
					else if (temp > _Threshold3 && temp <= _Threshold4) {
						tempcolor = lerp(_Color3, _Color4, (temp - _Threshold3) / (_Threshold4 - _Threshold3));
					}
					else {
						tempcolor = _Color4;
					}
					return tempcolor;
				}

				v2f vert(appdata v)
				{
					v2f o;
					UNITY_SETUP_INSTANCE_ID(v);
					UNITY_TRANSFER_INSTANCE_ID(v,o);

					float height = (mul(unity_ObjectToWorld , v.vertex)).y;
					o.temp = height / (_PreHeight*2);
					o.vertex = UnityObjectToClipPos(v.vertex);
					o.uv = TRANSFORM_TEX(v.uv, _MainTex);    //把材质球列纹理贴图和顶点的UV坐标对齐

					return o;
				}

				fixed4 frag(v2f i) : SV_Target
				{
					UNITY_SETUP_INSTANCE_ID(i);

					fixed4 col = tex2D(_MainTex, i.uv);  //获取顶点i坐标映射到纹理图片上的颜色值
					fixed4 color = Temp2Color(i.temp , col);
					return color;
				}
				ENDCG
			}
		}
		//FallBack "Diffuse"
}