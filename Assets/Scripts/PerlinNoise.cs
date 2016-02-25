using UnityEngine;
using System.Collections;

/*
 * REFERENCE : http://catlikecoding.com/unity/tutorials/noise/
 */

public static class PerlinNoise {
	private static int[] PERM = {
		151,160,137, 91, 90, 15,131, 13,201, 95, 96, 53,194,233,  7,225,
		140, 36,103, 30, 69,142,  8, 99, 37,240, 21, 10, 23,190,  6,148,
		247,120,234, 75,  0, 26,197, 62, 94,252,219,203,117, 35, 11, 32,
		57,177, 33, 88,237,149, 56, 87,174, 20,125,136,171,168, 68,175,
		74,165, 71,134,139, 48, 27,166, 77,146,158,231, 83,111,229,122,
		60,211,133,230,220,105, 92, 41, 55, 46,245, 40,244,102,143, 54,
		65, 25, 63,161,  1,216, 80, 73,209, 76,132,187,208, 89, 18,169,
		200,196,135,130,116,188,159, 86,164,100,109,198,173,186,  3, 64,
		52,217,226,250,124,123,  5,202, 38,147,118,126,255, 82, 85,212,
		207,206, 59,227, 47, 16, 58, 17,182,189, 28, 42,223,183,170,213,
		119,248,152,  2, 44,154,163, 70,221,153,101,155,167, 43,172,  9,
		129, 22, 39,253, 19, 98,108,110, 79,113,224,232,178,185,112,104,
		218,246, 97,228,251, 34,242,193,238,210,144, 12,191,179,162,241,
		81, 51,145,235,249, 14,239,107, 49,192,214, 31,181,199,106,157,
		184, 84,204,176,115,121, 50, 45,127,  4,150,254,138,236,205, 93,
		222,114, 67, 29, 24, 72,243,141,128,195, 78, 66,215, 61,156,180,
		
		151,160,137, 91, 90, 15,131, 13,201, 95, 96, 53,194,233,  7,225,
		140, 36,103, 30, 69,142,  8, 99, 37,240, 21, 10, 23,190,  6,148,
		247,120,234, 75,  0, 26,197, 62, 94,252,219,203,117, 35, 11, 32,
		57,177, 33, 88,237,149, 56, 87,174, 20,125,136,171,168, 68,175,
		74,165, 71,134,139, 48, 27,166, 77,146,158,231, 83,111,229,122,
		60,211,133,230,220,105, 92, 41, 55, 46,245, 40,244,102,143, 54,
		65, 25, 63,161,  1,216, 80, 73,209, 76,132,187,208, 89, 18,169,
		200,196,135,130,116,188,159, 86,164,100,109,198,173,186,  3, 64,
		52,217,226,250,124,123,  5,202, 38,147,118,126,255, 82, 85,212,
		207,206, 59,227, 47, 16, 58, 17,182,189, 28, 42,223,183,170,213,
		119,248,152,  2, 44,154,163, 70,221,153,101,155,167, 43,172,  9,
		129, 22, 39,253, 19, 98,108,110, 79,113,224,232,178,185,112,104,
		218,246, 97,228,251, 34,242,193,238,210,144, 12,191,179,162,241,
		81, 51,145,235,249, 14,239,107, 49,192,214, 31,181,199,106,157,
		184, 84,204,176,115,121, 50, 45,127,  4,150,254,138,236,205, 93,
		222,114, 67, 29, 24, 72,243,141,128,195, 78, 66,215, 61,156,180
	};
	private static int PERM_MAX_VAL = 255;

