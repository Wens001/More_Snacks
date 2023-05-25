Shader "Matcap_Transparent"
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

		_StartYPos("_StartYPos",float) = 0
		_StartColor("_StartColor",color) = (1,1,1,1)
		_EndYPos("_StartYPos",float) = 0
		_EndColor("_EndColor",color) = (1,1,1,1)
	}
 
	SubShader
	{
		Blend SrcAlpha OneMinusSrcAlpha
		Tags
		{
			"Queue" = "Transparent"
			"RenderType" = "Transparent"
			"LightMode" = "ForwardBase"
		}
 
		Pass
		{
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
			fixed _ShadowMul;

			float _StartYPos;
			float _EndYPos;
			float4 _StartColor;
			float4 _EndColor;
 
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
				float4 worldPos : TEXCOORD4;
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

				o.worldPos = mul(UNITY_MATRIX_M, v.vertex);

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

				float4 endCurve = lerp(_StartColor, _EndColor, smoothstep(_StartYPos, _EndYPos, v.worldPos.y));


				fixed4 col = finalColor * _ShadowMul * endCurve * _LightColor0 + shadow * finalColor * (1 - _ShadowMul);
				col.a = _MainColor.a;
				return col ;
			}
			ENDCG
		}




	}
 
	Fallback "VertexLit"
}