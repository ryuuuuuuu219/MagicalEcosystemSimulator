Shader "MagicalEcosystem/Experiment/SpaceWarp"
{
    Properties
    {
        _WarpPower ("Distance Power", Range(1, 6)) = 3
        _DistortionStrength ("Distortion Strength", Range(0, 1)) = 1
        _BaseAlpha ("Base Alpha", Range(0, 1)) = 1
        _ObjectRadius ("Object Radius", Float) = 1
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

        Cull Back
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
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareOpaqueTexture.hlsl"

            CBUFFER_START(UnityPerMaterial)
                half _WarpPower;
                half _DistortionStrength;
                half _BaseAlpha;
                half _ObjectRadius;
            CBUFFER_END

            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS : NORMAL;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float3 positionWS : TEXCOORD0;
                float3 normalWS : TEXCOORD1;
            };

            float2 ClipToScreenUV(float4 positionCS)
            {
                float2 ndc = positionCS.xy / max(positionCS.w, 0.0001);
                ndc.y *= _ProjectionParams.x;
                return ndc * 0.5 + 0.5;
            }

            Varyings vert(Attributes input)
            {
                Varyings output;
                VertexPositionInputs positionInputs = GetVertexPositionInputs(input.positionOS.xyz);
                VertexNormalInputs normalInputs = GetVertexNormalInputs(input.normalOS);

                output.positionCS = positionInputs.positionCS;
                output.positionWS = positionInputs.positionWS;
                output.normalWS = normalInputs.normalWS;
                return output;
            }

            half4 frag(Varyings input) : SV_Target
            {
                float2 uv = GetNormalizedScreenSpaceUV(input.positionCS);

                float4 centerCS = TransformObjectToHClip(float3(0.0, 0.0, 0.0));
                float2 centerUV = ClipToScreenUV(centerCS);

                float3 centerWS = TransformObjectToWorld(float3(0.0, 0.0, 0.0));
                float3 cameraRightWS = normalize(float3(UNITY_MATRIX_I_V._m00, UNITY_MATRIX_I_V._m10, UNITY_MATRIX_I_V._m20));
                float3 cameraUpWS = normalize(float3(UNITY_MATRIX_I_V._m01, UNITY_MATRIX_I_V._m11, UNITY_MATRIX_I_V._m21));
                float objectRadius = max(_ObjectRadius, 0.001);

                float2 rightRadiusUV = ClipToScreenUV(TransformWorldToHClip(centerWS + cameraRightWS * objectRadius)) - centerUV;
                float2 upRadiusUV = ClipToScreenUV(TransformWorldToHClip(centerWS + cameraUpWS * objectRadius)) - centerUV;

                float3 fromCenterWS = input.positionWS - centerWS;
                float2 objectPlane = float2(dot(fromCenterWS, cameraRightWS), dot(fromCenterWS, cameraUpWS)) / objectRadius;
                float r = saturate(length(objectPlane));
                float2 dirN = objectPlane / max(length(objectPlane), 0.0001);

                float outerR = pow(r, 1.0 / max(_WarpPower, 0.0001));
                float sampleR = lerp(r, outerR, _DistortionStrength);
                float2 samplePlane = dirN * sampleR;
                float2 sampleUV = saturate(centerUV + rightRadiusUV * samplePlane.x + upRadiusUV * samplePlane.y);

                half3 bg = SampleSceneColor(sampleUV);
                return half4(bg, _BaseAlpha);
            }
            ENDHLSL
        }
    }

    FallBack Off
}
