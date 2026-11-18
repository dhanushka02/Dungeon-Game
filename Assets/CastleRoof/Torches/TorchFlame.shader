Shader "Castle/Torch Flame"
{
    SubShader
    {
        Tags { "RenderPipeline"="UniversalPipeline" "RenderType"="Opaque" }
        Pass
        {
            Cull Off
            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag
            #pragma multi_compile_fog
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            struct Input { float3 pos : POSITION; half4 color : COLOR; };
            struct Varyings { float4 pos : SV_POSITION; half3 color : TEXCOORD0; half fog : TEXCOORD1; };
            Varyings Vert(Input input)
            {
                float3 p = input.pos;
                float3 anchor = TransformObjectToWorld(float3(0,0,0));
                float phase = _Time.y * 4.8 + dot(anchor.xz, float2(1.7,2.3));
                float tip = saturate(p.y / 0.32);
                p.x += tip * tip * (0.027 * sin(phase) + 0.009 * sin(phase * 2.3));
                p.y += tip * 0.021 * sin(phase * 1.3);
                Varyings o;
                o.pos = TransformObjectToHClip(p);
                o.color = input.color.rgb;
                o.fog = ComputeFogFactor(o.pos.z);
                return o;
            }
            half4 Frag(Varyings input) : SV_Target
            {
                return half4(MixFog(input.color, input.fog), 1);
            }
            ENDHLSL
        }
    }
}
