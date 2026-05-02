Shader "MagicalEcosystem/Experiment/MagicParticleBillboard"
{
    Properties
    {
        _BaseColor ("Base Color", Color) = (1, 1, 1, 1)
        _CoreStrength ("Core Strength", Range(0, 3)) = 1.45
        _EdgeSoftness ("Edge Softness", Range(0.01, 1)) = 0.5
    }

    SubShader
    {
        Tags
        {
            "Queue" = "Transparent"
            "RenderType" = "Transparent"
            "RenderPipeline" = "UniversalPipeline"
            "IgnoreProjector" = "True"
        }

        Cull Off
        ZWrite Off
        ZTest LEqual
        Blend SrcAlpha OneMinusSrcAlpha

        Pass
        {
            Name "Forward"
            Tags { "LightMode" = "UniversalForward" }

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            CBUFFER_START(UnityPerMaterial)
                half4 _BaseColor;
                half _CoreStrength;
                half _EdgeSoftness;
            CBUFFER_END

            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv : TEXCOORD0;
                half4 color : COLOR;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float2 uv : TEXCOORD0;
                half4 color : COLOR;
            };

            Varyings vert(Attributes input)
            {
                Varyings output;
                output.positionCS = TransformObjectToHClip(input.positionOS.xyz);
                output.uv = input.uv;
                output.color = input.color;
                return output;
            }

            half4 frag(Varyings input) : SV_Target
            {
                float2 centeredUv = input.uv * 2.0 - 1.0;
                half r = (half)length(centeredUv);
                half alpha = 1.0h - smoothstep(1.0h - _EdgeSoftness, 1.0h, r);
                half core = 1.0h - smoothstep(0.0h, 0.65h, r);
                half shade = saturate(0.45h + core * _CoreStrength);
                half3 color = _BaseColor.rgb * input.color.rgb * shade;
                return half4(color, alpha * _BaseColor.a * input.color.a);
            }
            ENDHLSL
        }
    }

    FallBack Off
}
