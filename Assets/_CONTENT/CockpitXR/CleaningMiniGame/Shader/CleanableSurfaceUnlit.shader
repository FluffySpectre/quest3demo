Shader "Custom/CleanableSurfaceUnlit"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _RevealTex ("Reveal Texture", 2D) = "white" {}
        _Brightness ("Brightness", Range(0.5, 2)) = 1.0
    }
    
    SubShader
    {
        Tags 
        { 
            "RenderType" = "Opaque"
            "Queue" = "Geometry"
            "RenderPipeline" = "UniversalPipeline"
        }
        
        Pass
        {
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_fog
            
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            
            struct Attributes
            {
                float4 positionOS : POSITION;
                float4 color : COLOR;
                float2 uv : TEXCOORD0;
            };
            
            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float4 color : COLOR;
                float2 uv : TEXCOORD0;
                float fogFactor : TEXCOORD1;
            };
            
            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);
            TEXTURE2D(_RevealTex);
            SAMPLER(sampler_RevealTex);
            
            CBUFFER_START(UnityPerMaterial)
                float4 _MainTex_ST;
                float4 _RevealTex_ST;
                float _Brightness;
            CBUFFER_END
            
            Varyings vert(Attributes IN)
            {
                Varyings OUT;
                
                OUT.positionCS = TransformObjectToHClip(IN.positionOS.xyz);
                OUT.color = IN.color;
                OUT.uv = TRANSFORM_TEX(IN.uv, _MainTex);
                OUT.fogFactor = ComputeFogFactor(OUT.positionCS.z);
                
                return OUT;
            }
            
            half4 frag(Varyings IN) : SV_Target
            {
                half4 texColor = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, IN.uv);
                half4 revealColor = SAMPLE_TEXTURE2D(_RevealTex, sampler_RevealTex, IN.uv);
                
                half4 dirtyWetColor = texColor * IN.color * _Brightness;
                
                // Blend between dirty/wet color and reveal color based on vertex alpha
                half4 finalColor;
                finalColor.rgb = lerp(dirtyWetColor.rgb, revealColor.rgb * _Brightness, IN.color.a);
                finalColor.a = 1.0;
                
                finalColor.rgb = MixFog(finalColor.rgb, IN.fogFactor);
                
                return finalColor;
            }
            ENDHLSL
        }
    }
}
