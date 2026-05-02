Shader "MagicalEcosystem/Experiment/WindSurface"
{
    Properties
    {
        _LineColor ("Line Color", Color) = (0.82, 1.0, 0.9, 1)
        _DarkLineColor ("Dark Line Color", Color) = (0.05, 0.18, 0.12, 1)
        _LineIntensity ("Line Intensity", Range(0, 3)) = 1.25
        _LineWidth ("Line Width", Range(0.002, 0.12)) = 0.025
        _StreamCount ("Stream Count", Range(1, 12)) = 1
        _Twist ("Twist", Range(-8, 8)) = 2.4
        _Speed ("Speed", Range(-5, 5)) = 1.4
        _SurfaceAlpha ("Surface Alpha", Range(0, 1)) = 0.18
        _Fade ("Fade", Range(0, 1)) = 1
        _RotationOffset ("Rotation Offset", Range(0, 6.2831853)) = 0
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
        Blend SrcAlpha One

        Pass
        {
            Name "Forward"
            Tags { "LightMode" = "UniversalForward" }

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            CBUFFER_START(UnityPerMaterial)
                half4 _LineColor;
                half4 _DarkLineColor;
                float _LineIntensity;
                float _LineWidth;
                float _StreamCount;
                float _Twist;
                float _Speed;
                float _SurfaceAlpha;
                float _Fade;
                float _RotationOffset;
            CBUFFER_END

            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS : NORMAL;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float3 normalOS : TEXCOORD0;
                float3 normalWS : TEXCOORD1;
                float3 positionWS : TEXCOORD2;
            };

            Varyings vert(Attributes input)
            {
                Varyings output;
                VertexPositionInputs positionInputs = GetVertexPositionInputs(input.positionOS.xyz);
                VertexNormalInputs normalInputs = GetVertexNormalInputs(input.normalOS);
                output.positionCS = positionInputs.positionCS;
                output.normalOS = normalize(input.normalOS);
                output.normalWS = normalize(normalInputs.normalWS);
                output.positionWS = positionInputs.positionWS;
                return output;
            }

            float Hash01(float n)
            {
                return frac(sin(n * 17.371 + 43.17) * 43758.5453);
            }

            float BuildStreamMask(float3 n, float3 axis, float3 reference, float timeOffset)
            {
                float3 safeAxis = normalize(axis);
                float3 tangentA = normalize(reference - safeAxis * dot(reference, safeAxis));
                float3 tangentB = normalize(cross(safeAxis, tangentA));
                float longitude = atan2(dot(n, tangentB), dot(n, tangentA)) / 6.2831853 + 0.5;
                float latitude = asin(clamp(dot(n, safeAxis), -1.0, 1.0)) / 3.14159265 + 0.5;
                float streamMask = 0.0;
                float streamCount = _StreamCount > 1.0 ? _StreamCount : 1.0;

                [unroll]
                for (int i = 0; i < 12; i++)
                {
                    if (i >= streamCount)
                        break;

                    float seed = (float)i + timeOffset * 19.0;
                    float startOffset = Hash01(seed) + _Time.y * _Speed * (0.18 + Hash01(seed + 5.0) * 0.12);
                    float latitudeOffset = (Hash01(seed + 1.7) - 0.5) * 0.34;
                    float wave = sin((longitude + startOffset) * 6.2831853);
                    float path = frac(longitude + startOffset);
                    float twistMul = lerp(0.65, 1.35, Hash01(seed + 9.1));
                    float center = 0.5 + latitudeOffset + wave * 0.16 + (path - 0.5) * _Twist * twistMul * 0.06;
                    float d = abs(latitude - center);
                    float stroke = 1.0 - smoothstep(_LineWidth, _LineWidth * 2.8, d);
                    float gate = smoothstep(0.02, 0.18, path) * (1.0 - smoothstep(0.78, 1.0, path));
                    float strokeMask = stroke * gate;
                    streamMask += strokeMask;
                }

                return saturate(streamMask);
            }

            half4 frag(Varyings input, FRONT_FACE_TYPE facing : FRONT_FACE_SEMANTIC) : SV_Target
            {
                float3 n = normalize(input.normalOS);
                float angle = _Time.y * _Speed * 0.5 + _RotationOffset;
                float sinA = sin(angle);
                float cosA = cos(angle);
                float3 axis = normalize(float3(cosA * 0.25, 1.0, sinA * 0.25));
                float streamMask = BuildStreamMask(n, axis, float3(1.0, 0.0, 0.0), 0.0);

                float3 normalWS = normalize(input.normalWS);
                float3 viewDirWS = normalize(GetWorldSpaceViewDir(input.positionWS));
                half rim = pow(1.0h - saturate(dot(normalWS, viewDirWS)), 2.5h);
                bool frontFace = IS_FRONT_VFACE(facing, true, false);
                half layerShade = frontFace ? 1.0h : 0.55h;
                half3 lineColor = lerp(_DarkLineColor.rgb, _LineColor.rgb, frontFace ? 1.0h : 0.35h);
                half alpha = saturate((_SurfaceAlpha * rim + streamMask * _LineIntensity) * _Fade);
                half3 color = lineColor * (streamMask * _LineIntensity + rim * 0.25h) * layerShade;
                return half4(color, alpha);
            }
            ENDHLSL
        }
    }

    FallBack Off
}
