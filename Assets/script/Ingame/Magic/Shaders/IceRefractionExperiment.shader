Shader "MagicalEcosystem/Experiment/IceRefraction"
{
    Properties
    {
        _LightBlue ("Light Blue", Color) = (0.55, 0.9, 1.0, 1)
        _DeepBlue ("Deep Blue", Color) = (0.02, 0.22, 0.55, 1)
        _IceWhite ("Ice White", Color) = (0.92, 0.98, 1.0, 1)
        _BlueAmount ("Blue Amount", Range(0, 1)) = 0.42
        _FresnelPower ("Fresnel Power", Range(1, 8)) = 4
        _ThicknessScale ("Thickness Scale", Range(0, 4)) = 0.75
        _DistortionStrength ("Distortion Strength", Range(0, 0.08)) = 0.015
        _NoiseTex ("Noise Tex", 2D) = "white" {}
        _NoiseScale ("Noise Scale", Range(0.1, 20)) = 4
        _NoiseDistortionStrength ("Noise Distortion Strength", Range(0, 0.08)) = 0.012
        _CrackStrength ("Crack Strength", Range(0, 1)) = 0.22
        _Alpha ("Alpha", Range(0, 1)) = 0.42
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
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareOpaqueTexture.hlsl"
            TEXTURE2D(_NoiseTex);
            SAMPLER(sampler_NoiseTex);
            CBUFFER_START(UnityPerMaterial)
                half4 _LightBlue;
                half4 _DeepBlue;
                half4 _IceWhite;
                half _BlueAmount;
                half _FresnelPower;
                half _ThicknessScale;
                half _DistortionStrength;
                float4 _NoiseTex_ST;
                half _NoiseScale;
                half _NoiseDistortionStrength;
                half _CrackStrength;
                half _Alpha;
            CBUFFER_END
            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS : NORMAL;
                float2 uv : TEXCOORD0;
            };
            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float3 positionWS : TEXCOORD0;
                float3 normalWS : TEXCOORD1;
                float3 normalVS : TEXCOORD2;
                float3 positionOS : TEXCOORD3;
                float2 uv : TEXCOORD4;
            };
            Varyings vert(Attributes input)
            {
                Varyings output;
                VertexPositionInputs positionInputs = GetVertexPositionInputs(input.positionOS.xyz);
                VertexNormalInputs normalInputs = GetVertexNormalInputs(input.normalOS);
                output.positionCS = positionInputs.positionCS;
                output.positionWS = positionInputs.positionWS;
                output.normalWS = normalInputs.normalWS;
                output.normalVS = TransformWorldToViewDir(normalInputs.normalWS, true);
                output.positionOS = input.positionOS.xyz;
                output.uv = TRANSFORM_TEX(input.uv, _NoiseTex);
                return output;
            }
            half4 frag(Varyings input) : SV_Target
            {
                float3 normalWS = normalize(input.normalWS);
                float3 normalVS = normalize(input.normalVS);
                float3 viewDirWS = normalize(GetWorldSpaceViewDir(input.positionWS));
                half ndotV = saturate(dot(normalWS, viewDirWS));
                half fresnel = pow(1.0h - ndotV, _FresnelPower);
                Light mainLight = GetMainLight();
                half ndotL = saturate(dot(normalWS, normalize(mainLight.direction)));
                half lightTerm = ndotL * mainLight.distanceAttenuation * mainLight.shadowAttenuation;
                float2 screenUV = input.positionCS.xy / _ScaledScreenParams.xy;
                float2 noiseUV = input.uv * _NoiseScale + input.positionOS.xz * 0.13;
                half4 noiseSample = SAMPLE_TEXTURE2D(_NoiseTex, sampler_NoiseTex, noiseUV);
                half noise = noiseSample.r;
                half crackMask = smoothstep(0.78h, 0.96h, noiseSample.g);
                half fogNoise = noiseSample.b - 0.5h;
                float2 normalDistortion = normalVS.xy * _DistortionStrength;
                float2 noiseDistortion = (noiseSample.rg - 0.5h) * _NoiseDistortionStrength;
                float2 distortedUV = saturate(screenUV + normalDistortion + noiseDistortion);
                half3 bg = SampleSceneColor(distortedUV);
                half bgLuma = dot(bg, half3(0.299h, 0.587h, 0.114h));
                bg = lerp(half3(bgLuma, bgLuma, bgLuma), bg, 0.85h);
                half thicknessGeom = saturate(length(input.positionOS) + fogNoise * 0.35h);
                half thicknessView = rcp(max(ndotV, 0.2h));
                half thickness = thicknessGeom * thicknessView;
                half blueT = saturate(thickness * _ThicknessScale);
                half3 blue = lerp(_LightBlue.rgb, _DeepBlue.rgb, blueT);
                half3 blueFiltered = lerp(bg, blue, _BlueAmount);
                half3 whiteReflection = _IceWhite.rgb * saturate(fresnel * (0.25h + lightTerm * 1.35h));
                half3 color = lerp(blueFiltered, whiteReflection, fresnel);
                half internalFog = saturate((noise - 0.62h) * 2.5h) * (1.0h - fresnel) * 0.18h;
                half3 crackWhite = _IceWhite.rgb * (crackMask + internalFog);
                color += crackWhite * _CrackStrength;
                half alpha = saturate(_Alpha + fresnel * 0.18h + blueT * 0.08h);
                return half4(color, alpha);
            }
            ENDHLSL
        }
    }
    FallBack Off
}
