Shader "MagicalEcosystem/Lab/SpaceWarpCustomMvp"
{
    Properties
    {
        _WarpPower ("Distance Power", Range(1, 6)) = 1
        _DistortionStrength ("Distortion Strength", Range(0, 1)) = 1
        _BaseAlpha ("Base Alpha", Range(0, 1)) = 1
        _ObjectRadius ("Object Radius", Float) = 1
        _GlitchPointUV ("Glitch Point UV", Vector) = (-1, -1, 0, 0)
        _GlitchSeed ("Glitch Seed", Float) = 0
        _GlitchEnabled ("Glitch Enabled", Range(0, 1)) = 0
        _GlitchLineThickness ("Glitch Line Thickness", Range(0.25, 4)) = 1
        _FresnelColor ("Fresnel Color", Color) = (0.45, 0.85, 1, 1)
        _FresnelPower ("Fresnel Power", Range(1, 8)) = 3
        _FresnelStrength ("Fresnel Strength", Range(0, 2)) = 0.65
        _PortalTex ("Portal View", 2D) = "black" {}
        _PortalBlend ("Portal Blend", Range(0, 1)) = 1
        _PortalTint ("Portal Tint", Color) = (1, 1, 1, 1)
        _PortalHoleProgress ("Portal Hole Progress", Range(0, 1)) = 0
        _PortalHoleMaxRadius ("Portal Hole Max Radius", Float) = 0.72
        _PortalHoleSoftness ("Portal Hole Softness", Range(0.001, 0.4)) = 0.08
        _PortalHoleCount ("Portal Hole Count", Range(0, 8)) = 0
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

            TEXTURE2D(_PortalTex);
            SAMPLER(sampler_PortalTex);

            CBUFFER_START(UnityPerMaterial)
                half _WarpPower;
                half _DistortionStrength;
                half _BaseAlpha;
                half _ObjectRadius;
                float4 _GlitchPointUV;
                float _GlitchSeed;
                half _GlitchEnabled;
                half _GlitchLineThickness;
                half4 _FresnelColor;
                half _FresnelPower;
                half _FresnelStrength;
                half _PortalBlend;
                half4 _PortalTint;
                half _PortalHoleProgress;
                half _PortalHoleMaxRadius;
                half _PortalHoleSoftness;
                half _PortalHoleCount;
                float4 _PortalHoleCenters[8];
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

            float DistanceToSegment(float2 queryUV, float2 segmentStart, float2 segmentEnd)
            {
                float2 segment = segmentEnd - segmentStart;
                float segmentLengthSqr = max(dot(segment, segment), 0.0000001);
                float t = saturate(dot(queryUV - segmentStart, segment) / segmentLengthSqr);
                return length(queryUV - (segmentStart + segment * t));
            }

            float2 WarpPlaneToUV(float2 plane, float2 centerUV, float2 rightRadiusUV, float2 upRadiusUV)
            {
                float planeLength = length(plane);
                float2 dirN = plane / max(planeLength, 0.0001);
                float r = saturate(planeLength);
                float outerR = pow(r, 1.0 / max(_WarpPower, 0.0001));
                float sampleR = lerp(r, outerR, _DistortionStrength);
                float2 samplePlane = dirN * sampleR;
                return saturate(centerUV + rightRadiusUV * samplePlane.x + upRadiusUV * samplePlane.y);
            }

            float2 WarpPlaneToPortalUV(float2 plane)
            {
                float planeLength = length(plane);
                float2 dirN = plane / max(planeLength, 0.0001);
                float r = saturate(planeLength);
                float outerR = pow(r, 1.0 / max(_WarpPower, 0.0001));
                float sampleR = lerp(r, outerR, _DistortionStrength);
                float2 samplePlane = dirN * sampleR;
                return saturate(samplePlane * 0.5 + 0.5);
            }

            float PortalHoleCurve(float x)
            {
                return 1.8447 * x * x * x - 3.3670 * x * x + 1.4539 * x;
            }

            float PortalHoleRise(float t)
            {
                float curveStart = PortalHoleCurve(1.2);
                float curveValue = PortalHoleCurve(lerp(1.2, 1.6, saturate(t)));
                return max(0.0, (curveValue - curveStart) / max(1.0 - curveStart, 0.0001));
            }

            float PortalHoleStagedProgress(float progress)
            {
                progress = saturate(progress);
                float firstRise = PortalHoleRise(progress / 0.42) * 0.42;
                float secondRise = lerp(0.42, 1.0, PortalHoleRise((progress - 0.58) / 0.42));
                return progress < 0.42 ? firstRise : (progress < 0.58 ? 0.42 : secondRise);
            }

            float PortalHoleMask(float3 sphereDirection)
            {
                float radius = _PortalHoleMaxRadius * PortalHoleStagedProgress(_PortalHoleProgress) * 1.5;
                float softness = max(_PortalHoleSoftness, 0.001);
                float mask = 0.0;

                [unroll]
                for (int i = 0; i < 8; i++)
                {
                    float enabled = step(i + 0.5, _PortalHoleCount);
                    float3 center = normalize(_PortalHoleCenters[i].xyz);
                    float chordDistance = length(sphereDirection - center);
                    float hole = 1.0 - smoothstep(radius, radius + softness, chordDistance);
                    mask = max(mask, hole * enabled);
                }

                return saturate(mask);
            }

            float2 ScreenUVToWarpPlane(float2 screenUV, float2 centerUV, float2 rightRadiusUV, float2 upRadiusUV)
            {
                float2 delta = screenUV - centerUV;
                float determinant = rightRadiusUV.x * upRadiusUV.y - rightRadiusUV.y * upRadiusUV.x;
                if (abs(determinant) < 0.0000001)
                    return float2(99.0, 99.0);

                float invDeterminant = 1.0 / determinant;
                return float2(
                    (delta.x * upRadiusUV.y - delta.y * upRadiusUV.x) * invDeterminant,
                    (rightRadiusUV.x * delta.y - rightRadiusUV.y * delta.x) * invDeterminant
                );
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

                float2 sampleUV = WarpPlaneToUV(objectPlane, centerUV, rightRadiusUV, upRadiusUV);

                half3 bg = SampleSceneColor(sampleUV);
                float2 portalUV = WarpPlaneToPortalUV(objectPlane);
                half3 portalView = SAMPLE_TEXTURE2D(_PortalTex, sampler_PortalTex, portalUV).rgb * _PortalTint.rgb;
                float3 objectPosition = TransformWorldToObject(input.positionWS);
                float3 sphereDirection = normalize(objectPosition / objectRadius);
                half portalMask = saturate(PortalHoleMask(sphereDirection) * _PortalBlend);
                bg = lerp(bg, portalView, portalMask);

                float3 viewDirWS = normalize(GetWorldSpaceViewDir(input.positionWS));
                float ndotV = saturate(dot(normalize(input.normalWS), viewDirWS));
                half fresnel = pow(1.0 - ndotV, _FresnelPower) * _FresnelStrength;
                bg = lerp(bg, _FresnelColor.rgb, saturate(fresnel));

                float2 glitchStartUV = _GlitchPointUV.xy;
                float2 glitchPlane = ScreenUVToWarpPlane(glitchStartUV, centerUV, rightRadiusUV, upRadiusUV);
                float2 glitchEndUV = WarpPlaneToUV(glitchPlane, centerUV, rightRadiusUV, upRadiusUV);
                float pixelWidth = max(1.0 / _ScreenParams.x, 1.0 / _ScreenParams.y) * max(_GlitchLineThickness, 0.25);
                float2 pixelSize = max(float2(1.0 / _ScreenParams.x, 1.0 / _ScreenParams.y), float2(0.000001, 0.000001));
                float pointMask = step(length((uv - glitchStartUV) / pixelSize), 0.75);
                float lineMask = step(DistanceToSegment(uv, glitchStartUV, glitchEndUV), pixelWidth);
                float insideGlitchArea = step(length(glitchPlane), 1.0);
                float glitchMask = saturate(max(pointMask, lineMask) * _GlitchEnabled * insideGlitchArea);
                bg = lerp(bg, half3(0.0, 0.0, 0.0), glitchMask);

                return half4(bg, _BaseAlpha);
            }
            ENDHLSL
        }
    }

    FallBack Off
}
