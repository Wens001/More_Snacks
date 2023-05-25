Shader "_MyLibrary/Toon/Transparent_Dithering"
{
	Properties
    {
        _MainTex("Texture", 2D) = "white" {}

        [Header(Colorize)][Space(5)]  //colorize
        _Color("Color", COLOR) = (1,1,1,1)

        [HideInInspector] _ColIntense("Intensity", Range(0,3)) = 1
        [HideInInspector] _ColBright("Brightness", Range(-1,1)) = 0
        _AmbientCol("Ambient", Range(0,1)) = 0

        [Header(Detail)][Space(5)]  //detail
        [Toggle] _Segmented("Segmented", Float) = 1
        _Steps("Steps", Range(1,25)) = 3
        _StpSmooth("Smoothness", Range(0,1)) = 0
        _Offset("Lit Offset", Range(-1,1.1)) = 0

        [Header(Light)][Space(5)]  //light
        [Toggle] _Clipped("Clipped", Float) = 0
        _MinLight("Min Light", Range(0,1)) = 0
        _MaxLight("Max Light", Range(0,1)) = 1
        _Lumin("Luminocity", Range(0,2)) = 0

        [Header(Shine)][Space(5)]  //shine
        [HDR] _ShnColor("Color", COLOR) = (1,1,0,1)
        [Toggle] _ShnOverlap("Overlap", Float) = 0

        _ShnIntense("Intensity", Range(0,1)) = 0
        _ShnRange("Range", Range(0,1)) = 0.15
        _ShnSmooth("Smoothness", Range(0,1)) = 0

        _StartDitheringFade("Start Dithering Fade", Float) = 0
        _EndDitheringFade("End Dithering Fade", Float) = 1
    }

    SubShader
    {

        CGINCLUDE
            #include "UnityCG.cginc"
            UNITY_INSTANCING_BUFFER_START(Props)
                UNITY_DEFINE_INSTANCED_PROP(float4, _Color)
            UNITY_INSTANCING_BUFFER_END(Props)

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
                float3 normal : NORMAL;
            };
            #include "Core.cginc"
        ENDCG
        Tags { "Queue" = "Transparent" "RenderType" = "Transparent" }
        Blend SrcAlpha OneMinusSrcAlpha
        Pass  //full transparency
        {
            LOD 300
            ColorMask 0
        }

        Pass
        {
            Tags { "LightMode" = "ForwardBase" }
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
                fixed eyeDepth : TEXCOORD3;
                fixed4 screenPosition : TEXCOORD4;
            };

            v2f vert (appdata v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                o.worldNormal = UnityObjectToWorldNormal(v.normal);
				o.viewDir = WorldSpaceViewDir(v.vertex);
                o.eyeDepth = -UnityObjectToViewPos(v.vertex.xyz).z;
                o.screenPosition = ComputeScreenPos(UnityObjectToClipPos(v.vertex));
                TRANSFER_VERTEX_TO_FRAGMENT(o);
                return o;
            }

            float _StartDitheringFade;
            float _EndDitheringFade;

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
                float toon = Toon(NdotL, atten);

				fixed4 shadecol = _DarkColor;
				fixed4 litcol = ColorBlend(_Color, _LightColor0, _AmbientCol);
				fixed4 texcol = tex2D(_MainTex, i.uv) * litcol * _ColIntense + _ColBright;

				float4 blendCol = ColorBlend(shadecol, texcol, toon);
				float4 postCol = PostEffects(blendCol * _LightColor0, toon, atten, NdotL, NdotH, VdotN, FdotV) ;

                float fade = (_StartDitheringFade + _ProjectionParams.y);
                float4 screenPos = i.screenPosition;
                float4 screenPosNorm = screenPos / screenPos.w;
                screenPosNorm.z = (UNITY_NEAR_CLIP_VALUE >= 0) ? screenPosNorm.z : screenPosNorm.z * 0.5 + 0.5;
                float2 clipScreen26 = screenPosNorm.xy * _ScreenParams.xy;

                float end = (i.eyeDepth - fade) / (_EndDitheringFade - fade) ;
				postCol.a = UNITY_ACCESS_INSTANCED_PROP(Props, _Color).a * saturate(end);
				return postCol;
            }
            ENDCG
        }

        Pass
        {
            Tags { "LightMode" = "ForwardAdd" }
            LOD 100
            CGPROGRAM
            #pragma vertex SpotVert
            #pragma fragment SpotFrag
            #pragma multi_compile_fwdadd_fullshadows
            fixed4 SpotFrag(SpotV2f i) : SV_Target
            {
                fixed3 col = CalculatorSpotColor(i);
                return fixed4(col, UNITY_ACCESS_INSTANCED_PROP(Props, _Color).a);
            }
            ENDCG
        }
        UsePass "Legacy Shaders/VertexLit/SHADOWCASTER"
    }
}
