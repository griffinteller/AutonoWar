Shader "Unlit/Silhouette"
{
    Properties
    {
        _Color ("Color", Color) = (1, 1, 1, 1)
        _OutlineWidth ("Outline Width", float) = 0.1
        _ObjectWidth("Object Width", float) = 1.0
        
    }
    SubShader
    {
        Tags { "Queue"="Overlay" }
        
        Pass {
        
            Name "Masking"
            Cull back
            ZTest Always
            ZWrite Off
            Stencil { // don't draw anything, but mark where the object is rendered
                Ref 1
                Comp never
                Fail replace
            }
        
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            
            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
            };
            
            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                return o;
            }
            
            fixed4 frag (v2f i) : SV_Target
            {
                return (0.0, 0.0, 0.0, 0.0);
            }
            ENDCG
        
        
        }

        Pass
        {
            Name "Silhouette"
            Cull back
            ZTest Always
            ZWrite Off
            Stencil { // only render where the object isn't
                Ref 1
                Comp NotEqual
            }
            
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            
            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
            };
            
            fixed4 _Color;
            float _OutlineWidth;
            float _ObjectWidth;
            
            v2f vert (appdata v)
            {
                v2f o;
                
                float4 objectOriginView = float4(UnityObjectToViewPos(float3(0.0, 0.0, 0.0)), 1.0);
                float4 normal = normalize (v.vertex);
                
                o.vertex = v.vertex;
                o.vertex += normal * _OutlineWidth * -objectOriginView.z / _ObjectWidth;
                o.vertex = float4(UnityObjectToViewPos(o.vertex.xyz), 1.0);
                o.vertex = mul (UNITY_MATRIX_P, o.vertex);
                return o;
            }
            
            fixed4 frag (v2f i) : SV_Target
            {
                fixed4 col = _Color;
                return col;
            }
            ENDCG
        }
    }
}