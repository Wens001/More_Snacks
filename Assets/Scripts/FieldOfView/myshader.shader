Shader "Unlit/myshader"
{
    Properties
    {
        _Color("_Color",color) = (1,1,1,1)
        _MainTex ("Texture", 2D) = "white" {}
    }
    SubShader
    {
        Blend SrcAlpha OneMinusSrcAlpha
        Tags { "RenderType"="Transparent" "RenderQueue" = "Transparent" }
        
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
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

            sampler2D _MainTex;
            float4 _MainTex_ST;
            fixed4 _Color;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                return o;
            }

            float distance(float2 a, float2 b) {
                return (a.x - b.x) * (a.x - b.x) + (a.y - b.y) * (a.y - b.y);
            }

            fixed4 frag(v2f i) : SV_Target
            {
                float dis = distance(i.uv,float2(.5,.5));
                dis = saturate(.3 - saturate(dis)) ;
                fixed4 col = _Color * tex2D(_MainTex, i.uv);
                col.a *= dis;
                return col;
            }
            ENDCG
        }
    }
}
