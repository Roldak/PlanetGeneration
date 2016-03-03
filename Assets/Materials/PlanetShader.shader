Shader "Custom/PlanetShader" {
	Properties {
		_Grass ("Grass (RGB)", 2D) = "white" {}
		_Rock("Rock (RGB)", 2D) = "white" {}
		_Glossiness ("Smoothness", Range(0,1)) = 0.5
		_Metallic ("Metallic", Range(0,1)) = 0.0
	}
	SubShader {
		Tags { "RenderType"="Opaque" }
		LOD 200
		
		CGPROGRAM
		// Physically based Standard lighting model, and enable shadows on all light types
		#pragma surface surf Standard fullforwardshadows vertex:vert

		// Use shader model 4.0 target, to get nicer looking lighting
		#pragma target 4.0

		sampler2D _Grass;
		sampler2D _Rock;
		half _Glossiness;
		half _Metallic;

		static const float PI = 3.14159265f;

		struct Input {
			float2 uv_Grass;
			float2 uv_Rock;
			float3 localPos;
			float3 localNormal;
		};

		void vert(inout appdata_full v, out Input o) {
			UNITY_INITIALIZE_OUTPUT(Input, o);
			o.localPos = v.vertex.xyz;
			o.localNormal = v.normal.xyz;
		}

		void surf (Input IN, inout SurfaceOutputStandard o) {
			// Albedo comes from a texture tinted by color
			//fixed4 c = tex2D (_MainTex, IN.uv_MainTex);
			float3 perp = normalize(IN.localPos); 
			float slope = 1.0 - dot(perp, normalize(IN.localNormal));
			
			o.Albedo.rgb = slope > 0.17 ? tex2D(_Rock, IN.uv_Rock) : tex2D(_Grass, IN.uv_Grass);
			// Metallic and smoothness come from slider variables
			o.Metallic = _Metallic;
			o.Smoothness = _Glossiness;
			//o.Alpha = c.a;
		}
		ENDCG
	}
	FallBack "Diffuse"
}
