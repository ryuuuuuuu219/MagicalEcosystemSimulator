Shader "MagicalEcosystem/Experiment/IceRefraction" // Material から選ぶ Shader 名。
{ // Shader 全体の開始。
    Properties // Inspector に出す調整項目。
    { // Properties の開始。
        _LightBlue ("Light Blue", Color) = (0.55, 0.9, 1.0, 1) // 薄い氷色。
        _DeepBlue ("Deep Blue", Color) = (0.02, 0.22, 0.55, 1) // 厚い部分の濃い青。
        _IceWhite ("Ice White", Color) = (0.92, 0.98, 1.0, 1) // 反射やひびの白。
        _BlueAmount ("Blue Amount", Range(0, 1)) = 0.42 // 背景を青に寄せる量。
        _FresnelPower ("Fresnel Power", Range(1, 8)) = 4 // 輪郭反射の鋭さ。
        _ThicknessScale ("Thickness Scale", Range(0, 4)) = 0.75 // 厚みの青さへの効き。
        _DistortionStrength ("Distortion Strength", Range(0, 0.08)) = 0.015 // 法線による歪み。
        _NoiseTex ("Noise Tex", 2D) = "white" {} // 濁りやひび用ノイズ。
        _NoiseScale ("Noise Scale", Range(0.1, 20)) = 4 // ノイズの細かさ。
        _NoiseDistortionStrength ("Noise Distortion Strength", Range(0, 0.08)) = 0.012 // ノイズ歪み量。
        _CrackStrength ("Crack Strength", Range(0, 1)) = 0.22 // ひび白の強さ。
        _Alpha ("Alpha", Range(0, 1)) = 0.42 // 基本透明度。
    } // Properties の終了。
    SubShader // 描画処理のまとまり。
    { // SubShader の開始。
        Tags // URP への描画分類。
        { // Tags の開始。
            "Queue" = "Transparent" // 透明物として後で描く。
            "RenderType" = "Transparent" // 透明 RenderType。
            "RenderPipeline" = "UniversalPipeline" // URP 専用。
            "IgnoreProjector" = "True" // Projector を無視。
        } // Tags の終了。
        Cull Back // 裏面を描かない。
        ZWrite Off // 透明なので深度を書かない。
        ZTest LEqual // 通常の深度テスト。
        Blend SrcAlpha OneMinusSrcAlpha // 標準アルファ合成。
        Pass // 1回の描画 Pass。
        { // Pass の開始。
            Name "Forward" // Pass 名。
            Tags { "LightMode" = "UniversalForward" } // URP Forward 用。
            HLSLPROGRAM // HLSL 開始。
            #pragma vertex vert // 頂点関数指定。
            #pragma fragment frag // ピクセル関数指定。
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl" // URP 基本関数。
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl" // ライト関数。
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareOpaqueTexture.hlsl" // 背景取得。
            TEXTURE2D(_NoiseTex); // ノイズテクスチャ宣言。
            SAMPLER(sampler_NoiseTex); // ノイズ用 sampler。
            CBUFFER_START(UnityPerMaterial) // Material 値の開始。
                half4 _LightBlue; // 薄い青。
                half4 _DeepBlue; // 濃い青。
                half4 _IceWhite; // 白反射色。
                half _BlueAmount; // 青フィルター量。
                half _FresnelPower; // Fresnel 指数。
                half _ThicknessScale; // 厚み倍率。
                half _DistortionStrength; // 法線歪み。
                float4 _NoiseTex_ST; // NoiseTex の tiling/offset。
                half _NoiseScale; // ノイズ倍率。
                half _NoiseDistortionStrength; // ノイズ歪み。
                half _CrackStrength; // ひび白量。
                half _Alpha; // 透明度。
            CBUFFER_END // Material 値の終了。
            struct Attributes // Mesh から来る値。
            { // Attributes の開始。
                float4 positionOS : POSITION; // Object 空間位置。
                float3 normalOS : NORMAL; // Object 空間法線。
                float2 uv : TEXCOORD0; // UV0。
            }; // Attributes の終了。
            struct Varyings // fragment へ渡す値。
            { // Varyings の開始。
                float4 positionCS : SV_POSITION; // Clip 空間位置。
                float3 positionWS : TEXCOORD0; // World 空間位置。
                float3 normalWS : TEXCOORD1; // World 空間法線。
                float3 normalVS : TEXCOORD2; // View 空間法線。
                float3 positionOS : TEXCOORD3; // Object 空間位置。
                float2 uv : TEXCOORD4; // ノイズ用 UV。
            }; // Varyings の終了。
            Varyings vert(Attributes input) // 頂点シェーダー。
            { // vert の開始。
                Varyings output; // 出力を作る。
                VertexPositionInputs positionInputs = GetVertexPositionInputs(input.positionOS.xyz); // 位置変換。
                VertexNormalInputs normalInputs = GetVertexNormalInputs(input.normalOS); // 法線変換。
                output.positionCS = positionInputs.positionCS; // Clip 位置を渡す。
                output.positionWS = positionInputs.positionWS; // World 位置を渡す。
                output.normalWS = normalInputs.normalWS; // World 法線を渡す。
                output.normalVS = TransformWorldToViewDir(normalInputs.normalWS, true); // View 法線を渡す。
                output.positionOS = input.positionOS.xyz; // Object 位置を渡す。
                output.uv = TRANSFORM_TEX(input.uv, _NoiseTex); // tiling 済み UV。
                return output; // 出力を返す。
            } // vert の終了。
            half4 frag(Varyings input) : SV_Target // ピクセルシェーダー。
            { // frag の開始。
                float3 normalWS = normalize(input.normalWS); // World 法線を正規化。
                float3 normalVS = normalize(input.normalVS); // View 法線を正規化。
                float3 viewDirWS = normalize(GetWorldSpaceViewDir(input.positionWS)); // 視線方向。
                half ndotV = saturate(dot(normalWS, viewDirWS)); // 正面度。
                half fresnel = pow(1.0h - ndotV, _FresnelPower); // 輪郭反射量。
                Light mainLight = GetMainLight(); // メインライト取得。
                half ndotL = saturate(dot(normalWS, normalize(mainLight.direction))); // ライト正面度。
                half lightTerm = ndotL * mainLight.distanceAttenuation * mainLight.shadowAttenuation; // ライト強度。
                float2 screenUV = input.positionCS.xy / _ScaledScreenParams.xy; // 画面 UV。
                float2 noiseUV = input.uv * _NoiseScale + input.positionOS.xz * 0.13; // ノイズ UV。
                half4 noiseSample = SAMPLE_TEXTURE2D(_NoiseTex, sampler_NoiseTex, noiseUV); // ノイズ取得。
                half noise = noiseSample.r; // 濁り用ノイズ。
                half crackMask = smoothstep(0.78h, 0.96h, noiseSample.g); // ひびマスク。
                half fogNoise = noiseSample.b - 0.5h; // 厚みムラ。
                float2 normalDistortion = normalVS.xy * _DistortionStrength; // 法線歪み。
                float2 noiseDistortion = (noiseSample.rg - 0.5h) * _NoiseDistortionStrength; // ノイズ歪み。
                float2 distortedUV = saturate(screenUV + normalDistortion + noiseDistortion); // 歪んだ画面 UV。
                half3 bg = SampleSceneColor(distortedUV); // 背景色取得。
                half bgLuma = dot(bg, half3(0.299h, 0.587h, 0.114h)); // 背景輝度。
                bg = lerp(half3(bgLuma, bgLuma, bgLuma), bg, 0.85h); // 背景を少し抑える。
                half thicknessGeom = saturate(length(input.positionOS) + fogNoise * 0.35h); // 簡易実厚み。
                half thicknessView = rcp(max(ndotV, 0.2h)); // 見かけ厚み。
                half thickness = thicknessGeom * thicknessView; // 合成厚み。
                half blueT = saturate(thickness * _ThicknessScale); // 青濃度。
                half3 blue = lerp(_LightBlue.rgb, _DeepBlue.rgb, blueT); // 厚みによる青。
                half3 blueFiltered = lerp(bg, blue, _BlueAmount); // 背景に青を混ぜる。
                half3 whiteReflection = _IceWhite.rgb * saturate(fresnel * (0.25h + lightTerm * 1.35h)); // 白反射。
                half3 color = lerp(blueFiltered, whiteReflection, fresnel); // 青透過と白反射を混ぜる。
                half internalFog = saturate((noise - 0.62h) * 2.5h) * (1.0h - fresnel) * 0.18h; // 内部白濁。
                half3 crackWhite = _IceWhite.rgb * (crackMask + internalFog); // ひびと白濁。
                color += crackWhite * _CrackStrength; // ひびを加算。
                half alpha = saturate(_Alpha + fresnel * 0.18h + blueT * 0.08h); // 最終透明度。
                return half4(color, alpha); // 色を返す。
            } // frag の終了。
            ENDHLSL // HLSL 終了。
        } // Pass の終了。
    } // SubShader の終了。
    FallBack Off // 代替 Shader なし。
} // Shader 全体の終了。
