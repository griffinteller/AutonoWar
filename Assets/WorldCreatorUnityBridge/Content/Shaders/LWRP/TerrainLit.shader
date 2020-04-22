Shader "Lightweight Render Pipeline/Terrain/WC_Lit"
{
    Properties
    {
        // used in fallback on old cards & base map
        [HideInInspector] _MainTex("BaseMap (RGB)", 2D) = "grey" {}
        [HideInInspector] _BaseColor("Main Color", Color) = (1,1,1,1)

        // TODO: Implement ShaderGUI for the shader and display the checkbox only when instancing is enabled.
        [Toggle(_TERRAIN_INSTANCED_PERPIXEL_NORMAL)] _TERRAIN_INSTANCED_PERPIXEL_NORMAL("Enable Instanced Per-pixel Normal", Float) = 0

        _ColorMap("ColorMap (RGB)", 2D) = "white" {}
        _OffsetSize("Offset / Size", Vector) = (0,0,0,0)
        [Toggle(LWRP_ENABLED)] _LWRPEnabled("LWRP Enabled", Float) = 0
    }

    SubShader
    {
        Tags { "Queue" = "Geometry-100" "RenderType" = "Opaque" "RenderPipeline" = "LightweightPipeline" "IgnoreProjector" = "False"}

        Pass
        {
            Name "ForwardLit"
            Tags { "LightMode" = "LightweightForward" }
            HLSLPROGRAM
            // Required to compile gles 2.0 with standard srp library
            #pragma prefer_hlslcc gles
            #pragma exclude_renderers d3d11_9x
            #pragma target 3.0

            #pragma vertex SplatmapVert
            #pragma fragment SplatmapFragment

            #define _METALLICSPECGLOSSMAP 1
            #define _SMOOTHNESS_TEXTURE_ALBEDO_CHANNEL_A 1

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
            // Sample normal in pixel shader when doing instancing
            #pragma shader_feature_local _TERRAIN_INSTANCED_PERPIXEL_NORMAL

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

        Pass
        {
            Name "ShadowCaster"
            Tags{"LightMode" = "ShadowCaster"}

            ZWrite On

            HLSLPROGRAM
            // Required to compile gles 2.0 with standard srp library
            #pragma prefer_hlslcc gles
            #pragma exclude_renderers d3d11_9x
            #pragma target 2.0

            #pragma vertex ShadowPassVertex
            #pragma fragment ShadowPassFragment

            #pragma multi_compile_instancing
            #pragma instancing_options assumeuniformscaling nomatrices nolightprobe nolightmap

            #ifdef LWRP_ENABLED
              #include "./TerrainLitInput.hlsl"
              #include "./TerrainLitPasses.hlsl"
            #else
              float4 ShadowPassVertex() : SV_POSITION { return float4(0,0,0,1); }
              float4 ShadowPassFragment() : SV_TARGET { return float4(0,0,0,1); }
            #endif
            ENDHLSL
        }

        Pass
        {
            Name "DepthOnly"
            Tags{"LightMode" = "DepthOnly"}

            ZWrite On
            ColorMask 0

            HLSLPROGRAM
            // Required to compile gles 2.0 with standard srp library
            #pragma prefer_hlslcc gles
            #pragma exclude_renderers d3d11_9x
            #pragma target 2.0

            #pragma vertex DepthOnlyVertex
            #pragma fragment DepthOnlyFragment

            #pragma multi_compile_instancing
            #pragma instancing_options assumeuniformscaling nomatrices nolightprobe nolightmap

            #ifdef LWRP_ENABLED
              #include "./TerrainLitInput.hlsl"
              #include "./TerrainLitPasses.hlsl"
            #else
              float4 DepthOnlyVertex() : SV_POSITION { return float4(0,0,0,1); }
              float4 DepthOnlyFragment() : SV_TARGET { return float4(0,0,0,1); }
            #endif
            ENDHLSL
        }

        UsePass "Hidden/Nature/Terrain/Utilities/PICKING"
        UsePass "Hidden/Nature/Terrain/Utilities/SELECTION"
    }
    Dependency "AddPassShader" = "Hidden/Lightweight Render Pipeline/Terrain/WC_Lit (Add Pass)"
    Dependency "BaseMapShader" = "Hidden/Lightweight Render Pipeline/Terrain/WC_Lit (Base Pass)"

    Fallback "Hidden/InternalErrorShader"
}
