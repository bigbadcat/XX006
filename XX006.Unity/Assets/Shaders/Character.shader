Shader "Unlit/Character"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}

        [Toggle(_HIDE_EFFECT)] _HideEffect("HideEffect", Int) = 0       //是否开启消失效果
        _HideColor("Hide Color", Color) = (1, 1, 1, 1)                  //消失过渡颜色
        _HideVar("Hide Var", Range(0.01, 1)) = 0.5                      //消失过渡范围
        _HideRate("Hide Rate", Range(0, 1)) = 0                         //消失进度
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 100

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma shader_feature _ _HIDE_EFFECT

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            float4 _HideColor;
            float _HideVar;
            float _HideRate;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                fixed4 col = tex2D(_MainTex, i.uv);

#if defined(_HIDE_EFFECT)
                float hr = _HideRate * (1+_HideVar);
                float hide_value = 1 - (col.r + col.g + col.b) / 3;
                float r = hide_value - hr + _HideVar;
                clip(r);
                col = lerp(_HideColor, col, saturate(r/_HideVar));
#endif
                return col;
            }
            ENDCG
        }
    }
}
