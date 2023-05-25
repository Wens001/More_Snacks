﻿Shader "Matcap_XRay"
{
	Properties
	{
		_MainColor("Main Color", Color) = (1.0, 1.0, 1.0, 1.0)
		_DetailColor("Detail Color", Color) = (1.0, 1.0, 1.0, 1.0)
		_DetailTex("Detail Textrue", 2D) = "white" {}
		_DetailTexDepthOffset("Detail Textrue Depth Offset", Float) = 1.0
		_DiffuseColor("Diffuse Color", Color) = (1, 1, 1, 1)
		_DiffuseTex("Diffuse Textrue", 2D) = "white" {}
		_MatCap("MatCap", 2D) = "white" {}
		_ReflectionColor("Reflection Color", Color) = (0, 0, 0, 1.0)
		_ReflectionMap("Reflection Cube Map", Cube) = "" {}
		_ReflectionStrength("Reflection Strength", Range(0.0, 1.0)) = .4
		_ShadowMul("_ShadowMul",Range(0,1)) = .85
		//_XRayColor("XRay Color", Color) = (1,1,1,1)
		_Specular("Specular Color",Color) = (1,1,1,1)//高光颜色
		_Gloss("Gloss",Range(8,200)) = 10 //发光强度
	}
 
	SubShader
	{
		Tags
		{
			"Queue" = "Geometry"
			"LightMode" = "ForwardBase"
			"RenderType" = "Opaque"
		}
 
			//渲染X光效果的Pass
		Pass
		{
			Tags{ "Queue" = "Geometry+10" "RenderType" = "Opaque" }
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
				o.viewDir = WorldSpaceViewDir(v.vertex);
				o.normal = normalize(mul(v.normal, (float3x3) unity_WorldToObject));//世界坐标法线
				return o;
			}

			fixed4 frag(v2f i) : SV_Target
			{
				float3 normal = normalize(i.normal);
				float3 viewDir = normalize(i.viewDir);
				float rim = 1 - max(0, dot(normal, viewDir));
				return _XRayColor * rim;
			}
			#pragma vertex vert
			#pragma fragment frag
			ENDCG
		}



		Pass
		{
			Blend Off
			ZWrite On
			CGPROGRAM
			#include "UnityCG.cginc"
			#include "Lighting.cginc"
			#include "AutoLight.cginc"
			#pragma fragment frag
			#pragma vertex vert
			#pragma multi_compile_fwdbase
			#pragma multi_compile_instancing

			float4 _MainColor;
			float4 _DetailColor;
			sampler2D _DetailTex;
			float4 _DetailTex_ST;
			float _DetailTexDepthOffset;
			float4 _DiffuseColor;
			sampler2D _DiffuseTex;
			float4 _DiffuseTex_ST;
			sampler2D _MatCap;
			float4 _ReflectionColor;
			samplerCUBE _ReflectionMap;
			float _ReflectionStrength;
			float _Gloss;
			fixed _ShadowMul;
			float4 _Specular;
 
			struct VertexInput
			{
				float3 normal : NORMAL;
				float4 vertex : POSITION;
				float2 UVCoordsChannel1: TEXCOORD0;
				UNITY_VERTEX_INPUT_INSTANCE_ID
			};
 
			struct VertexToFragment
			{
				float3 detailUVCoordsAndDepth : TEXCOORD0;
				float4 diffuseUVAndMatCapCoords : TEXCOORD1;
				float4 position : SV_POSITION;
				float3 worldSpaceReflectionVector : TEXCOORD2;
				SHADOW_COORDS(3)
				float3 normal : NORMAL;
				float3 viewDir : TEXCOORD4;
				UNITY_VERTEX_INPUT_INSTANCE_ID
			};
 
			VertexToFragment vert(VertexInput v)
			{
				VertexToFragment o;
				UNITY_SETUP_INSTANCE_ID(v);
				UNITY_TRANSFER_INSTANCE_ID(v, o);
				
				o.diffuseUVAndMatCapCoords.xy = TRANSFORM_TEX(v.UVCoordsChannel1, _DiffuseTex);
				o.diffuseUVAndMatCapCoords.z = dot(normalize(UNITY_MATRIX_IT_MV[0].xyz), normalize(v.normal));
				o.diffuseUVAndMatCapCoords.w = dot(normalize(UNITY_MATRIX_IT_MV[1].xyz), normalize(v.normal));
				o.diffuseUVAndMatCapCoords.zw = o.diffuseUVAndMatCapCoords.zw * 0.5 + 0.5;
				o.position = UnityObjectToClipPos(v.vertex);

				o.normal = normalize(mul(v.normal, (float3x3) unity_WorldToObject));//世界坐标法线
				o.viewDir = normalize(_WorldSpaceCameraPos.xyz - mul(v.vertex, unity_WorldToObject).xyz);//顶点朝着相机看的方向

				o.detailUVCoordsAndDepth.xy = TRANSFORM_TEX(v.UVCoordsChannel1, _DetailTex);
				o.detailUVCoordsAndDepth.z = o.position.z;
				float3 worldSpacePosition = mul(unity_ObjectToWorld, v.vertex).xyz;
				float3 worldSpaceNormal = normalize(mul((float3x3)unity_ObjectToWorld, v.normal));
				o.worldSpaceReflectionVector = reflect(worldSpacePosition - _WorldSpaceCameraPos.xyz, worldSpaceNormal);
				TRANSFER_SHADOW(o)
				return o;
			}
 
			float4 frag(VertexToFragment v) : COLOR
			{
				float3 reflectionColor = texCUBE(_ReflectionMap, v.worldSpaceReflectionVector).rgb * _ReflectionColor.rgb;
				float4 diffuseColor = tex2D(_DiffuseTex, v.diffuseUVAndMatCapCoords.xy) * _DiffuseColor;
				float3 mainColor = lerp(lerp(_MainColor.rgb, diffuseColor.rgb, diffuseColor.a), reflectionColor, _ReflectionStrength);
				float3 detailMask = tex2D(_DetailTex, v.detailUVCoordsAndDepth.xy).rgb;
				float3 detailColor = lerp(_DetailColor.rgb, mainColor, detailMask);
				mainColor = lerp(detailColor, mainColor, saturate(v.detailUVCoordsAndDepth.z * _DetailTexDepthOffset));
				float3 matCapColor = tex2D(_MatCap, v.diffuseUVAndMatCapCoords.zw).rgb;
				float4 finalColor=float4(mainColor * matCapColor * 2.0, diffuseColor.a);

				fixed shadow = SHADOW_ATTENUATION(v);


				fixed3 lightDir = normalize(_WorldSpaceLightPos0.xyz);//世界坐标光方向
				fixed3 reflectDir = normalize(reflect(-lightDir, v.normal));
				float specular = _LightColor0.rgb * _Specular * pow(max(dot(reflectDir, v.viewDir), 0), _Gloss);

				return finalColor * _ShadowMul + shadow * finalColor * (1 - _ShadowMul) + specular;
			}
			ENDCG
		}
	}
 
	Fallback "VertexLit"
}