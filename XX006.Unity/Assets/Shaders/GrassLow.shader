Shader "Unlit/GrassLow"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _Color("Color", Color) = (0, 1, 0, 1)                   //草的颜色

        _ClipAlpha("Clip Alpha", Range(0, 1)) = 0.5             //裁剪的alpha
        _Move("Move", Vector) = (0, 0, 0, 0)                    //偏移值
        _MoveRate("MoveRate", Range(0, 1)) = 0                  //偏移系数
    }

    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 100

        Cull Off

        Pass
        {
            CGPROGRAM
            #pragma target 3.0
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_fwdbase
            #pragma multi_compile_instancing
            #pragma instancing_options procedural:setup

            #include "UnityCG.cginc"
            #include "Lighting.cginc"
			#include "AutoLight.cginc"

            struct instancing_data
            {
                float4x4 trs;
                float4 wind;
            };

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
                float3 normal : NORMAL;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
                SHADOW_COORDS(2)            // 2 即 TEXCOORD2
                float3 normal : NORMAL;
                float3 vertex_world : TEXCOORD3;    
            };

            sampler2D _MainTex;
            float4 _Color;
            float _ClipAlpha;
            float4 _Move;
            float _MoveRate;

            StructuredBuffer<instancing_data> _InstancingBuffer;

            void setup()
            {
            #if defined(UNITY_INSTANCING_ENABLED) || defined(UNITY_PROCEDURAL_INSTANCING_ENABLED) || defined(UNITY_STEREO_INSTANCING_ENABLED)
                instancing_data data = _InstancingBuffer[unity_InstanceID];
                unity_ObjectToWorld = data.trs;
            #endif
            }

            v2f vert (appdata v)
            {
                v2f o;
                UNITY_SETUP_INSTANCE_ID(v);
            #if defined(UNITY_INSTANCING_ENABLED) || defined(UNITY_PROCEDURAL_INSTANCING_ENABLED) || defined(UNITY_STEREO_INSTANCING_ENABLED)
                instancing_data data = _InstancingBuffer[unity_InstanceID];
                float4 wind = data.wind;
            #else
                float4 wind = float4(_Move.xyz, _MoveRate);
            #endif

                float4 vw = mul(unity_ObjectToWorld, v.vertex);
                float3 wpos = vw + (wind.xyz * v.vertex.y * v.vertex.y) * wind.w;
                o.vertex = UnityWorldToClipPos(float4(wpos, 1));
                o.uv = v.uv;
                o.normal = normalize(mul(unity_ObjectToWorld, v.normal));
                o.vertex_world = wpos;

                //阴影
                TRANSFER_SHADOW(o);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                fixed4 col = tex2D(_MainTex, i.uv);
                clip(col.a - _ClipAlpha);

                //环境光                
                fixed3 _tex_color = col.rgb * _Color;
				fixed3 ambient = UNITY_LIGHTMODEL_AMBIENT.xyz * _tex_color;

                //漫反射，背面没有裁剪，法线是反方向
                //导致背面对着光源也不会接受光照
                //但在随机生成的草丛里，能得到更好的效果
                //阳光下的草地不会因为观察朝向光影时有类似背光效果(草整体变暗)
				float3 worldLightDir = normalize(UnityWorldSpaceLightDir(i.vertex_world));
				float rate = saturate(dot(i.normal, worldLightDir));
				fixed3 diffuse = _LightColor0.rgb * _tex_color * rate;

                //阴影
                fixed atten = SHADOW_ATTENUATION(i) * rate;         //乘以漫反射系数，可以让阴影只影响向阳面(背阳面系数为0)
                return float4(ambient + diffuse * atten, 1);
            }
            ENDCG
        }
    }
}
