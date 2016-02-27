using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;

public class PlanetMeshGenerator : MonoBehaviour {
    public float radius = 1f;
    public int X = 100;
    public int Y = 100;

    public Material terrainMaterial;
    public PhysicMaterial terrainPhysicMaterial;

    [Range(0, 1)]
    public float noiseMagnitude = 0.5f;
    public float noiseScale = 30f;
    public Vector3 noiseOffset = Vector3.zero;

    [Range(1, 8)]
    public int octaves = 5;
    public float lacunarity = 2f;
    public float persistance = 0.5f;

    public bool generateColliders = true;
    public bool enableMultithreading = true;

    private List<GameObject> childs = new List<GameObject>();

    // Use this for initialization
    void Start() {
        Generate();
    }

    // Update is called once per frame
    void Update() {
        //transform.Rotate(new Vector3(1, 2, 3) * 0.1f);
    }

    public void Generate() {
        foreach (GameObject child in childs) {
            DestroyImmediate(child);
        }
        childs.Clear();
        
        MeshGenerator.VertexParametrization[] parametrizations = new MeshGenerator.VertexParametrization[6];

        parametrizations[0] = (float x, float y) => withNoise(centeredNormalizedPosition(x, 1f, y), x, y); // top
        parametrizations[1] = (float x, float y) => withNoise(centeredNormalizedPosition(y, 0f, x), x, y); // bottom
        parametrizations[2] = (float x, float y) => withNoise(centeredNormalizedPosition(1f, y, x), x, y); // right
        parametrizations[3] = (float x, float y) => withNoise(centeredNormalizedPosition(0f, x, y), x, y); // left
        parametrizations[4] = (float x, float y) => withNoise(centeredNormalizedPosition(y, x, 1f), x, y); // back
        parametrizations[5] = (float x, float y) => withNoise(centeredNormalizedPosition(x, y, 0f), x, y); // front

        childs.AddRange(GenerateFromParametrizations(parametrizations, "part", 0));
    }

    public GameObject[] GenerateFromParametrizations(MeshGenerator.VertexParametrization[] parametrizations, string baseName, int lodLevel) {
        int N = parametrizations.GetLength(0);

        Vector3[][] vertices = new Vector3[N][];
        Vector3[][] normals = new Vector3[N][];
        Vector2[][] uvs = new Vector2[N][];

        int[][] triangles = new int[N][];
        Color[][] colors = new Color[N][];

        Action<int> generateFace = (int i) => {
            triangles[i] = new int[(X - 1) * (Y - 1) * 2 * 3];
            colors[i] = new Color[X * Y];
            vertices[i] = new Vector3[X * Y];
            normals[i] = new Vector3[X * Y];
            uvs[i] = new Vector2[X * Y];

            MeshGenerator gen = new MeshGenerator();
            gen.setParameters(X, Y, true, false);
            gen.setVerticesOutputArrays(vertices[i], normals[i], uvs[i], 0);
            gen.setIndicesOutputArray(triangles[i], 0);
            gen.setColorsOutputArray(colors[i], 0);
            gen.Generate(parametrizations[i]);
        };
        
        Stopwatch watch = Stopwatch.StartNew();
        if (enableMultithreading) {
            Parallel.For(0, N, 1, generateFace);
        } else {
            for (int i = 0; i < N; ++i) {
                generateFace(i);
            }
        }
        UnityEngine.Debug.Log(watch.Elapsed.TotalSeconds);

        GameObject[] objects = new GameObject[N];

        for (int i = 0; i < N; i++) {
            GameObject child = new GameObject(baseName + i);
            child.transform.parent = transform;

            // MESH

            Mesh mesh = new Mesh();

            mesh.vertices = vertices[i];
            mesh.triangles = triangles[i];
            mesh.normals = normals[i];
            mesh.uv = uvs[i];
            mesh.Optimize();
            mesh.RecalculateBounds();
            //mesh.RecalculateNormals();

            MeshFilter mf = child.AddComponent<MeshFilter>();
            mf.sharedMesh = mesh;

            // MATERIAL

            Texture2D tex = new Texture2D(X, Y);
            tex.SetPixels(colors[i]);
            tex.filterMode = FilterMode.Trilinear;
            tex.wrapMode = TextureWrapMode.Clamp;
            tex.Apply();

            Renderer r = child.AddComponent<MeshRenderer>();
            r.sharedMaterial = new Material(terrainMaterial);
            r.sharedMaterial.mainTexture = tex;

            // COLLIDER

            if (generateColliders) {
                MeshCollider col = child.AddComponent<MeshCollider>();
                col.sharedMesh = null;
                col.sharedMesh = mesh;
                col.sharedMaterial = terrainPhysicMaterial;
            }

            // PLANET MESH SPLITTER
            
            PlanetMeshSplitter splitter = child.AddComponent<PlanetMeshSplitter>();
            splitter.planetGenerator = this;
            splitter.parametrization = parametrizations[i];
            splitter.level = lodLevel;

            objects[i] = child;
        }

        return objects;
    }

    private static Vector3 centeredNormalizedPosition(float x, float y, float z) {
        return new Vector3(x * 2f - 1f, y * 2f - 1f, z * 2f - 1f).normalized;
    }

    private static readonly float SEA_LEVEL = -0.1f;
    private static readonly Color SAND_COLOR = Color.HSVToRGB(0.13f, 0.61f, 0.79f);
    private static readonly Color LAND_COLOR = new Color(0.651f, 0.40f, 0.314f);
    private static readonly float SAND_THRESHOLD = 0f;

    private MeshGenerator.Vertex withNoise(Vector3 vertexPos, float x, float y) {
        float sample = FBMNoise.valueAt(vertexPos / noiseScale + noiseOffset, octaves, lacunarity, persistance);

        MeshGenerator.Vertex vert;
        vert.position = radius * vertexPos * (1 + sample * noiseMagnitude);

        vert.uv = new Vector2(x, y);

        vert.color = Color.Lerp(Color.black, Color.white, sample * 2f + 0.5f) * LAND_COLOR;
        if (sample < SEA_LEVEL) {
            vert.color = SAND_COLOR;
        } else if (sample < SAND_THRESHOLD) {
            vert.color = Color.Lerp(SAND_COLOR, vert.color, Mathf.InverseLerp(SEA_LEVEL, SAND_THRESHOLD, sample));
        }

        vert.normal = Vector3.zero;

        return vert;
    }
}
