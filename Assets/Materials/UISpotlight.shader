Shader "UI/Spotlight"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {} // <-- DÒNG MỚI QUAN TRỌNG
        _Color ("Tint Color", Color) = (0,0,0,0.95)
        _Center ("Center (UV)", Vector) = (0.5, 0.5, 0, 0)
        _Radius ("Hole Radius", Range(0, 1)) = 0.15
        _Softness ("Edge Softness", Range(0.01, 1)) = 0.1
        
        // Cần thiết cho UI Masking (nếu có dùng)
        _StencilComp ("Stencil Comparison", Float) = 8
        _Stencil ("Stencil ID", Float) = 0
        _StencilOp ("Stencil Operation", Float) = 0
        _WriteMask ("Stencil Write Mask", Float) = 255
        _ReadMask ("Stencil Read Mask", Float) = 255
        _ColorMask ("Color Mask", Float) = 15
    }

    SubShader
    {
        Tags
        {
            "Queue"="Transparent"
            "IgnoreProjector"="True"
            "RenderType"="Transparent"
            "PreviewType"="Plane"
            "CanUseSpriteAtlas"="True"
        }

        Stencil
        {
            Ref [_Stencil]
            Comp [_StencilComp]
            Pass [_StencilOp] 
            ReadMask [_ReadMask]
            WriteMask [_WriteMask]
        }

        Cull Off
        Lighting Off
        ZWrite Off
        ZTest [unity_GUIZTestMode]
        Blend SrcAlpha OneMinusSrcAlpha
        ColorMask [_ColorMask]

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 2.0

            #include "UnityCG.cginc"
            #include "UnityUI.cginc"

            struct appdata_t
            {
                float4 vertex   : POSITION;
                float4 color    : COLOR;
                float2 texcoord : TEXCOORD0;
            };

            struct v2f
            {
                float4 vertex   : SV_POSITION;
                fixed4 color    : COLOR;
                float2 texcoord : TEXCOORD0;
                float4 worldPosition : TEXCOORD1;
            };

            fixed4 _Color;
            float4 _Center;
            float _Radius;
            float _Softness;
            sampler2D _MainTex; // <-- KHAI BÁO BIẾN Ở ĐÂY

            v2f vert(appdata_t v)
            {
                v2f OUT;
                OUT.worldPosition = v.vertex;
                OUT.vertex = UnityObjectToClipPos(OUT.worldPosition);
                OUT.texcoord = v.texcoord;
                OUT.color = v.color * _Color;
                return OUT;
            }

            fixed4 frag(v2f IN) : SV_Target
            {
                // Tính khoảng cách
                float dist = distance(IN.texcoord, _Center.xy);
                
                // Hiệu ứng lỗ thủng
                float alphaMask = smoothstep(_Radius, _Radius + _Softness, dist);

                // Lấy màu từ texture (mặc định là trắng nếu Source Image = None)
                // Phải nhân vào để Unity ko báo lỗi logic
                fixed4 texColor = tex2D(_MainTex, IN.texcoord);
                
                fixed4 color = IN.color * texColor;
                color.a *= alphaMask;

                return color;
            }
            ENDCG
        }
    }
}