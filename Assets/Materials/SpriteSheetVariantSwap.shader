Shader "Custom/SpriteSheetVariantSwap"
{
    Properties
    {
        _BaseTex ("Base Sprite Sheet", 2D) = "white" {}
        _VariantTex ("Variant Sprite Sheet", 2D) = "white" {}
    }

    SubShader
    {
        Tags { "RenderType"="Transparent" "Queue"="Transparent" }
        Blend SrcAlpha OneMinusSrcAlpha
        LOD 100

        Pass
        {
            Cull Off

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionHCS : SV_POSITION;
                float2 uv : TEXCOORD0;
            };

            sampler2D _BaseTex;
            sampler2D _VariantTex;

            Varyings vert (Attributes IN)
            {
                Varyings OUT;
                OUT.positionHCS = TransformObjectToHClip(IN.positionOS.xyz);
                OUT.uv = IN.uv;
                return OUT;
            }

            float4 frag (Varyings IN) : SV_Target
            {
                float2 uv = IN.uv;

                // Sample base and variant
                float4 baseColor = tex2D(_BaseTex, uv);
                float4 variantColor = tex2D(_VariantTex, uv);

                // Preserve alpha from base
                variantColor.a = baseColor.a;

                return variantColor;
            }
            ENDHLSL
        }
    }
}