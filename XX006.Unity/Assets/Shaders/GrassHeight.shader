//
//绘制草的shader，需要配合DrawMeshInstancedIndirect传入_InstancingBuffer一起才能正常使用
//
Shader "Unlit/GrassHeight"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _Color("Color", Color) = (0, 1, 0, 1)                   //草的颜色

        _ClipAlpha("Clip Alpha", Range(0, 1)) = 0.5             //裁剪的alpha
        _Move("Move", Vector) = (0, 0, 0, 0)                    //偏移值
        _MoveRate("MoveRate", Range(0, 1)) = 0                  //偏移系数

        _Bend("Move", Vector) = (0, 0, 0, 0)                    //压弯值
        _BendRate("MoveRate", Range(0, 1)) = 0                  //压弯系数
    }

    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 100

        Cull Off

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_fwdbase
            #pragma multi_compile_instancing

            #include "UnityCG.cginc"
            #include "Lighting.cginc"
			#include "AutoLight.cginc"

            struct instancing_data
            {
                float4x4 trs;
                float4 wind;
                float4 bend;
            };

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
                float3 normal : NORMAL;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
                SHADOW_COORDS(2)            // 2 即 TEXCOORD2
                fixed4 diffuse : TEXCOORD3;
            };

            sampler2D _MainTex;
            float4 _Color;
            float _ClipAlpha;
            float4 _Move;
            float _MoveRate;
            float4 _Bend;
            float _BendRate;

            StructuredBuffer<instancing_data> _InstancingBuffer;

            v2f vert (appdata v, uint instanceID : SV_InstanceID)
            {
                v2f o;
                float4 wind = float4(_Move.xyz, _MoveRate);
                float4 bend = float4(_Bend.xyz, _BendRate);
                instancing_data data = _InstancingBuffer[instanceID];
                unity_ObjectToWorld = data.trs;
                wind = data.wind;
                bend = data.bend;

                float4 vw = mul(unity_ObjectToWorld, v.vertex);
                float3 wpos = vw + (wind.xyz * v.vertex.y * v.vertex.y) * wind.w + (bend.xyz * v.vertex.y) * bend.w;
                o.vertex = UnityWorldToClipPos(float4(wpos, 1));
                o.uv = v.uv;

                //漫反射，背面没有裁剪，法线是反方向
                //导致背面对着光源也不会接受光照
                //但在随机生成的草丛里，能得到更好的效果
                //阳光下的草地不会因为观察朝向光影时有类似背光效果(草整体变暗)
				float3 worldLightDir = normalize(UnityWorldSpaceLightDir(wpos));
                float3 normal = normalize(mul(unity_ObjectToWorld, v.normal));
				float rate = saturate(dot(normal, worldLightDir));
				o.diffuse = float4(_LightColor0.rgb * rate, rate);

                //阴影
                TRANSFER_SHADOW(o);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                fixed4 col = tex2D(_MainTex, i.uv);
                clip(col.a - _ClipAlpha);

                fixed3 _tex_color = col * _Color;

                //环境光
				fixed3 ambient = UNITY_LIGHTMODEL_AMBIENT.xyz * _tex_color;

                //漫反射				
				fixed3 diffuse = i.diffuse.rgb * _tex_color;

                //阴影
                fixed atten = SHADOW_ATTENUATION(i) * i.diffuse.a;         //乘以漫反射系数，可以让阴影只影响向阳面(背阳面系数为0)
                return float4(ambient + diffuse * atten, 1);
            }
            ENDCG
        }

        Pass
		{
			Name "ShadowCaster"
			Tags{ "LightMode" = "ShadowCaster" }

			CGPROGRAM
			#pragma vertex vertShadow
			#pragma fragment fragShadow
			#pragma multi_compile_shadowcaster
            #pragma multi_compile_instancing

			#include "UnityCG.cginc"

            struct instancing_data
            {
                float4x4 trs;
                float4 wind;
                float4 bend;
            };

            struct appdata
            {
                float4 vertex : POSITION;
                float2 texcoord : TEXCOORD0;
                float3 normal : NORMAL;
            };

			struct v2fShadow {
                float2 uv : TEXCOORD1;
				V2F_SHADOW_CASTER;
			};

            sampler2D _MainTex;
            float _ClipAlpha;
            float4 _Move;
            float _MoveRate;
            float4 _Bend;
            float _BendRate;

            StructuredBuffer<instancing_data> _InstancingBuffer;

            //自定义UnityApplyLinearShadowBias，不使用摄像机的Bias，而是固定使用0
            //以为草是贴着地面的面，摄像机的Bias会造成阴影与投影模型有一定分离，造成接地部分不连接，产生光亮的缝隙
            float4 UnityApplyLinearShadowBias_XX(float4 clipPos)
            {
                #if !(defined(SHADOWS_CUBE) && defined(SHADOWS_CUBE_IN_DEPTH_TEX))
                    #if defined(UNITY_REVERSED_Z)
                        //clipPos.z += max(-1, min(unity_LightShadowBias.x / clipPos.w, 0));        //官方代码
                        clipPos.z += max(-1, min(0 / clipPos.w, 0));
                    #else
                        //clipPos.z += saturate(unity_LightShadowBias.x/clipPos.w);     //官方代码
                        clipPos.z += saturate(0/clipPos.w);
                    #endif
                #endif

                #if defined(UNITY_REVERSED_Z)
                    float clamped = min(clipPos.z, clipPos.w*UNITY_NEAR_CLIP_VALUE);
                #else
                    float clamped = max(clipPos.z, clipPos.w*UNITY_NEAR_CLIP_VALUE);
                #endif
                    clipPos.z = lerp(clipPos.z, clamped, unity_LightShadowBias.y);
                    return clipPos;
            }

			v2fShadow vertShadow(appdata v, uint instanceID : SV_InstanceID)
            {
				v2fShadow o;
                float4 wind = float4(_Move.xyz, _MoveRate);
                float4 bend = float4(_Bend.xyz, _BendRate);
                instancing_data data = _InstancingBuffer[instanceID];
                unity_ObjectToWorld = data.trs;
                wind = data.wind;
                bend = data.bend;

                float4 vw = mul(unity_ObjectToWorld, v.vertex);
                float3 wpos = vw + (wind.xyz * v.vertex.y * v.vertex.y) * wind.w + (bend.xyz * v.vertex.y) * bend.w;
                
                //重写官方TRANSFER_SHADOW_CASTER_NORMALOFFSET的宏和相关函数
                //主要是变换矩阵通过data.trs获得，官方代码中的模型空间与世界空间的相关变换都无效了
                if (unity_LightShadowBias.z != 0.0)
                {
                    float3 wNormal = normalize(mul(unity_ObjectToWorld, v.normal));
                    float3 wLight = normalize(UnityWorldSpaceLightDir(wpos));
                    float shadowCos = dot(wNormal, wLight);
                    float shadowSine = sqrt(1-shadowCos*shadowCos);
                    float normalBias = unity_LightShadowBias.z * shadowSine;
                    wpos -= wNormal * normalBias;
                }
                o.pos = mul(UNITY_MATRIX_VP, float4(wpos, 1));
                o.pos = UnityApplyLinearShadowBias_XX(o.pos);
                o.uv = v.texcoord;
				return o;
			}

			fixed4 fragShadow(v2fShadow i) : SV_Target
            {
                fixed4 col = tex2D(_MainTex, i.uv);
                clip(col.a - _ClipAlpha);
				SHADOW_CASTER_FRAGMENT(i);
			}
			ENDCG
		}
    }
}
