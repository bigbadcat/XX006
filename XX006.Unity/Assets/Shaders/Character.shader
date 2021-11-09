Shader "Unlit/Character"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _Emissive("emissive", Range(0, 1)) = 0
        _Diffuse("diffuse",Color) = (1,1,1,1)
        _DiffuseRate("diffuse rate", Range(0, 1)) = 1
        _Specular("specular",Color) = (1,1,1,1)
		_Gloss("gloss",Range(1,256)) = 20
        _Tooniness("_Tooniness", Range(1, 20)) = 1

        [Toggle(_HIDE_EFFECT)] _HideEffect("HideEffect", Int) = 0       //是否开启消失效果
        _HideColor("hide color", Color) = (1, 1, 1, 1)                  //消失过渡颜色
        _HideVar("hide var", Range(0.01, 1)) = 0.5                      //消失过渡范围
        _HideRate("hide rate", Range(0, 1)) = 0                         //消失进度
    }

    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 100

        Pass
        {
            Tags{"LightMode"="ForwardBase"}
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_fwdbase
            #pragma shader_feature _ _HIDE_EFFECT

            #include "UnityCG.cginc"
            #include "Lighting.cginc"
			#include "AutoLight.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
                float3 normal : NORMAL;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                float2 uv : TEXCOORD0;
                float3 normal : NORMAL;
                SHADOW_COORDS(2)            // 2 即 TEXCOORD2
                float3 vertex_world : TEXCOORD3;        
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            float _Emissive;
            fixed4 _Diffuse;
            float _DiffuseRate;
            fixed4 _Specular;
			float _Gloss;
			float _Tooniness;

            float3 _HideColor;
            float _HideVar;
            float _HideRate;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                o.normal = UnityObjectToWorldNormal(v.normal);
                o.vertex_world = mul(UNITY_MATRIX_M, v.vertex).xyz;
                TRANSFER_SHADOW(o);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                fixed3 col = tex2D(_MainTex, i.uv);
                //col = floor(col * _Tooniness)/_Tooniness;

#if defined(_HIDE_EFFECT)
                float hr = _HideRate * (1+_HideVar);
                float hide_value = 1 - (col.r + col.g + col.b) / 3;
                float r = hide_value - hr + _HideVar;
                clip(r);
                col = lerp(_HideColor, col, saturate(r/_HideVar));
#endif

                //自发光
                fixed3 _tex_color = col * _Diffuse;
                fixed3 self = _tex_color * _Emissive;

                //环境光                
				fixed3 ambient = UNITY_LIGHTMODEL_AMBIENT.xyz * _tex_color;

                //漫反射				
				float3 worldNormal = normalize(i.normal);
				float3 worldLightDir = normalize(UnityWorldSpaceLightDir(i.vertex_world));
				float rate = saturate(dot(worldNormal, worldLightDir));
                //rate = floor(rate * _Tooniness)/_Tooniness;
                float keep = rate + (1 - rate) * _DiffuseRate;
				fixed3 diffuse = _LightColor0.rgb * _tex_color * keep;

                ////高光
				//float3 viewDir = normalize(_WorldSpaceCameraPos.xyz - i.vertex_world);
				//fixed3 halfDir = normalize(viewDir + worldLightDir);
				//fixed3 specular = _LightColor0.rgb*_Specular.rgb*pow(saturate(dot(worldNormal, halfDir)), _Gloss);

                //阴影
                //fixed atten = SHADOW_ATTENUATION(i);
                fixed atten = SHADOW_ATTENUATION(i) * rate;         //乘以漫反射系数，可以让阴影只影响向阳面(背阳面系数为0)
                return float4(self + ambient + diffuse * atten, 1);
                //return float4(self + ambient + (diffuse + specular) * atten, 1);
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
            #pragma shader_feature _ _HIDE_EFFECT

			#include "UnityCG.cginc"

			struct v2fShadow {
                float2 uv : TEXCOORD1;
				V2F_SHADOW_CASTER;
			};

            sampler2D _MainTex;
            float4 _MainTex_ST;
            float _HideVar;
            float _HideRate;

			v2fShadow vertShadow(appdata_base v) {
				v2fShadow o;
                o.uv = TRANSFORM_TEX(v.texcoord, _MainTex);
				TRANSFER_SHADOW_CASTER_NORMALOFFSET(o);
				return o;
			}

			fixed4 fragShadow(v2fShadow i) : SV_Target{
#if defined(_HIDE_EFFECT)
                fixed4 col = tex2D(_MainTex, i.uv);
                float hr = _HideRate * (1+_HideVar);
                float hide_value = 1 - (col.r + col.g + col.b) / 3;
                float r = hide_value - hr + _HideVar;
                clip(r);
#endif

				SHADOW_CASTER_FRAGMENT(i);
			}
			ENDCG
		}
    }
}
