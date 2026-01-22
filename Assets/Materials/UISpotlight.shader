Shader "UI/Spotlight"
{
    Properties
    {
        _Color ("Tint Color", Color) = (0,0,0,0.95) // Màu nền tối (mặc định đen mờ)
        _Center ("Center (UV)", Vector) = (0.5, 0.5, 0, 0) // Vị trí tâm lỗ (0.5, 0.5 là giữa màn hình)
        _Radius ("Hole Radius", Range(0, 1)) = 0.15 // Độ lớn của lỗ thủng
        _Softness ("Edge Softness", Range(0.01, 1)) = 0.1 // Độ mờ viền lỗ
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

        Cull Off
        Lighting Off
        ZWrite Off
        ZTest [unity_GUIZTestMode]
        Blend SrcAlpha OneMinusSrcAlpha

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
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
                // Tính khoảng cách từ pixel hiện tại đến tâm (_Center)
                float dist = distance(IN.texcoord, _Center.xy);
                
                // Tạo hiệu ứng lỗ thủng: Nếu gần tâm thì alpha = 0, xa tâm thì alpha = 1
                // smoothstep giúp làm mềm viền
                float alphaMask = smoothstep(_Radius, _Radius + _Softness, dist);

                fixed4 color = IN.color;
                color.a *= alphaMask; // Nhân alpha gốc với mask lỗ thủng

                return color;
            }
            ENDCG
        }
    }
}