Shader "Light/PointLight"
{
	Properties
	{
		_MainTex ("Texture", 2D) = "white" {}
	}
	SubShader
	{
		Tags { "RenderType"="Transparent" "LightMode"="Light" }
		LOD 100

		Blend One One
		Cull Off
		ZTest Always
		ZWrite Off

		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma multi_compile_instancing
			#pragma multi_compile EDITOR GAME
			
			#include "UnityCG.cginc"
			#include "UnityPBSLighting.cginc"

			struct appdata
			{
				float4 vertex : POSITION;
				float2 uv : TEXCOORD0;
				UNITY_VERTEX_INPUT_INSTANCE_ID
			};

			struct v2f
			{
				float2 uv : TEXCOORD0;
				float4 vertex : SV_POSITION;
				float2 screenUV : TEXCOORD1;
				float3 worldPos : TEXCOORD2;
				UNITY_VERTEX_INPUT_INSTANCE_ID
			};

			sampler2D _MainTex;

			sampler2D _GBuffer0;
			sampler2D _GBuffer1;
			sampler2D _GBuffer2;
			float4 _LightColor[256];
			float _FlipY;
			float _ShadowBleedDistance;
			float4 _TexelSize;
			float4x4 _UVZtoWORLD;

#define TWOPI 6.283185f
#define PI 3.141592f
#define HALFPI 1.570796f
#define NEAR 0.001f
#define FAR _Radius

#define COLUMNWIDTH 3.0f
#define HALFWIDTH 1.5f

			float2 GetUVFromAngle(float angle, float _Column)
			{
				float angleFraction = angle / TWOPI;
				float mid = _Column * COLUMNWIDTH + HALFWIDTH;

				return float2(_TexelSize.x * (mid), angleFraction);
			}

			inline float4 GetUVZ(float2 uv, float4 gb1)
			{
				return float4(uv.x * 2.0f - 1.0f, uv.y * 2.0f - 1.0f, gb1.w, 1.0f);
			}
			
			v2f vert (appdata v, uint id : SV_InstanceID)
			{
				v2f o;

				UNITY_SETUP_INSTANCE_ID(v);
				UNITY_TRANSFER_INSTANCE_ID(v, o);

				o.vertex = UnityObjectToClipPos(v.vertex);

				o.screenUV = (o.vertex.xy + 1.0f) * 0.5f;

				//o.screenUV.y = lerp(o.screenUV.y, 1.0f - o.screenUV.y, _FlipY);
				o.screenUV.y = 1.0f - o.screenUV.y;

				o.worldPos = (mul(unity_ObjectToWorld, float4(0.0f, 0.0f, 0.0f, 1.0f)).xyz);
				//o.worldPos.z = -o.worldPos.z;

				//o.vertex = v.vertex;
				o.uv = v.uv;
				return o;
			} 

			float3 GetWorldPos(float2 uv, float zOffset)
			{
				float2 ndc = uv * 2.0f - 1.0f;

				float2 xyPos = _WorldSpaceCameraPos.xy + unity_OrthoParams.xy * ndc;

				return float3(xyPos, zOffset);
			}

#define PI 3.1415923

			float4 frag (v2f i) : SV_Target
			{
				UNITY_SETUP_INSTANCE_ID(i);

				float4 gb0 = tex2D(_GBuffer0, i.screenUV);
				float4 gb1 = tex2D(_GBuffer1, i.screenUV);
				float4 gb2 = tex2D(_GBuffer2, i.screenUV);

				float3 color = gb0.xyz;
				float3 wNorm = gb1.xyz;
				float2 material = gb2.xy;

				float4 uvz = GetUVZ(i.screenUV, gb1);

				float attenuation = saturate(1.0f - length(i.uv - float2(0.5f, 0.5f)) / 0.5f);
				attenuation = attenuation * attenuation;
#ifdef UNITY_INSTANCING_ENABLED
				//float4 lightColor = float4(0.0f, 1.0f, 0.0f, 0.0f); unity_InstanceID
				float4 lightColor = _LightColor[unity_InstanceID];
				float column = unity_InstanceID;
#else
				float4 lightColor = float4(1.0f, 0.0f, 0.0f, 0.0f);
				float column = 0.0f;
#endif

				float specularValue = lerp(0.04f, 1.0f, material.x);

				float3 specularColor = lerp(float3(0.04f, 0.04f, 0.04f), color, material.x);
				float3 diffuseColor = (1.0f - specularValue) * color;

				//We assume the lights and the camera are on a plane in front of the scene because lighting doesn't
				//really work in 2D, so, like, y'know
				//Gotta extrapolate some info and pretend we're in 3D

				//float3 wPos = GetWorldPos(i.screenUV, 0.0f);
				//float3 wPos = gb1.xyz;
				float3 wPos = mul(_UVZtoWORLD, uvz).xyz;
				float3 camPos = float3(_WorldSpaceCameraPos.xy, -0.5f);

				float3 toLight = (i.worldPos - wPos);
				float worldDistance = length(toLight.xy);
				toLight = normalize(toLight);
				//float3 toCam = normalize(camPos - wPos);
				float3 toCam = float3(0.0f, 0.0f, -1.0f);

				//float3 wNorm = float3(0.0f, 0.0f, -1.0f);

				float3 refVec = reflect(-toCam, wNorm);

				UnityLight lightData;
				lightData.color = lightColor.xyz;
				lightData.dir = toLight;
				UnityIndirect ambient;
				ambient.diffuse = float3(0.0f, 0.0f, 0.0f);
				ambient.specular = float3(0.0f, 0.0f, 0.0f);

				float smoothness = 1.0f - material.y;

				float RdotL = saturate(dot(toLight, refVec));
				float NdotL = saturate(dot(toLight, wNorm));

				float specValue = pow(RdotL, 32 * smoothness);

				half3 diff = diffuseColor * NdotL;
				half3 spec = specularColor * specValue;

				//half3 L = (diff + spec) * lightColor;
				//half3 L = (diff) * lightColor;
				//half3 L = (spec) * lightColor;

				half4 L = UNITY_BRDF_PBS(diffuseColor, specularColor, 1.0f - specularValue, smoothness, wNorm, toCam, lightData, ambient);

				float angle = atan2(-toLight.y, -toLight.x);
				angle = (angle < 0.0f) ? (TWOPI + angle) : angle;

				//float NdotL = dot(toLight, wNorm);

				//NdotL = gb2.w;

				//return float4(NdotL, NdotL, NdotL, 0.0f);

				float2 shmapUV = GetUVFromAngle(angle, column);

				//return float4(shmapUV, 0.0f, 1.0f);

				//float4 shmap = tex2D(_MainTex, i.uv);
				float4 shmap = tex2D(_MainTex, shmapUV);

				//Because we're viewing things from the side, we want the light to sort of bleed into things that are technically behind a shadow caster

				//float shadow = (worldDistance > shmap.x) ? 0.0f : 1.0f;
				
				float delta = worldDistance  - shmap.x;

				float shadow = 1.0f - saturate(max(delta, 0.0f) / _ShadowBleedDistance);

				//return shmap;

				attenuation *= shadow;

				//return float4(wPos, 1.0f);

				//float wew = column * 0.1f;

				//return float4(wew, wew, wew, 0.0f);

				//return float4(i.screenUV, 0.0f, 0.0f);
				//return float4(i.worldPos, 1.0f);
				//return float4(wPos.xyz, 1.0f);
				//return float4(0.10f, 0.0f, 0.0f, 0.0f);
				return float4(L.xyz * attenuation, 0.0f);
			}
			ENDCG
		}
	}
}
