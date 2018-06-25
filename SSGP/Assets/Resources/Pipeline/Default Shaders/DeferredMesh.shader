Shader "DeferredSprite/DeferredMesh"
{
	Properties
	{
		_MainTex ("Texture", 2D) = "white" {}
		_Color ("Color", Color) = (1.0, 1.0, 1.0, 1.0)
		_Metallic("Metallic", Range(0, 1)) = 0.1
		_Roughness("Roughness", Range(0, 1)) = 0.3
	}
	SubShader
	{
		Tags { "RenderType"="Opaque" "LightMode"="DeferredSprite" }
		LOD 100

		Cull Off

		Pass
		{
			Stencil
			{
				Ref 1
				Comp always
				Pass replace
			}

			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			
			#include "UnityCG.cginc"

			struct FragOut
			{
				float4 gb0 : SV_TARGET0;
				float4 gb1 : SV_TARGET1;
				float4 gb2 : SV_TARGET2;
				float4 gb3 : SV_TARGET3; 
			};

			struct appdata
			{
				float4 vertex : POSITION;
				float4 normal : NORMAL;
				float2 uv : TEXCOORD0;
			};

			struct v2f
			{
				float2 uv : TEXCOORD0;
				float4 vertex : SV_POSITION;
				float3 color : TEXCOORD1;
				float3 wNorm : TEXCOORD2;
				float3 wPos : TEXCOORD3;
			};

			sampler2D _MainTex;
			float _Metallic;
			float _Roughness;
			float4 _MainTex_ST;
			float4 _AmbientColor;
			float4 _Color;

			v2f vert (appdata v)
			{
				v2f o;
				o.wPos = mul(unity_ObjectToWorld, v.vertex);
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv = TRANSFORM_TEX(v.uv, _MainTex);
				o.wNorm = (mul(unity_ObjectToWorld, float4(v.normal.xyz, 0.0f))).xyz;
				o.color = _Color;
				return o;
			}
			
			//GBuffer
			//0:	Color.xyz, Material.metalness		ARGB32
			//1 :	Normal.xyz, wPos.z					ARGBFloat
			//2 :	Emission.xyz, Material.roughness	ARGBHalf
			FragOut frag(v2f i)
			{
				FragOut o;

				// sample the texture
				fixed4 col = tex2D(_MainTex, i.uv);

				if (col.w < 0.9f)
					discard;

				o.gb0 = col * float4(i.color.xyz, 0.0f);
				o.gb1 = float4(normalize(i.wNorm), i.wPos.z);
				o.gb2 = float4(_Metallic, _Roughness, 0.0f, 0.0f);
				float3 diffuseColor = o.gb0.xyz * (1.0f - _Metallic);
				o.gb3 = float4(_AmbientColor.xyz * diffuseColor, 0.0f);

				return o;
			}
			ENDCG
		}
	}
}
