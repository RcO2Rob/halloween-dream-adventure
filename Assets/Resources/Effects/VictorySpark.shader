Shader "LostDream/VictorySpark"
{
    SubShader
    {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" "IgnoreProjector"="True" }
        Blend SrcAlpha One
        Cull Off ZWrite Off
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"
            struct Input { float4 position : POSITION; float4 color : COLOR; float2 uv : TEXCOORD0; };
            struct Output { float4 position : SV_POSITION; float4 color : COLOR; float2 uv : TEXCOORD0; };
            Output vert(Input input)
            {
                Output output;
                output.position = UnityObjectToClipPos(input.position);
                output.color = input.color; output.uv = input.uv;
                return output;
            }
            float4 frag(Output input) : SV_Target
            {
                float2 offset = input.uv * 2 - 1;
                float glow = saturate(1 - dot(offset, offset));
                return float4(input.color.rgb * 3.2, input.color.a * glow);
            }
            ENDCG
        }
    }
}
