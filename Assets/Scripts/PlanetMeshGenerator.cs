using UnityEngine;
using System;
using System.Collections;
using System.Diagnostics;

[RequireComponent (typeof(MeshFilter))]
[RequireComponent (typeof(MeshRenderer))]
public class PlanetMeshGenerator : MonoBehaviour {
	public float radius = 1f;
	public int X = 100;
	public int Y = 100;

	[Range(0, 1)]
	public float noiseMagnitude = 0.5f;
	public float noiseScale = 30f;
	public Vector3 noiseOffset = Vector3.zero;
	
	[Range(1, 8)]
	public int octaves = 5;
	public float lacunarity = 2f;
	public float persistance = 0.5f;

	public bool enableMultithreading = true;

	// Use this for initialization
	void Start () {
		Generate();
	}
	
	// Update is called once per frame
	void Update () {
		transform.Rotate(new Vector3(1, 2, 3) * 0.1f);
	}

	public void Generate() {
		// Mesh and Material

		Mesh mesh = new Mesh();
		mesh.subMeshCount = 6;

		Vector3[] vertices = new Vector3[X * Y * 6];
		Vector3[] normals= new Vector3[X * Y * 6];
		Vector2[] uvs = new Vector2[X * Y * 6];
		
		int[][] triangles = new int[6][];
		Color[][] colors = new Color[6][];

		for (int i = 0; i < 6; ++i) {
			triangles[i] = new int[(X - 1) * (Y - 1) * 2 * 3];
			colors[i] = new Color[X * Y];
		}

		MeshGenerator.VertexParametrization[] parametrizations = new MeshGenerator.VertexParametrization[6];

		parametrizations[0] = (float x, float y) => withNoise(centeredNormalizedPosition(x, 1f, y), x, y);
		parametrizations[1] = (float x, float y) => withNoise(centeredNormalizedPosition(y, 0f, x), x, y);
		parametrizations[2] = (float x, float y) => withNoise(centeredNormalizedPosition(1f, y, x), x, y);
		parametrizations[3] = (float x, float y) => withNoise(centeredNormalizedPosition(0f, x, y), x, y);
		parametrizations[4] = (float x, float y) => withNoise(centeredNormalizedPosition(y, x, 1f), x, y);
		parametrizations[5] = (float x, float y) => withNoise(centeredNormalizedPosition(x, y, 0f), x, y);
		
		Stopwatch watch = Stopwatch.StartNew();
		if (enableMultithreading) {
			Parallel.For(0, 6, 1, (int i) => {
				MeshGenerator gen = new MeshGenerator();
				gen.setParameters(X, Y, true, false);
				gen.setVerticesOutputArrays(vertices, normals, uvs, i * X * Y);
				gen.setIndicesOutputArray(triangles[i], 0);
				gen.setColorsOutputArray(colors[i], 0);
				gen.Generate(parametrizations[i]);
			});
		} else {
			MeshGenerator gen = new MeshGenerator();
			gen.setParameters(X, Y, true, false);
			gen.setVerticesOutputArrays(vertices, normals, uvs, 0);

			for (int i = 0; i < 6; ++i) {
				gen.setIndicesOutputArray(triangles[i], 0);
				gen.setColorsOutputArray(colors[i], 0);
				gen.Generate(parametrizations[i]);
			}
		}
		UnityEngine.Debug.Log(watch.Elapsed.TotalSeconds);

		mesh.vertices = vertices;
		mesh.uv = uvs;
		for (int i = 0; i < 6; ++i) {
			mesh.SetTriangles(triangles[i], i);
		}
		mesh.Optimize();
		mesh.RecalculateBounds();
		mesh.normals = normals;
		//mesh.RecalculateNormals();

		GetComponent<MeshFilter>().sharedMesh = mesh;

		// Materials

		Material[] mat = new Material[6];
		for (int i = 0; i < 6; ++i) {
			Texture2D tex = new Texture2D(X, Y);
			tex.SetPixels(colors[i]);
			tex.filterMode = FilterMode.Trilinear;
			tex.wrapMode = TextureWrapMode.Clamp;
			tex.Apply();

			mat[i] = new Material(GetComponent<Renderer>().sharedMaterial);
			mat[i].mainTexture = tex;
		}

		GetComponent<Renderer>().sharedMaterials = mat;

		// Collider
		
		MeshCollider collider = GetComponent<MeshCollider>();
		if (collider) {
			collider.sharedMesh = null;
			collider.sharedMesh = mesh;
		}
	}

	private static Vector3 centeredNormalizedPosition(float x, float y, float z) {
		return new Vector3(x * 2f - 1f, y * 2f - 1f, z * 2f - 1f).normalized;
	}

	private MeshGenerator.Vertex withNoise(Vector3 vertexPos, float x, float y) {
		float sample = FBMNoise.valueAt(vertexPos / noiseScale + noiseOffset, octaves, lacunarity, persistance);

		MeshGenerator.Vertex vert;
		vert.position = radius * vertexPos * (1 + sample * noiseMagnitude);
		vert.uv = new Vector2(x, y);
		vert.color = Color.Lerp(Color.black, Color.white, sample * 2f + 0.5f);
		vert.normal = Vector3.zero; // anything

		return vert;
	}
}
