Shader "_MyLibrary/Toon/Default"
{
    Properties
    {
        _MainTex("_MainTex", 2D) = "white" {}
        //_BumpTex ("_BumpTex", 2D) = "bump" {}
        //_BumpScale("_BumpScale", Range(0, 1)) = 1

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

                #include "Core.cginc"
                //sampler2D _BumpTex;
                //float4 _BumpTex_TexelSize;
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
                float3 worldNormal : NORMAL;
				float3 viewDir : TEXCOORD2;
                float3 worldVertex : TEXCOORD3;
            };
            half _Gloss;
            half4 _GlossColor;

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
                float3 normal : NORMAL;
            };

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

            float3 CalculateNormal(in sampler2D _DepthMap,float4 _DepthMap_TexelSize,float2 uv ,float _Scale = 1)
            {
                float2 du = float2(_DepthMap_TexelSize.x * 0.5, 0);
                float u1 = tex2D(_DepthMap, uv - du);
                float u2 = tex2D(_DepthMap, uv + du);
                float3 tu = float3(1, 0, (u2 - u1) * _Scale);

                float2 dv = float2(0, _DepthMap_TexelSize.y * 0.5);
                float v1 = tex2D(_DepthMap, uv - dv);
                float v2 = tex2D(_DepthMap, uv + dv);
                float3 tv = float3(0, 1, (v2 - v1) * _Scale);

                return normalize(-cross(tv, tu)); 
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
                float toon = Toon(NdotL, atten);
                fixed4 color = UNITY_ACCESS_INSTANCED_PROP(Props, _Color);
				fixed4 shadecol = _DarkColor;
				fixed4 litcol = ColorBlend(color, _LightColor0, _AmbientCol);
				fixed4 texcol = tex2D(_MainTex, i.uv) * litcol * _ColIntense + _ColBright;

				float4 blendCol = ColorBlend(shadecol, texcol, toon);
				float4 postCol = PostEffects(blendCol * _LightColor0, toon, atten, NdotL, NdotH, VdotN, FdotV) ;

                //specular
                fixed3 reflectDir = normalize(reflect(-light_dir, normal));
                fixed3 specular = _LightColor0.rgb * pow(max(dot(reflectDir, view_dir), 0), _Gloss);
                postCol.rgb += specular * _GlossColor;
                
                //rim
			    half rim = 1 - max(0, dot(view_dir, normal));
			    float4 rimColor = UNITY_ACCESS_INSTANCED_PROP(Props, _RimColor) * 
                    UNITY_ACCESS_INSTANCED_PROP(Props, _RimFactor) * rim * _LightColor0;
                postCol += rimColor;

				postCol.a = 1.;
				return  postCol ;
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
    }
}
