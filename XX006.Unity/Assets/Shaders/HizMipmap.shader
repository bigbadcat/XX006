Shader "Unlit/HizMipmap"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "black" {}
    }
    SubShader
    {
        Pass
        {
            ZWrite Off
            ZTest Always

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                float2 uv : TEXCOORD0;                
            };

            sampler2D _MainTex;
            float4 _MainTex_TexelSize;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex.xyz);
                o.uv = v.uv;
                return o;
            }

            //获取下一级的深度值
            inline float GetMipmapDepth(float2 uv)
            {
                float4 depth;
                float offset = _MainTex_TexelSize.x;
                depth.x = tex2D(_MainTex, uv);
                depth.y = tex2D(_MainTex, uv - float2(0, offset));
                depth.z = tex2D(_MainTex, uv - float2(offset, 0));
                depth.w = tex2D(_MainTex, uv - float2(offset, offset));
                return min(min(depth.x, depth.y), min(depth.z, depth.w));

                //以下为裁剪判断备注
                //深度值与NDC坐标的z值，不同平台api取值范围和含义都不一样
                //DX下，NDC.z取值0~1表示从近到远，但MVP变换后被Unity反转过的，所以实际时1-0表示近到远(深度图越红越近)
                //OpenGL下，NDC.z取值-1~1表示从近到远，depth * 0.5f + 0.5f转为0~1与纹理值进行比较(深度图越黑越近)
                //PS：Unity2020.3.20下没有进行平台区分同样的代码都能正常运作(Editor Android)
            }

            fixed4 frag (v2f i) : SV_Target
            {
                float depth = GetMipmapDepth(i.uv);
                return float4(depth, 0, 0, 1.0f);
            }
            ENDCG
        }
    }
}