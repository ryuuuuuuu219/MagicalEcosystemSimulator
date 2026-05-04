Shader "MagicalEcosystem/Experiment/SpellCircleFractal"
{
    Properties
    {
        _BaseColor ("Line Color", Color) = (1, 1, 1, 0.5)
        _Sides ("Sides", Float) = 12
        _LineWidth ("Line Width", Range(0.001, 0.08)) = 0.018
        _MinScale ("Minimum Scale", Range(0.02, 0.95)) = 0.16
        _MaxIterations ("Max Iterations", Float) = 7
        _SpawnTime ("Spawn Time", Float) = 0
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
                half4 _BaseColor;
                half _Sides;
                half _LineWidth;
                half _MinScale;
                half _MaxIterations;
                half _SpawnTime;
            CBUFFER_END

            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float2 uv : TEXCOORD0;
            };

            Varyings vert(Attributes input)
            {
                Varyings output;
                output.positionCS = TransformObjectToHClip(input.positionOS.xyz);
                output.uv = input.uv;
                return output;
            }

            float2 Rotate2D(float2 value, float angle)
            {
                float s = sin(angle);
                float c = cos(angle);
                return float2(value.x * c - value.y * s, value.x * s + value.y * c);
            }

            float DistanceToSegment(float2 samplePos, float2 startPos, float2 endPos)
            {
                float2 pa = samplePos - startPos;
                float2 ba = endPos - startPos;
                float h = saturate(dot(pa, ba) / max(dot(ba, ba), 0.0001));
                return length(pa - ba * h);
            }

            float PolygonLine(float2 samplePos, float radius, int sides)
            {
                float distanceToEdge = 10.0;
                [unroll(12)]
                for (int i = 0; i < 12; i++)
                {
                    if (i < sides)
                    {
                        float a0 = 6.2831853 * (float)i / (float)sides;
                        float a1 = 6.2831853 * (float)(i + 1) / (float)sides;
                        float2 p0 = float2(cos(a0), sin(a0)) * radius;
                        float2 p1 = float2(cos(a1), sin(a1)) * radius;
                        distanceToEdge = min(distanceToEdge, DistanceToSegment(samplePos, p0, p1));
                    }
                }

                return 1.0 - smoothstep(_LineWidth, _LineWidth * 1.8, distanceToEdge);
            }

            float MidpointPolygonLine(float2 samplePos, float radius, int sides, int generation)
            {
                if (sides <= 2)
                {
                    float scale = pow(0.5, (float)generation);
                    float angle = generation * 1.5707963;
                    float2 localPos = Rotate2D(samplePos, angle);
                    float halfLength = radius * scale;
                    return 1.0 - smoothstep(_LineWidth, _LineWidth * 1.8, DistanceToSegment(localPos, float2(-halfLength, 0.0), float2(halfLength, 0.0)));
                }

                float shrink = cos(3.14159265 / (float)sides);
                float scale = pow(shrink, (float)generation);
                float rotation = (3.14159265 / (float)sides) * (float)generation;
                float localRadius = radius * scale;
                float2 localPos = Rotate2D(samplePos, -rotation);
                return PolygonLine(localPos, localRadius, sides);
            }

            float CircleLine(float r, float radius, float widthScale)
            {
                float distanceToCircle = abs(r - radius);
                float width = _LineWidth * widthScale;
                return 1.0 - smoothstep(width, width * 1.8, distanceToCircle);
            }

            half4 frag(Varyings input) : SV_Target
            {
                float2 samplePos = input.uv * 2.0 - 1.0;
                float r = length(samplePos);
                int sides = clamp((int)round(_Sides), 2, 12);
                float age = max(0.0, _Time.y - _SpawnTime);
                float spin = age * 0.55;
                float alpha = 0.0;

                alpha += CircleLine(r, 0.93, 1.1) * 0.85;
                alpha += CircleLine(r, 0.72, 0.8) * 0.55;
                alpha += CircleLine(r, 0.32, 0.7) * 0.38;

                [unroll(10)]
                for (int i = 0; i < 10; i++)
                {
                    float sidesShrink = sides <= 2 ? 0.5 : cos(3.14159265 / (float)sides);
                    float generationScale = pow(sidesShrink, (float)i);
                    if (i < (int)_MaxIterations && generationScale >= _MinScale)
                    {
                        float2 localPoint = Rotate2D(samplePos, spin * (0.35 + i * 0.04));
                        alpha += MidpointPolygonLine(localPoint, 0.72, sides, i) * (0.72 - i * 0.045);
                    }
                }

                [unroll(12)]
                for (int j = 0; j < 12; j++)
                {
                    if (j < sides)
                    {
                        float angle = 6.2831853 * (float)j / (float)sides + spin * 0.35;
                        float2 axis = float2(cos(angle), sin(angle));
                        float radialDistance = abs(dot(samplePos, float2(-axis.y, axis.x)));
                        float radialLength = abs(dot(samplePos, axis));
                        float spoke = 1.0 - smoothstep(_LineWidth * 0.75, _LineWidth * 1.6, radialDistance);
                        alpha += spoke * step(radialLength, 0.88) * 0.25;

                        float2 beadPoint = axis * 0.53;
                        alpha += 1.0 - smoothstep(_LineWidth * 1.2, _LineWidth * 2.5, abs(length(samplePos - beadPoint) - 0.035));
                    }
                }

                float edgeFade = 1.0 - smoothstep(0.96, 1.0, r);
                alpha = saturate(alpha * edgeFade);
                return half4(_BaseColor.rgb, alpha * _BaseColor.a);
            }
            ENDHLSL
        }
    }

    FallBack Off
}
