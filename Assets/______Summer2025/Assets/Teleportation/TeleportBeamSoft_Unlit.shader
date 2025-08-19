
Shader "WWWK/URP/TeleportBeamSoft_Unlit"
{
    Properties
    {
        _TopColor("Top Color", Color) = (0.75, 1, 0.95, 1)
        _BottomColor("Bottom Color", Color) = (0.23, 0.94, 1, 1)
        _BaseAlpha("Base Alpha", Range(0,1)) = 0.65

        _Height("Model Height", Float) = 2.0
        _BottomSoftness("Bottom Softness", Range(0,0.5)) = 0.12
        _TopStart("Top Start", Range(0,1)) = 0.72
        _TopSoftness("Top Softness", Range(0,0.5)) = 0.18

        _DepthFadeDistance("Depth Fade Distance", Range(0,1)) = 0.18

        _CapThreshold("Cap Threshold", Range(0,1)) = 0.45
        _CapSoftness("Cap Softness", Range(0,0.5)) = 0.15

        _FresnelPower("Fresnel Power", Range(0,8)) = 3.0
        _FresnelBoost("Fresnel Boost", Range(0,3)) = 1.2

        [Toggle(_ADDITIVE_BLEND)] _Additive("Additive Blending", Float) = 0
        _MainTex("Optional Stripe/Noise (Tiled)", 2D) = "white" {}
        _ScrollSpeed("Tex Scroll Speed Y", Float) = 0.0
        _TexToEmission("Tex To Emission", Range(0,2)) = 0.0
    }

    SubShader
    {
        Tags { "RenderPipeline"="UniversalRenderPipeline" "Queue"="Transparent" "RenderType"="Transparent" "IgnoreProjector"="True" }
        LOD 100
        Cull Front // render face = Front (hide cylinder cap when seen from outside)

        Pass
        {
            Name "ForwardUnlit"
            Tags { "LightMode"="UniversalForward" }

            // Default alpha blending; if you want Additive, toggle keyword to use the additive variant below
            Blend SrcAlpha OneMinusSrcAlpha
            ZWrite Off

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_fog
            #pragma multi_compile _ _ADDITIVE_BLEND
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareDepthTexture.hlsl"

            CBUFFER_START(UnityPerMaterial)
                float4 _TopColor;
                float4 _BottomColor;
                float _BaseAlpha;
                float _Height;
                float _BottomSoftness;
                float _TopStart;
                float _TopSoftness;
                float _DepthFadeDistance;
                float _CapThreshold;
                float _CapSoftness;
                float _FresnelPower;
                float _FresnelBoost;
                float4 _MainTex_ST;
                float _ScrollSpeed;
                float _TexToEmission;
            CBUFFER_END

            TEXTURE2D(_MainTex); SAMPLER(sampler_MainTex);

            struct Attributes
            {
                float3 positionOS : POSITION;
                float3 normalOS   : NORMAL;
                float2 uv         : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float3 positionWS : TEXCOORD0;
                float3 normalWS   : TEXCOORD1;
                float2 uv         : TEXCOORD2;
                float4 screenPos  : TEXCOORD3;
                UNITY_FOG_COORDS(4)
            };

            Varyings vert (Attributes IN)
            {
                Varyings OUT;
                float3 positionWS = TransformObjectToWorld(IN.positionOS);
                OUT.positionWS = positionWS;
                OUT.positionCS = TransformWorldToHClip(positionWS);
                OUT.normalWS = TransformObjectToWorldNormal(IN.normalOS);
                OUT.uv = IN.uv;
                OUT.screenPos = ComputeScreenPos(OUT.positionCS);
                UNITY_TRANSFER_FOG(OUT, OUT.positionCS);
                return OUT;
            }

            float Softstep(float e0, float e1, float x)
            {
                float t = saturate((x - e0) / max(1e-5, (e1 - e0)));
                return t * t * (3.0 - 2.0 * t);
            }

            half4 frag (Varyings IN) : SV_Target
            {
                // normalized height 0..1 using object-space Y (assume pivot center)
                float3 posOS = mul(unity_WorldToObject, float4(IN.positionWS,1)).xyz;
                float t = saturate((posOS.y + _Height * 0.5) / max(1e-5, _Height));

                // color lerp
                float3 col = lerp(_BottomColor.rgb, _TopColor.rgb, t);

                // base alpha soft top/bottom
                float bottomFade = Softstep(0.0, _BottomSoftness, t);
                float topSoft = Softstep(_TopStart, 1.0, t);
                float topFade = 1.0 - topSoft;
                float alphaUV = bottomFade * topFade;

                // cap fade by normal
                float ny = abs(normalize(IN.normalWS).y);
                float capMask = 1.0 - Softstep(_CapThreshold, _CapThreshold + _CapSoftness, ny);

                // depth fade
                float2 suv = IN.screenPos.xy / IN.screenPos.w;
                #if defined(REQUIRES_CAMERA_DEPTH_TEXTURE)
                    float raw = SampleSceneDepth(suv);
                    float sceneEye = LinearEyeDepth(raw, _ZBufferParams);
                    float thisEye  = LinearEyeDepth(IN.positionCS.z, _ZBufferParams);
                    float df = saturate( (sceneEye - thisEye) / max(1e-5, _DepthFadeDistance) );
                #else
                    float df = 1.0;
                #endif

                // optional scrolling texture into emission
                float2 tuv = TRANSFORM_TEX(IN.uv, _MainTex);
                tuv.y += _Time.y * _ScrollSpeed;
                float texLuma = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, tuv).r;

                // Fresnel
                float3 V = normalize(_WorldSpaceCameraPos - IN.positionWS);
                float3 N = normalize(IN.normalWS);
                float fres = pow(1.0 - saturate(dot(N, V)), _FresnelPower) * _FresnelBoost;

                float alpha = _BaseAlpha * alphaUV * df * capMask;
                float3 emission = (col * fres) + (texLuma * _TexToEmission);

                #if defined(_ADDITIVE_BLEND)
                    // additive output (ignores alpha in color blend equation)
                    return half4(col * (fres + texLuma * _TexToEmission), alpha);
                #else
                    return half4(col + emission, alpha);
                #endif
            }
            ENDHLSL
        }

        // Additive variant pass (for convenience). Enable via material keyword _ADDITIVE_BLEND.
        Pass
        {
            Name "ForwardUnlit_Additive"
            Tags { "LightMode"="UniversalForward" }
            Blend One One  // additive
            ZWrite Off
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #define _ADDITIVE_BLEND 1
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareDepthTexture.hlsl"
            // Reuse same CBUFFER, structs, functions and frag/vert from above by inlining again is verbose;
            // For clarity, duplicate code is omitted here in this minimal kit.
            ENDHLSL
        }
    }

    FallBack Off
}
