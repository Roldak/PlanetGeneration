﻿Shader "Custom/WaterShader" {
	Properties{
		_WaterColor("Water Color", Color) = (1,1,1,1)

		_NormalMap("Normal Map (RGB)", 2D) = "white" {}
		_NormalCorrection("Normal Correction", Vector) = (1, 1, 1, 1)

		_WaveScale_1_2_3_4("Wave Scale 1 & 2 & 3 & 4", Vector) = (0.5, 0.8, 1, 1)
		_WaveDir_1_2("Direction 1 & 2", Vector) = (0.5, 0.8, 0.5, -0.25)
		_WaveDir_3_4("Direction 3 & 4", Vector) = (0.7, 0.7, 0.7, -0.7)
	}
	SubShader{
		Tags{ "Queue" = "Transparent" "RenderType" = "Transparent" }
		LOD 200

		CGPROGRAM
		// Physically based Standard lighting model, and enable shadows on all light types
		#pragma surface surf Standard fullforwardshadows alpha

		// Use shader model 3.0 target, to get nicer looking lighting
		#pragma target 4.0

		struct Input {
			float2 uv_NormalMap;
			float3 viewDir;
		};

		fixed4 _WaterColor;

		sampler2D _NormalMap;
		float4 _NormalCorrection;

		float4 _WaveScale_1_2_3_4;
		float4 _WaveDir_1_2;
		float4 _WaveDir_3_4;

		float fresnel(float3 incident, float3 normal, float power) {
			return pow(1.0 - dot(incident, normal), power);
		}

		void surf(Input IN, inout SurfaceOutputStandard o) {
			float3 normal =
					UnpackNormal(tex2D(_NormalMap, IN.uv_NormalMap * _WaveScale_1_2_3_4.x + _WaveDir_1_2.xy * _Time.x))
				  + UnpackNormal(tex2D(_NormalMap, IN.uv_NormalMap * _WaveScale_1_2_3_4.y + _WaveDir_1_2.zw * _Time.x))
				  + UnpackNormal(tex2D(_NormalMap, IN.uv_NormalMap * _WaveScale_1_2_3_4.z + _WaveDir_3_4.xy * _Time.x))
				  + UnpackNormal(tex2D(_NormalMap, IN.uv_NormalMap * _WaveScale_1_2_3_4.w + _WaveDir_3_4.zw * _Time.x));

			normal *= _NormalCorrection.xyz;
			normal = normalize(normal);

			float fres = fresnel(normalize(IN.viewDir), normal, 0.5);

			o.Normal = normal;
			o.Albedo = _WaterColor.rgb;
			o.Metallic = 0.5;
			o.Smoothness = 0.5;
			o.Alpha = _WaterColor.a;//(1 - _WaterColor.a) * fres + _WaterColor.a;
		}
		ENDCG
	}
	FallBack "Diffuse"
}
