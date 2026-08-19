Shader "UI/Class Video Blur"
{
    Properties
    {
        [PerRendererData] _MainTex ("Texture", 2D) = "white" {}
        _Color ("Tint", Color) = (1,1,1,1)
        _BlurSize ("Blur Size", Range(0, 8)) = 3
        [HideInInspector] _StencilComp ("Stencil Comparison", Float) = 8
        [HideInInspector] _Stencil ("Stencil ID", Float) = 0
        [HideInInspector] _StencilOp ("Stencil Operation", Float) = 0
        [HideInInspector] _StencilWriteMask ("Stencil Write Mask", Float) = 255
        [HideInInspector] _StencilReadMask ("Stencil Read Mask", Float) = 255
        [HideInInspector] _ColorMask ("Color Mask", Float) = 15
    }

    SubShader
    {
        Tags
        {
            "Queue" = "Transparent"
            "IgnoreProjector" = "True"
            "RenderType" = "Transparent"
            "PreviewType" = "Plane"
            "CanUseSpriteAtlas" = "True"
        }

        Stencil
        {
            Ref [_Stencil]
            Comp [_StencilComp]
            Pass [_StencilOp]
            ReadMask [_StencilReadMask]
            WriteMask [_StencilWriteMask]
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
            #include "UnityCG.cginc"

            struct appdata_t
            {
                float4 vertex : POSITION;
                float4 color : COLOR;
                float2 texcoord : TEXCOORD0;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                fixed4 color : COLOR;
                float2 texcoord : TEXCOORD0;
            };

            sampler2D _MainTex;
            float4 _MainTex_TexelSize;
            fixed4 _Color;
            float _BlurSize;

            v2f vert(appdata_t input)
            {
                v2f output;
                output.vertex = UnityObjectToClipPos(input.vertex);
                output.texcoord = input.texcoord;
                output.color = input.color * _Color;
                return output;
            }

            fixed4 frag(v2f input) : SV_Target
            {
                float2 offset = _MainTex_TexelSize.xy * _BlurSize;
                fixed4 color = tex2D(_MainTex, input.texcoord) * 0.20;
                color += tex2D(_MainTex, input.texcoord + float2(offset.x, 0)) * 0.12;
                color += tex2D(_MainTex, input.texcoord - float2(offset.x, 0)) * 0.12;
                color += tex2D(_MainTex, input.texcoord + float2(0, offset.y)) * 0.12;
                color += tex2D(_MainTex, input.texcoord - float2(0, offset.y)) * 0.12;
                color += tex2D(_MainTex, input.texcoord + offset) * 0.08;
                color += tex2D(_MainTex, input.texcoord - offset) * 0.08;
                color += tex2D(_MainTex, input.texcoord + float2(offset.x, -offset.y)) * 0.08;
                color += tex2D(_MainTex, input.texcoord + float2(-offset.x, offset.y)) * 0.08;
                return color * input.color;
            }
            ENDCG
        }
    }
}
