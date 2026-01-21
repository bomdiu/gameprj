Shader "Custom/SpriteFlash" {
    Properties {
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
        _FlashColor ("Flash Color", Color) = (1,1,1,1)
        _FlashAmount ("Flash Amount", Range(0,1)) = 0
    }

    SubShader {
        Tags { 
            "Queue"="Transparent" 
            "RenderType"="Transparent" 
            "RenderPipeline" = "UniversalPipeline" 
        }

        Blend SrcAlpha OneMinusSrcAlpha
        Cull Off ZWrite Off

        Pass {
            Tags { "LightMode" = "Universal2D" }

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes {
                float4 positionOS : POSITION;
                float2 uv : TEXCOORD0;
                float4 color : COLOR;
            };

            struct Varyings {
                float4 positionCS : SV_POSITION;
                float2 uv : TEXCOORD0;
                float4 color : COLOR;
                float4 screenPos : TEXCOORD1; // Required to find where the light is
            };

            sampler2D _MainTex;
            // This is the "hidden" texture where URP 2D stores all scene lighting
            sampler2D _ShapeLightTexture0; 
            float4 _FlashColor;
            float _FlashAmount;

            Varyings vert (Attributes v) {
                Varyings o;
                o.positionCS = TransformObjectToHClip(v.positionOS.xyz);
                o.uv = v.uv;
                o.color = v.color;
                // Calculate where the sprite is on the screen to look up the light
                o.screenPos = ComputeScreenPos(o.positionCS);
                return o;
            }

            half4 frag (Varyings i) : SV_Target {
                // 1. Get the base sprite color
                half4 texColor = tex2D(_MainTex, i.uv) * i.color;
                
                // 2. Apply Flash logic
                texColor.rgb = lerp(texColor.rgb, _FlashColor.rgb, _FlashAmount);
                
                // 3. Sample the 2D Light Texture
                float2 lightUV = i.screenPos.xy / i.screenPos.w;
                half4 lightColor = tex2D(_ShapeLightTexture0, lightUV);
                
                // 4. Multiply the sprite by the light
                texColor.rgb *= lightColor.rgb;
                
                return texColor;
            }
            ENDHLSL
        }
    }
}