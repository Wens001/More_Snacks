Shader "Kaima/PostProcessing/LensCircle"
{
	Properties
	{
		_MainTex ("Texture", 2D) = "white" {}
		_ColorMul("_ColorMul",range(-1,1)) = .1
	}
	SubShader
	{
		Tags { "RenderType"="Opaque" }

		Pass
		{
			ZTest Always
			Cull Off
			ZWrite Off

			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			
			#include "UnityCG.cginc"

			struct v2f
			{
				float2 uv : TEXCOORD0;
				float4 vertex : SV_POSITION;
			};

			sampler2D _MainTex;
			half4 _MainTex_TexelSize;
			float _ColorMul;
			
			v2f vert (appdata_img v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv = v.texcoord;
				return o;
			}

			fixed4 frag (v2f i) : SV_Target
			{
				fixed4 col = tex2D(_MainTex, i.uv);
				if (i.uv.y < .5)
					col = col * (_ColorMul + smoothstep(0, 1, i.uv.y) * (1- _ColorMul));
				else
					col = col * (_ColorMul + smoothstep(0, 1, 1 - i.uv.y ) * (1 - _ColorMul));
				return col;
			}
			ENDCG
		}
	}
}