	private static Vector3[] GRADIENTS = new Vector3[] {
		new Vector3( 1f, 1f, 0f),
		new Vector3(-1f, 1f, 0f),
		new Vector3( 1f,-1f, 0f),
		new Vector3(-1f,-1f, 0f),
		new Vector3( 1f, 0f, 1f),
		new Vector3(-1f, 0f, 1f),
		new Vector3( 1f, 0f,-1f),
		new Vector3(-1f, 0f,-1f),
		new Vector3( 0f, 1f, 1f),
		new Vector3( 0f,-1f, 1f),
		new Vector3( 0f, 1f,-1f),
		new Vector3( 0f,-1f,-1f),
		
		new Vector3( 1f, 1f, 0f),
		new Vector3(-1f, 1f, 0f),
		new Vector3( 0f,-1f, 1f),
		new Vector3( 0f,-1f,-1f)
	};
	private static int GRADIENTS_MAX_INDEX = 15;
	
	public static float valueAt(Vector3 v) {
		return valueAt(v.x, v.y, v.z);
	}

	public static float valueAt(float x, float y, float z) {
		int X0 = Mathf.FloorToInt(x);
		int Y0 = Mathf.FloorToInt(y);
		int Z0 = Mathf.FloorToInt(z);
		int X1 = X0 + 1;
		int Y1 = Y0 + 1;
		int Z1 = Z0 + 1;

		float xFrac0 = x - X0;
		float yFrac0 = y - Y0;
		float zFrac0 = z - Z0;
		float xFrac1 = xFrac0 - 1f;
		float yFrac1 = yFrac0 - 1f;
		float zFrac1 = zFrac0 - 1f;

		X0 &= PERM_MAX_VAL; Y0 &= PERM_MAX_VAL; Z0 &= PERM_MAX_VAL;
		X1 &= PERM_MAX_VAL; Y1 &= PERM_MAX_VAL; Z1 &= PERM_MAX_VAL;

		int h0 = PERM[X0], h1 = PERM[X1];
		int h00 = PERM[h0 + Y0], h01 = PERM[h0 + Y1], h10 = PERM[h1 + Y0], h11 = PERM[h1 + Y1];
		int h000 = PERM[h00 + Z0], h001 = PERM[h00 + Z1], h010 = PERM[h01 + Z0], h011 = PERM[h01 + Z1], 
			h100 = PERM[h10 + Z0], h101 = PERM[h10 + Z1], h110 = PERM[h11 + Z0], h111 = PERM[h11 + Z1];

		float v000 = Dot(GRADIENTS[h000 & GRADIENTS_MAX_INDEX], xFrac0, yFrac0, zFrac0);
		float v001 = Dot(GRADIENTS[h001 & GRADIENTS_MAX_INDEX], xFrac0, yFrac0, zFrac1);
		float v010 = Dot(GRADIENTS[h010 & GRADIENTS_MAX_INDEX], xFrac0, yFrac1, zFrac0);
		float v011 = Dot(GRADIENTS[h011 & GRADIENTS_MAX_INDEX], xFrac0, yFrac1, zFrac1);
		float v100 = Dot(GRADIENTS[h100 & GRADIENTS_MAX_INDEX], xFrac1, yFrac0, zFrac0);
		float v101 = Dot(GRADIENTS[h101 & GRADIENTS_MAX_INDEX], xFrac1, yFrac0, zFrac1);
		float v110 = Dot(GRADIENTS[h110 & GRADIENTS_MAX_INDEX], xFrac1, yFrac1, zFrac0);
		float v111 = Dot(GRADIENTS[h111 & GRADIENTS_MAX_INDEX], xFrac1, yFrac1, zFrac1);

		float tx = Smooth(xFrac0);
		float ty = Smooth(yFrac0);
		float tz = Smooth(zFrac0);

		return Mathf.Lerp(
			Mathf.Lerp(Mathf.Lerp(v000, v100, tx), Mathf.Lerp(v010, v110, tx), ty),
			Mathf.Lerp(Mathf.Lerp(v001, v101, tx), Mathf.Lerp(v011, v111, tx), ty),
			tz);
	}

	private static float Smooth (float t) {
		return t * t * t * (t * (t * 6f - 15f) + 10f);
	}

	private static float Dot (Vector3 v, float x, float y, float z) {
		return v.x * x + v.y * y + v.z * z;
	}
}
