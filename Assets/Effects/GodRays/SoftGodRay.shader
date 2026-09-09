Shader "Dungeon/Soft God Ray"
{
    Properties
    {
        _Color ("Beam Color", Color) = (1.0, 0.78, 0.42, 1.0)
        _Intensity ("Brightness", Range(0, 3)) = 0.9
        _Opacity ("Density", Range(0, 0.5)) = 0.12
        _EdgePower ("Edge Softness", Range(0.5, 8)) = 2.8
        _TopFade ("Top Fade", Range(0.01, 0.5)) = 0.12
        _BottomFade ("Bottom Fade", Range(0.01, 0.6)) = 0.28
        _NoiseScale ("Density Scale", Range(0.1, 8)) = 1.7
        _NoiseStrength ("Density Variation", Range(0, 1)) = 0.32
        _NoiseSpeed ("Density Motion", Range(0, 2)) = 0.18
    }

    SubShader
    {
        Tags
        {
            "Queue" = "Transparent+10"
            "RenderType" = "Transparent"
            "IgnoreProjector" = "True"
        }

        Blend SrcAlpha One
        ZWrite Off
        Cull Off
        Lighting Off

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float3 normal : NORMAL;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float4 position : SV_POSITION;
                float3 worldPosition : TEXCOORD0;
                float3 worldNormal : TEXCOORD1;
                float2 uv : TEXCOORD2;
            };

            fixed4 _Color;
            float _Intensity;
            float _Opacity;
            float _EdgePower;
            float _TopFade;
            float _BottomFade;
            float _NoiseScale;
            float _NoiseStrength;
            float _NoiseSpeed;

            v2f vert(appdata input)
            {
                v2f output;
                output.position = UnityObjectToClipPos(input.vertex);
                output.worldPosition = mul(unity_ObjectToWorld, input.vertex).xyz;
                output.worldNormal = UnityObjectToWorldNormal(input.normal);
                output.uv = input.uv;
                return output;
            }

            fixed4 frag(v2f input) : SV_Target
            {
                float3 viewDirection = normalize(_WorldSpaceCameraPos.xyz - input.worldPosition);
                float facing = pow(saturate(abs(dot(normalize(input.worldNormal), viewDirection))), _EdgePower);
                float topFade = smoothstep(0.0, _TopFade, input.uv.y);
                float bottomFade = 1.0 - smoothstep(1.0 - _BottomFade, 1.0, input.uv.y);

                float3 samplePosition = input.worldPosition * _NoiseScale;
                float firstWave = sin(samplePosition.x * 1.31 + samplePosition.y * 0.47 + _Time.y * _NoiseSpeed);
                float secondWave = sin(samplePosition.z * 1.73 - samplePosition.y * 0.29 - _Time.y * _NoiseSpeed * 0.73);
                float noise = saturate(0.5 + 0.25 * firstWave + 0.25 * secondWave);
                float density = lerp(1.0 - _NoiseStrength, 1.0, noise);
                float alpha = _Opacity * facing * topFade * bottomFade * density;
                float3 color = _Color.rgb * _Intensity * density;

                return fixed4(color, alpha);
            }
            ENDCG
        }
    }

    Fallback Off
}
