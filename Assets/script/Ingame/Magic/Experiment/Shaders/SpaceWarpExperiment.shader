Shader "MagicalEcosystem/Experiment/SpaceWarp"
{
    Properties
    {
        _WarpPower ("Quadratic Power", Range(1, 4)) = 2
        _DistortionStrength ("Distortion Strength", Range(0, 0.35)) = 0.12
        _RadialPushPull ("Radial Push Pull", Range(-1, 1)) = -0.35
        _AnchorSharpness ("Anchor Sharpness", Range(1, 24)) = 9
        _AnchorRandomness ("Anchor Randomness", Range(0, 1)) = 0.65
        _CenterBiasStrength ("Center Bias Strength", Range(0, 1)) = 0.35
        _BoundaryConnectPower ("Boundary Connect Power", Range(0.5, 8)) = 3
        _EdgeStart ("Edge Start", Range(0, 0.99)) = 0.72
        _DarkenStrength ("Distance Ratio Darken", Range(0, 4)) = 1.6
        _CenterDarkness ("Center Darkness", Range(0, 1)) = 0.72
        _CenterRadius ("Center Radius", Range(0.01, 1)) = 0.35
        _FresnelPower ("Fresnel Power", Range(1, 8)) = 3.5
        _RimColor ("Rim Color", Color) = (0.62, 0.42, 1.0, 1)
        _RimIntensity ("Rim Intensity", Range(0, 2)) = 0.85
        _RimAlphaBoost ("Rim Alpha Boost", Range(0, 1)) = 0.35
        _BaseAlpha ("Base Alpha", Range(0, 1)) = 0.18
        _ScreenRadius ("Screen Radius", Range(0.01, 1)) = 0.16
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
                half _RadialPushPull;
                half _AnchorSharpness;
                half _AnchorRandomness;
                half _CenterBiasStrength;
                half _BoundaryConnectPower;
                half _EdgeStart;
                half _DarkenStrength;
                half _CenterDarkness;
                half _CenterRadius;
                half _FresnelPower;
                half4 _RimColor;
                half _RimIntensity;
                half _RimAlphaBoost;
                half _BaseAlpha;
                half _ScreenRadius;
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
                float2 centerUV : TEXCOORD2;
            };

            float Hash01(float n)
            {
                return frac(sin(n * 12.9898 + 78.233) * 43758.5453);
            }

            float SignedHash(float n)
            {
                return Hash01(n) * 2.0 - 1.0;
            }

            Varyings vert(Attributes input)
            {
                Varyings output;
                VertexPositionInputs positionInputs = GetVertexPositionInputs(input.positionOS.xyz);
                VertexNormalInputs normalInputs = GetVertexNormalInputs(input.normalOS);
                float4 centerCS = TransformObjectToHClip(float3(0.0, 0.0, 0.0));
                float2 centerNdc = centerCS.xy / max(centerCS.w, 0.0001);

                output.positionCS = positionInputs.positionCS;
                output.positionWS = positionInputs.positionWS;
                output.normalWS = normalInputs.normalWS;
                output.centerUV = centerNdc * 0.5 + 0.5;
                return output;
            }

            half4 frag(Varyings input) : SV_Target
            {
                float2 uv = input.positionCS.xy / _ScaledScreenParams.xy;
                float2 dir = uv - input.centerUV;
                float radius = max(_ScreenRadius, 0.0001);
                float r = saturate(length(dir) / radius);
                float2 dirN = dir / max(length(dir), 0.0001);
                float normalizedOriginalDistance = r;

                const int anchorCount = 16;
                float anchorA = 0.0;
                float anchorWeight = 0.0001;

                [unroll]
                for (int i = 0; i < anchorCount; i++)
                {
                    float angle = (6.2831853 / anchorCount) * i;
                    float2 anchorDir = float2(cos(angle), sin(angle));
                    float angular = saturate(dot(dirN, anchorDir) * 0.5 + 0.5);
                    float weight = pow(angular, _AnchorSharpness);
                    float randomA = SignedHash((float)i + 1.37);
                    float a = lerp(1.0, 1.0 + randomA, _AnchorRandomness);
                    anchorA += a * weight;
                    anchorWeight += weight;
                }

                anchorA /= anchorWeight;
                anchorA += (1.0 - normalizedOriginalDistance) * _CenterBiasStrength;

                float boundaryFade = pow(1.0 - saturate(r), _BoundaryConnectPower);
                float edgeFade = 1.0 - smoothstep(_EdgeStart, 1.0, r);
                float quadraticDistance = pow(normalizedOriginalDistance, _WarpPower);
                float radialDelta = (quadraticDistance - normalizedOriginalDistance) * anchorA;
                float pushPullDelta = normalizedOriginalDistance * (1.0 - normalizedOriginalDistance) * _RadialPushPull * anchorA;
                float delta = (radialDelta + pushPullDelta) * boundaryFade * edgeFade;
                float2 offset = dirN * delta * _DistortionStrength;
                float2 sampleUV = saturate(uv + offset);

                half3 bg = SampleSceneColor(sampleUV);
                half distanceRatio = (half)abs(delta / max(normalizedOriginalDistance, 0.02));
                half compression = saturate(distanceRatio + abs((half)(anchorA - 1.0)) * 0.18h);
                half darken = saturate(compression * _DarkenStrength);
                half3 color = bg * (1.0h - darken);

                half centerMask = 1.0h - smoothstep(0.0h, _CenterRadius, (half)r);
                color *= 1.0h - centerMask * _CenterDarkness;

                float3 normalWS = normalize(input.normalWS);
                float3 viewDirWS = normalize(GetWorldSpaceViewDir(input.positionWS));
                half ndotV = saturate(dot(normalWS, viewDirWS));
                half fresnel = pow(1.0h - ndotV, _FresnelPower);
                half rim = saturate(fresnel * _RimIntensity);

                color = lerp(color, _RimColor.rgb, rim);

                half warpVisibility = saturate(compression * 1.35h + centerMask * 0.35h);
                half alpha = saturate(max(_BaseAlpha + fresnel * _RimAlphaBoost, warpVisibility));
                return half4(color, alpha);
            }
            ENDHLSL
        }
    }

    FallBack Off
}
