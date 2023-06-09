﻿//X-Ray效果
Shader "_MyShader/XRayEffect"
{
	Properties
	{
		_MainTex("Base 2D", 2D) = "white"{}
		_XRayColor("XRay Color", Color) = (1,1,1,1)
	}
	SubShader
	{
		//渲染X光效果的Pass
		Pass
		{
			Tags{ "Queue" = "Geometry+1" "RenderType" = "Opaque" }
			Blend SrcAlpha One
			ZWrite Off
			ZTest Greater

			CGPROGRAM
			#include "Lighting.cginc"
			fixed4 _XRayColor;
			struct v2f
			{
				float4 pos : SV_POSITION;
				float3 normal : normal;
				float3 viewDir : TEXCOORD0;
			};

			v2f vert(appdata_base v)
			{
				v2f o;
				o.pos = UnityObjectToClipPos(v.vertex);
				o.viewDir = ObjSpaceViewDir(v.vertex);
				o.normal = v.normal;
				return o;
			}

			fixed4 frag(v2f i) : SV_Target
			{
				float3 normal = normalize(i.normal);
				float3 viewDir = normalize(i.viewDir);
				float rim = 1 - dot(normal, viewDir);
				return _XRayColor * rim;
			}
			#pragma vertex vert
			#pragma fragment frag
			ENDCG
		}

		//正常渲染的Pass
		Pass
		{
			Tags{ "Queue" = "Geometry+2" "RenderType" = "Opaque" }
			ZWrite On
			CGPROGRAM
			#include "Lighting.cginc"
			sampler2D _MainTex;
			float4 _MainTex_ST;

			struct v2f
			{
				float4 pos : SV_POSITION;
				float2 uv : TEXCOORD0;
			};

			v2f vert(appdata_base v)
			{
				v2f o;
				o.pos = UnityObjectToClipPos(v.vertex);
				o.uv = TRANSFORM_TEX(v.texcoord, _MainTex);
				return o;
			}

			fixed4 frag(v2f i) : SV_Target
			{
				return tex2D(_MainTex, i.uv);
			}

			#pragma vertex vert
			#pragma fragment frag	
			ENDCG
		}
	}
	FallBack "Diffuse"
}