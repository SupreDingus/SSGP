Shader "Unlit/ShadowCaster_Box"
{
	Properties
	{
	}
	SubShader
	{
		Tags { "RenderType"="Opaque" }
		LOD 100

		Cull Off
		ZTest LEqual
		ZWrite On

		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma geometry geom
			#pragma multi_compile_instancing
			
			#include "UnityCG.cginc"

			struct appdata
			{
				float4 vertex : POSITION;
				float4 normal : NORMAL;
				float2 uv : TEXCOORD0;
				UNITY_VERTEX_INPUT_INSTANCE_ID
			};

			struct FragOut
			{
				half4 color : SV_Target;
				float depth : SV_DEPTH;
			};

			struct v2f
			{
				float4 vertex : SV_POSITION;
				float4 localPosition : TEXCOORD0;
				float4 baseVertex : TEXCOORD1;
				float2 angle : TEXCOORD2;
				float3 normal : TEXCOORD3;
			};

			float _Radius;
			float4 _lightPosition;
			float4 _TexelSize;
			float _Column;

#define TWOPI 6.283185f
#define PI 3.141592f
#define HALFPI 1.570796f
#define NEAR 0.001f
#define FAR _Radius

#define COLUMNWIDTH 3.0f
#define HALFWIDTH 1.5f

			float NormalizeDepth(float d)
			{
				return 1.0f - d / FAR;
			}

			float3 GetVertFromAngle(v2f input)
			{
				float angleFraction = input.angle.y / PI;

				float dist = input.localPosition.w / _Radius;

				float mid = _Column * COLUMNWIDTH + HALFWIDTH;

				return float3(_TexelSize.x * (mid + COLUMNWIDTH * input.baseVertex.z) * 2.0f - 1.0f, 1.0f - angleFraction, 0.0f);
			}

			[maxvertexcount(6)]
			void geom(triangle v2f input[3], inout TriangleStream<v2f> outStream)
			{
				//Base triangle where neither value is increased

				float2 angDeltaA = abs(input[0].angle - input[1].angle);
				float2 angDeltaB = abs(input[1].angle - input[2].angle);

				float2 angDelta = (angDeltaA.x <= 0.000001f) ? angDeltaB : angDeltaA;

				//If the difference in angles is somehow greater than 180 degrees, set all angles greater than PI to their negative counterpart
				if (angDelta.y > PI)
				{
					input[0].angle.y = input[0].angle.x;
					input[1].angle.y = input[1].angle.x;
					input[2].angle.y = input[2].angle.x;

					input[0].vertex = float4(GetVertFromAngle(input[0]), 1.0f);
					input[1].vertex = float4(GetVertFromAngle(input[1]), 1.0f);
					input[2].vertex = float4(GetVertFromAngle(input[2]), 1.0f);

					outStream.Append(input[0]);
					outStream.Append(input[1]);
					outStream.Append(input[2]);

					outStream.RestartStrip();

					input[0].angle.y += TWOPI;
					input[1].angle.y += TWOPI;
					input[2].angle.y += TWOPI;

					input[0].vertex = float4(GetVertFromAngle(input[0]), 1.0f);
					input[1].vertex = float4(GetVertFromAngle(input[1]), 1.0f);
					input[2].vertex = float4(GetVertFromAngle(input[2]), 1.0f);
				}

				outStream.Append(input[0]);
				outStream.Append(input[1]);
				outStream.Append(input[2]);
			}

			v2f vert (appdata v)
			{
				UNITY_SETUP_INSTANCE_ID(v);

				v2f o;

				o.baseVertex = v.vertex;
				o.baseVertex.xyz *= 0.5f;

				o.localPosition = mul(unity_ObjectToWorld, o.baseVertex) - _lightPosition;


				float2 flatDirection = o.localPosition.xy;

				float distance = length(flatDirection);
				flatDirection /= distance;

				o.angle.x = atan2(flatDirection.y, flatDirection.x);
				o.angle.y = (o.angle.x < 0.0f) ? (TWOPI + o.angle.x) : o.angle.x;

				//o.normal = v.normal.xyz;

				o.normal.xy = normalize(mul(unity_ObjectToWorld, float4(v.normal.xyz, 0.0f)).xy);
				o.normal.z = dot(o.normal.xy, o.localPosition.xy);

				//o.vertex = UnityObjectToClipPos(v.vertex);
				o.vertex = float4(GetVertFromAngle(o), 1.0f);
				//o.vertex = float4(v.vertex.z * 0.2f, v.vertex.y * 0.2f, 0.0f, 1.0f);
				return o;
			}
			
			FragOut frag (v2f i)
			{
				FragOut o;

				float2 dir = float2(cos(i.angle.y), sin(i.angle.y));

				float t = i.normal.z / dot(dir, i.normal.xy);

				//t = i.angle.y / TWOPI;

				o.color = float4(t, 0.0f, 0.0f, t);
				//o.color = float4((i.normal.xy), 0.0f, t);
				//o.color = float4(1.0f, 1.0f, 0.0f, 0.0f);
				o.depth = NormalizeDepth(t);
				//o.depth = 0.5f;

				return o;
			}
			ENDCG
		}
	}
}
