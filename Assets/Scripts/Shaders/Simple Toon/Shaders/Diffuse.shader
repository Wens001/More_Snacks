﻿Shader "_MyLibrary/Diffuse"
{
	Properties
	{
		_Color("_Color",Color) = (1,1,1,1)
		_MainTex("_MainTex",2D) = "white"{}
	}
		SubShader
		{
			Tags { "RenderType" = "Opaque" }
			LOD 100

			Pass
			{
				CGPROGRAM
				#pragma vertex vert
				#pragma fragment frag
				#pragma multi_compile_instancing
				#pragma multi_compile_fwdbase

				#include "UnityCG.cginc"
				#include "Lighting.cginc"
				#include "AutoLight.cginc"

				sampler2D _MainTex;

				struct appdata
				{
					float4 vertex : POSITION;
					float2 uv : TEXCOORD0;
					UNITY_VERTEX_INPUT_INSTANCE_ID
				};

				struct v2f
				{
					float2 uv : TEXCOORD0;
					UNITY_VERTEX_INPUT_INSTANCE_ID
					float4 pos : SV_POSITION;
					SHADOW_COORDS(3)
				};

				UNITY_INSTANCING_BUFFER_START(Props)
				UNITY_DEFINE_INSTANCED_PROP(float4, _Color)
				UNITY_INSTANCING_BUFFER_END(Props)

				v2f vert(appdata v)
				{
					v2f o;
					UNITY_SETUP_INSTANCE_ID(v);
					UNITY_TRANSFER_INSTANCE_ID(v, o);
					o.pos = UnityObjectToClipPos(v.vertex);
					o.uv = v.uv;
					TRANSFER_SHADOW(o);
					return o;
				}

				float4 frag(v2f i) : SV_Target
				{
					UNITY_SETUP_INSTANCE_ID(i);
					float4 mainTex = tex2D(_MainTex, i.uv);
					return mainTex * UNITY_ACCESS_INSTANCED_PROP(Props, _Color);
				}
				ENDCG
			}
			UsePass "VertexLit/SHADOWCASTER"
		}
		fallback "Diffuse"
}