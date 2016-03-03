Shader "Custom/PlanetShader" {
	Properties {
		_Sand("Sand (RGB)", 2D) = "white" {}
		_Grass("Grass (RGB)", 2D) = "white" {}
		_Rock("Rock (RGB)", 2D) = "white" {}
		_Snow("Snow (RGB)", 2D) = "white" {}
		_Radius("Planet Radius", Float) = 1.0
		_NoiseFactor("Noise Factor", Range(0.0, 1.0)) = 0.35
	}
	SubShader{
		Tags { "RenderType" = "Opaque" }
		LOD 200

		CGPROGRAM
		// Physically based Standard lighting model, and enable shadows on all light types
		#pragma surface surf Standard fullforwardshadows vertex:vert

		// Use shader model 4.0 target, to get nicer looking lighting
		#pragma target 4.0

		struct Input {
			float2 uv_Sand;
			float2 uv_Grass;
			float2 uv_Rock;
			float3 localPos;
			float3 localNormal;
		};

		////// uniform maybe? 

		static const float LEVEL1_MAX_HEIGHT = 0.02; // sand
		static const float LEVEL2_MIN_HEIGHT = 0.08; // grass / rock
		static const float LEVEL2_MAX_HEIGHT = 0.20;
		static const float LEVEL3_MIN_HEIGHT = 0.25; // snow / rock

		static const float LEVEL2_STEP1_MAX_SLOPE = 0.17; // grass
		static const float LEVEL2_STEP2_MIN_SLOPE = 0.21; // rock

		static const float LEVEL3_STEP1_MAX_SLOPE = 0.25; // snow
		static const float LEVEL3_STEP2_MIN_SLOPE = 0.29; // rock

		//////

		sampler2D _Sand;
		sampler2D _Grass;
		sampler2D _Rock;
		sampler2D _Snow;

		half _Radius;
		half _NoiseFactor;

		static const float LEVEL1_2_TRANS_FACTOR = 1.0 / (LEVEL2_MIN_HEIGHT - LEVEL1_MAX_HEIGHT);
		static const float LEVEL2_3_TRANS_FACTOR = 1.0 / (LEVEL3_MIN_HEIGHT - LEVEL2_MAX_HEIGHT);

		static const float LEVEL2_STEP1_2_TRANS_FACTOR = 1.0 / (LEVEL2_STEP2_MIN_SLOPE - LEVEL2_STEP1_MAX_SLOPE);
		static const float LEVEL3_STEP1_2_TRANS_FACTOR = 1.0 / (LEVEL3_STEP2_MIN_SLOPE - LEVEL3_STEP1_MAX_SLOPE);

		static const float PI = 3.14159265f;

		void vert(inout appdata_full v, out Input o) {
			UNITY_INITIALIZE_OUTPUT(Input, o);
			o.localPos = v.vertex.xyz;
			o.localNormal = v.normal.xyz;
		}

		float3 normalAlbedo_level1(Input IN, float slope) {
			return tex2D(_Sand, IN.uv_Sand).rgb; // if we're low enough, return sand independently of the slope
		}

		float3 normalAlbedo_level2(Input IN, float slope) {
			if (slope <= LEVEL2_STEP1_MAX_SLOPE) {
				return tex2D(_Grass, IN.uv_Grass).rgb;
			} else if (slope <= LEVEL2_STEP2_MIN_SLOPE) {
				return lerp(tex2D(_Grass, IN.uv_Grass).rgb, tex2D(_Rock, IN.uv_Rock).rgb, (slope - LEVEL2_STEP1_MAX_SLOPE) * LEVEL2_STEP1_2_TRANS_FACTOR);
			} else {
				return tex2D(_Rock, IN.uv_Rock).rgb;
			}
		}

		float3 normalAlbedo_level3(Input IN, float slope) {
			if (slope <= LEVEL3_STEP1_MAX_SLOPE) {
				return tex2D(_Snow, IN.uv_Grass).rgb;
			} else if (slope <= LEVEL3_STEP2_MIN_SLOPE) {
				return lerp(tex2D(_Snow, IN.uv_Grass).rgb, tex2D(_Rock, IN.uv_Rock).rgb, (slope - LEVEL3_STEP1_MAX_SLOPE) * LEVEL3_STEP1_2_TRANS_FACTOR);
			} else {
				return tex2D(_Rock, IN.uv_Rock).rgb;
			}
		}

		float3 levelAlbedo(Input IN, float height, float slope) {
			if (height <= LEVEL1_MAX_HEIGHT) {
				return normalAlbedo_level1(IN, slope);
			} else if (height <= LEVEL2_MIN_HEIGHT) {
				return lerp(normalAlbedo_level1(IN, slope), normalAlbedo_level2(IN, slope), (height - LEVEL1_MAX_HEIGHT) * LEVEL1_2_TRANS_FACTOR);
			} else if (height <= LEVEL2_MAX_HEIGHT) {
				return normalAlbedo_level2(IN, slope);
			} else if (height <= LEVEL3_MIN_HEIGHT) {
				return lerp(normalAlbedo_level2(IN, slope), normalAlbedo_level3(IN, slope), (height - LEVEL2_MAX_HEIGHT) * LEVEL2_3_TRANS_FACTOR);
			} else {
				return normalAlbedo_level3(IN, slope);
			}
			return 0.0;
		}

		void surf(Input IN, inout SurfaceOutputStandard o) {
			float3 rDir = normalize(IN.localPos); 

			float height = (dot(rDir, IN.localPos) - _Radius) / (_Radius * _NoiseFactor); // get height in range [-1, 1]
			float slope = 1.0 - dot(rDir, IN.localNormal);
			
			//o.Albedo.rgb = height <= LEVEL1_MAX_HEIGHT ? float3(1, 0, 0) : float3(0, 0, 1);//levelAlbedo(IN, height, slope);
			o.Albedo.rgb = levelAlbedo(IN, height, slope);
			o.Metallic = 0;
			o.Smoothness = 1;
		}
		ENDCG
	}
	FallBack "Diffuse"
}
