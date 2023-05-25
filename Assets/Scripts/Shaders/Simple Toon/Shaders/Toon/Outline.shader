Shader "_MyLibrary/Toon/Outline"
{
	Properties
    {
        _MainTex ("Texture", 2D) = "white" {}

        [Header(Colorize)][Space(5)]  //colorize
		_Color ("Color", COLOR) = (1,1,1,1)

		[HideInInspector] _ColIntense ("Intensity", Range(0,3)) = 1
        [HideInInspector] _ColBright ("Brightness", Range(-1,1)) = 0
		_AmbientCol ("Ambient", Range(0,1)) = 0

        [Header(Detail)][Space(5)]  //detail
        [Toggle] _Segmented ("Segmented", Float) = 1
        _Steps ("Steps", Range(1,25)) = 3
        _StpSmooth ("Smoothness", Range(0,1)) = 0
        _Offset ("Lit Offset", Range(-1,1.1)) = 0

        [Header(Light)][Space(5)]  //light
        [Toggle] _Clipped ("Clipped", Float) = 0
        _MinLight ("Min Light", Range(0,1)) = 0
        _MaxLight ("Max Light", Range(0,1)) = 1
        _Lumin ("Luminocity", Range(0,2)) = 0

		[Header(Outline)][Space(5)]  //outline
		_OtlColor ("Color", COLOR) = (0,0,0,1)
		_OtlWidth ("Width", Range(0,5)) = 1

        [Header(Shine)][Space(5)]  //shine
		[HDR] _ShnColor ("Color", COLOR) = (1,1,0,1)
        [Toggle] _ShnOverlap ("Overlap", Float) = 0

        _ShnIntense ("Intensity", Range(0,1)) = 0
        _ShnRange ("Range", Range(0,1)) = 0.15
        _ShnSmooth ("Smoothness", Range(0,1)) = 0

        [Header(Specular)][Space(5)]  //specular 
        _GlossColor("_GlossColor", Color) = (1,1,1,1)
        _Gloss("Gloss", Range(8,200)) = 50 

        [Header(Rim)][Space(5)]  //Rim 
        _RimColor("RimColor",Color) = (1,1,1,1)
        _RimFactor("RimFactor",Range(0,3.0)) = 0.1
    }

    SubShader
    {
        CGINCLUDE

            #include "UnityCG.cginc"
            UNITY_INSTANCING_BUFFER_START(Props)
                UNITY_DEFINE_INSTANCED_PROP(float4, _Color)
                UNITY_DEFINE_INSTANCED_PROP(float4, _RimColor)
                UNITY_DEFINE_INSTANCED_PROP(float, _RimFactor)
            UNITY_INSTANCING_BUFFER_END(Props)


            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
                float3 normal : NORMAL;
            };
            #include "Core.cginc"
        ENDCG


        Tags { "RenderType" = "Opaque" "LightMode" = "ForwardBase" }
        Pass
        {
            Name "DirectLight"
            LOD 80

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_fwdbase

            struct v2f
            {
                LIGHTING_COORDS(0,1)
                float2 uv : TEXCOORD0;
                float4 pos : SV_POSITION;
                half3 worldNormal : NORMAL;
				float3 viewDir : TEXCOORD2;
                float3 worldVertex : TEXCOORD3;
            };
            half _Gloss;
            half4 _GlossColor;

            v2f vert (appdata v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                o.worldNormal = UnityObjectToWorldNormal(v.normal);
				o.viewDir = WorldSpaceViewDir(v.vertex);
                o.worldVertex = mul(v.vertex, unity_WorldToObject).xyz;
                TRANSFER_VERTEX_TO_FRAGMENT(o);
                return o;
            }

			fixed4 frag (v2f i) : SV_Target
            {
                _MaxLight = max(_MinLight, _MaxLight);
                _Steps = _Segmented ? _Steps : 1;
                _StpSmooth = _Segmented ? _StpSmooth : 1;

				_DarkColor = fixed4(0,0,0,1);
				_MaxAtten = 1.0;

				float3 normal = normalize(i.worldNormal);
				float3 light_dir = normalize(_WorldSpaceLightPos0.xyz);
				float3 view_dir = normalize(i.viewDir);
				float3 halfVec = normalize(light_dir + view_dir);
				float3 forward = mul((float3x3)unity_CameraToWorld, float3(0,0,1));

                float NdotL = dot(normal, light_dir);
				float NdotH = dot(normal, halfVec);
				float VdotN = dot(view_dir, normal);
				float FdotV = dot(forward, -view_dir);

                fixed atten = SHADOW_ATTENUATION(i);
                float toon = Toon(NdotL, atten );

				fixed4 shadecol = _DarkColor;
				fixed4 litcol = ColorBlend(UNITY_ACCESS_INSTANCED_PROP(Props, _Color), _LightColor0, _AmbientCol);
				fixed4 texcol = tex2D(_MainTex, i.uv) * litcol * _ColIntense + _ColBright;

				float4 blendCol = ColorBlend(shadecol, texcol, toon) ;
				float4 postCol = PostEffects(blendCol * _LightColor0, toon, atten, NdotL, NdotH, VdotN, FdotV) ;
                
                //specular
                fixed3 reflectDir = normalize(reflect(-light_dir, normal));
                fixed3 specular = _LightColor0.rgb * pow(max(dot(reflectDir, view_dir), 0), _Gloss);
                postCol.rgb += specular * _GlossColor;
                
                //rim
			    float rim = 1 - max(0, dot(view_dir, normal));
                float4 rimColor = UNITY_ACCESS_INSTANCED_PROP(Props, _RimColor) *
                    UNITY_ACCESS_INSTANCED_PROP(Props, _RimFactor) * rim;
                postCol += rimColor;

                postCol.a = 1;

				return postCol ;
            }

            ENDCG
        }

        Tags { "RenderType" = "Opaque" "LightMode" = "ForwardAdd" }
        Pass
        {
            Name "SpotLight"
            BlendOp Max
            LOD 100
            CGPROGRAM
            #pragma vertex SpotVert
            #pragma fragment SpotFrag
            #pragma multi_compile_fwdadd_fullshadows
            fixed4 SpotFrag(SpotV2f i) : SV_Target
            {
                fixed3 col = CalculatorSpotColor(i);
                return fixed4(col, 1);
            }

            ENDCG
        }

		UsePass "Legacy Shaders/VertexLit/SHADOWCASTER"

		Pass
        {
			Tags { "RenderType" = "Opaque" "LightMode" = "ForwardBase" }
			Blend Off
            Cull Front

            CGPROGRAM
            #pragma vertex vert
 			#pragma fragment frag
			float4 _OtlColor;
            float _OtlWidth;

			struct v2f
			{
				float4 pos : SV_POSITION;
			};

            v2f vert (appdata v)
            {
                v2f o;
			    o.pos = v.vertex;
			    o.pos.xyz += normalize(v.normal.xyz) * _OtlWidth * 0.008;
			    o.pos = UnityObjectToClipPos(o.pos);
			    return o;
            }

            fixed4 frag(v2f i) : SV_Target
			{
				clip(-negz(_OtlWidth));
		    	return _OtlColor;
			}

            ENDCG
        }
    }
}
