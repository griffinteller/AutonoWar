Shader "Hidden/Lightweight Render Pipeline/Terrain/WC_Lit (Add Pass)"
{
    Properties
    {
        // used in fallback on old cards & base map
        [HideInInspector] _BaseMap("BaseMap (RGB)", 2D) = "white" {}
        [HideInInspector] _BaseColor("Main Color", Color) = (1,1,1,1)
        _ColorMap("ColorMap (RGB)", 2D) = "white" {}
        _OffsetSize("Offset / Size", Vector) = (0,0,0,0)
        [Toggle(LWRP_ENABLED)] _LWRPEnabled("LWRP Enabled", Float) = 0
    }

    SubShader
    {
        Tags { "Queue" = "Geometry-99" "RenderType" = "Opaque" "RenderPipeline" = "LightweightPipeline" "IgnoreProjector" = "True"}

        Pass
        {
            Name "TerrainAddLit"
            Tags { "LightMode" = "LightweightForward" }
            Blend One One
            HLSLPROGRAM
            // Required to compile gles 2.0 with standard srp library
            #pragma prefer_hlslcc gles
            #pragma exclude_renderers d3d11_9x
            #pragma target 3.0

            #pragma vertex SplatmapVert
            #pragma fragment SplatmapFragment

            // -------------------------------------
            // Lightweight Pipeline keywords
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS_CASCADE
            #pragma multi_compile _ _ADDITIONAL_LIGHTS_VERTEX _ADDITIONAL_LIGHTS
            #pragma multi_compile _ _ADDITIONAL_LIGHT_SHADOWS
            #pragma multi_compile _ _SHADOWS_SOFT
            #pragma multi_compile _ _MIXED_LIGHTING_SUBTRACTIVE

            // -------------------------------------
            // Unity defined keywords
            #pragma multi_compile _ DIRLIGHTMAP_COMBINED
            #pragma multi_compile _ LIGHTMAP_ON
            #pragma multi_compile_fog
            #pragma multi_compile_instancing
            #pragma instancing_options assumeuniformscaling nomatrices nolightprobe nolightmap
            
            #pragma shader_feature_local _NORMALMAP
            #define TERRAIN_SPLAT_ADDPASS 1

            #pragma shader_feature LWRP_ENABLED
            #ifdef LWRP_ENABLED
              #include "./TerrainLitInput.hlsl"
              #include "./TerrainLitPasses.hlsl"
            #else
              float4 SplatmapVert() : SV_POSITION { return float4(0,0,0,1); }
              float4 SplatmapFragment() : SV_TARGET { return float4(0,0,0,1); }
            #endif
            ENDHLSL
        }
    }
    Fallback "Hidden/InternalErrorShader"
}
