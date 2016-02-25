using UnityEngine;
using System.Collections;

public static class FBMNoise {
	public static float valueAt(Vector3 v, int octaves, float lacunarity, float persistance) {
		return valueAt(v.x, v.y, v.z, octaves, lacunarity, persistance);
	}
	
	public static float valueAt(float x, float y, float z, int octaves, float lacunarity, float persistance) {
		float value = 0f, maxValue = 0f;

		for (int i = 0; i < octaves; ++i) {
			float frequency = Mathf.Pow(lacunarity, i);
			float amplitude = Mathf.Pow(persistance, i);
			value += PerlinNoise.valueAt(x * frequency, y * frequency, z * frequency) * amplitude;
			maxValue += amplitude;
		}

		return value / maxValue;
	}
}
