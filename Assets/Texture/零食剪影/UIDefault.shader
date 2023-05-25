//默认UI模板
Shader "_MyShader/UI/Default"     
{     
    Properties     
    {     
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}     
        _Color ("Tint", Color) = (1,1,1,1)
        _StencilComp ("Stencil Comparison", Float) = 8
        _Stencil ("Stencil ID", Float) = 0
        _StencilOp ("Stencil Operation", Float) = 0
        _StencilWriteMask ("Stencil Write Mask", Float) = 255
        _StencilReadMask ("Stencil Read Mask", Float) = 255
        _ColorMask ("Color Mask", Float) = 15
        _Curve("_Curve",range(0,1)) = .5
        _MaskColor("_MaskColor",color) = (1,1,1,1)
    }     

    SubShader     
    {     
        Tags     
        {      
            "Queue"="Transparent"      
            "IgnoreProjector"="True"      
            "RenderType"="Transparent"      
            "PreviewType"="Plane"     
            "CanUseSpriteAtlas"="True"     
        }
		
		Stencil
        {
            Ref [_Stencil]
            Comp [_StencilComp]
            Pass [_StencilOp] 
            ReadMask [_StencilReadMask]
            WriteMask [_StencilWriteMask]
        }
		
        Blend SrcAlpha OneMinusSrcAlpha  
		Lighting Off
		ZWrite Off
        ZTest [unity_GUIZTestMode]
        ColorMask [_ColorMask]
        
        Pass     
        {     
            CGPROGRAM     
            #pragma vertex vert     
            #pragma fragment frag     
            #include "UnityCG.cginc"     

            struct appdata_t     
            {     
                float4 vertex   : POSITION;     
                float2 texcoord : TEXCOORD0;     
            };     

            struct v2f     
            {     
                float4 vertex   : SV_POSITION;     
                half2 texcoord  : TEXCOORD0;     
            };     

            sampler2D _MainTex;
            fixed4 _Color;
            float _Curve;
            float4 _MaskColor;

            v2f vert(appdata_t IN)     
            {     
                v2f OUT;     
                OUT.vertex = UnityObjectToClipPos(IN.vertex);
                OUT.texcoord = IN.texcoord;
                return OUT;  
            }

            float4 frag(v2f IN) : SV_Target
            {
                float4 col = tex2D(_MainTex, IN.texcoord);
                if (IN.texcoord.x < _Curve)
                    return _MaskColor * col.a;
                return col;
            }
            ENDCG     
        }     
    }     
}