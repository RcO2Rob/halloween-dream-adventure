Shader "LostDream/TitleBackdrop"
{
    Properties { _MainTex ("Cover", 2D) = "white" {} }
    SubShader
    {
        Tags { "Queue"="Overlay" "IgnoreProjector"="True" }
        Cull Off ZWrite Off ZTest Always
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"
            struct Input { float4 position : POSITION; float2 uv : TEXCOORD0; };
            struct Output { float4 position : SV_POSITION; float2 uv : TEXCOORD0; };
            sampler2D _MainTex;
            Output vert(Input input)
            {
                Output output;
                output.position = UnityObjectToClipPos(input.position);
                output.uv = input.uv;
                return output;
            }
            float4 frag(Output input) : SV_Target { return tex2D(_MainTex, input.uv); }
            ENDCG
        }
    }
}
