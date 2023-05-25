Shader "_MyLibrary/Other/UV_View"{
    Properties{
        _MainTex ("_MainTex", 2D) = "white" {}
        _Scale ("_Scale", Range(0,3)) = 1
        _Lerp ("_Lerp", Range(0,1)) = 0
    }
    SubShader{
        Tags { "RenderType"="Opaque" }
        
        CGPROGRAM
        #pragma surface surf Standard vertex:vert 
        struct Input{
            float2 uv_MainTex;
        };
        sampler2D _MainTex;
        float _Scale,_Lerp;
        void vert(inout appdata_full v, out Input o) {      
            float2 uvNew = ( v.texcoord.xy * 2 - 1 ) * _Scale ;
            float3 pos = float3(uvNew.x,0,uvNew.y);
            v.vertex.xyz = lerp(v.vertex.xyz ,pos , _Lerp);
            UNITY_INITIALIZE_OUTPUT(Input, o);
        }
        void surf (Input IN, inout SurfaceOutputStandard o){
            fixed4 c = tex2D (_MainTex, IN.uv_MainTex);
            o.Albedo = c.rgb;
        }
        ENDCG
    }
}
