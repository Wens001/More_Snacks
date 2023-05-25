Shader "_MyLibrary/Other/Billboard"
{
    Properties
    {
        _Color("_Color",Color) = (1,1,1,1)
        _MainTex ("Texture", 2D) = "white" {}
		[MaterialToggle]_Vertical("Vertical",Range(0,1)) = 1
    }
    SubShader
    {
        Tags { "RenderType"="Transparent" "Queue"="Transparent" "IgnoreProjection"="True" "DisableBacthing"="True" }
        LOD 100

        Pass
        {
			ZWrite Off
			Blend SrcAlpha OneMinusSrcAlpha
			Cull Off

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_fog
            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                UNITY_FOG_COORDS(1)
                float4 vertex : SV_POSITION;
            };

            UNITY_INSTANCING_BUFFER_START(Props)
                UNITY_DEFINE_INSTANCED_PROP(float4, _Color)
            UNITY_INSTANCING_BUFFER_END(Props)

            sampler2D _MainTex;
            float4 _MainTex_ST;
			fixed _Vertical;

            v2f vert (appdata v)
            {
                v2f o;
				float3 center = float3(0, 0, 0);
				float3 view = mul(unity_WorldToObject, float4(_WorldSpaceCameraPos, 1));

				float3 normalDir = view - center;
				normalDir.y = normalDir.y*_Vertical;
				normalDir = normalize(normalDir);

				float3 upDir = abs(normalDir.y) > 0.999 ? float3(0, 0, 1) : float3(0, 1, 0);
				float3 rightDir = normalize(cross(upDir, normalDir));
				upDir = normalize(cross(normalDir, rightDir));
				float3 centerOffset = v.vertex.xyz - center;
				float3 localPos = center + rightDir * centerOffset.x + upDir * centerOffset.y + normalDir * centerOffset.z;

                o.vertex = UnityObjectToClipPos(localPos);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                UNITY_TRANSFER_FOG(o,o.vertex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                fixed4 col = tex2D(_MainTex, i.uv);
                UNITY_APPLY_FOG(i.fogCoord, col);
                return col * UNITY_ACCESS_INSTANCED_PROP(Props, _Color);
            }
            ENDCG
        }
    }
}
