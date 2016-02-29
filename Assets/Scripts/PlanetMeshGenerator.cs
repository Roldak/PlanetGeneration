using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;

public class PlanetMeshGenerator : MonoBehaviour {
    public delegate Vector3 SurfaceParametrization(float x, float y);

    public struct SurfaceMeshData {
        public Vector3[] vertices;
        public Vector3[] normals;
        public Vector2[] uvs;
        public int[] triangles;

        public Color[] colors;
        public SurfaceParametrization parametrization;
    }

    public class SurfaceObjectCreator {
        private GameObject obj;
        private SurfaceMeshData data;

        private PlanetMeshGenerator gen;
        private Mesh mesh;

        public SurfaceObjectCreator(SurfaceMeshData data, PlanetMeshGenerator gen) {
            this.data = data;
            this.gen = gen;
        }

        public void CreateObject(string name) {
            obj = new GameObject(name);
            obj.transform.parent = gen.transform;
            gen.childs.Add(obj);
        }

        public void CreateMesh() {
            mesh = new Mesh();

            mesh.vertices = data.vertices;
            mesh.triangles = data.triangles;
            mesh.normals = data.normals;
            mesh.uv = data.uvs;
            mesh.Optimize();
            mesh.RecalculateBounds();
        }

        public void AssignMesh() {
            MeshFilter mf = obj.AddComponent<MeshFilter>();
            mf.sharedMesh = mesh;
        }

        public void AssignMaterial() {
            Texture2D tex = new Texture2D(gen.X, gen.Y);
            tex.SetPixels(data.colors);
            tex.filterMode = FilterMode.Trilinear;
            tex.wrapMode = TextureWrapMode.Clamp;
            tex.Apply();

            Renderer r = obj.AddComponent<MeshRenderer>();
            r.sharedMaterial = new Material(gen.terrainMaterial);
            r.sharedMaterial.mainTexture = tex;
        }

        public void AssignCollider() {
            if (gen.generateColliders) {
                MeshCollider col = obj.AddComponent<MeshCollider>();
                col.sharedMesh = null;
                col.sharedMesh = mesh;
                col.sharedMaterial = gen.terrainPhysicMaterial;
            }
        }

        public void AssignMeshSplitter(int lodLevel) {
            PlanetMeshSplitter splitter = obj.AddComponent<PlanetMeshSplitter>();
            splitter.planetGenerator = gen;
            splitter.faceParametrization = data.parametrization;
            splitter.level = lodLevel;
        }

        public GameObject getObject() {
            return obj;
        }
    }

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

    protected List<GameObject> childs = new List<GameObject>();

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
        SurfaceObjectCreator[] surfaces = GenerateMeshDataFromSurfaceParametrizations(CUBE_SURFACE_PARAMETRIZATIONS);

        Stopwatch watch = Stopwatch.StartNew();
        int i = 0;
        foreach (SurfaceObjectCreator surface in surfaces) {
            surface.CreateObject("part" + (i++));
            surface.CreateMesh();
            surface.AssignMesh();
            surface.AssignMaterial();
            surface.AssignCollider();
            surface.AssignMeshSplitter(0);
        }
        UnityEngine.Debug.Log(watch.Elapsed.TotalSeconds);
    }

    public SurfaceObjectCreator[] GenerateMeshDataFromSurfaceParametrizations(SurfaceParametrization[] faceParametrization) {
        int N = faceParametrization.GetLength(0);

        SurfaceObjectCreator[] objectCreators = new SurfaceObjectCreator[N];

        SurfaceMeshData[] meshdata = new SurfaceMeshData[N];
        MeshGenerator.VertexParametrization[] parametrizations = new MeshGenerator.VertexParametrization[N];

        for (int i = 0; i < N; i++) {
            SurfaceParametrization face = faceParametrization[i];
            meshdata[i].parametrization = face;
            parametrizations[i] = (float x, float y) => withNoise(face(x, y), x, y);
        }

        Action<int> generateFace = (int i) => {
            meshdata[i].triangles = new int[(X - 1) * (Y - 1) * 2 * 3];
            meshdata[i].colors = new Color[X * Y];
            meshdata[i].vertices = new Vector3[X * Y];
            meshdata[i].normals = new Vector3[X * Y];
            meshdata[i].uvs = new Vector2[X * Y];

            MeshGenerator gen = new MeshGenerator();
            gen.setParameters(X, Y, true, false);
            gen.setVerticesOutputArrays(meshdata[i].vertices, meshdata[i].normals, meshdata[i].uvs, 0);
            gen.setIndicesOutputArray(meshdata[i].triangles, 0);
            gen.setColorsOutputArray(meshdata[i].colors, 0);
            gen.Generate(parametrizations[i]);
        };

        if (enableMultithreading) {
            Parallel.For(0, N, 1, generateFace);
        } else {
            for (int i = 0; i < N; ++i) {
                generateFace(i);
            }
        }

        for (int i = 0; i < N; i++) {
            objectCreators[i] = new SurfaceObjectCreator(meshdata[i], this);
        }
        
        return objectCreators;
    }

    private static Vector3 centeredNormalizedPosition(float x, float y, float z) {
        return new Vector3(x * 2f - 1f, y * 2f - 1f, z * 2f - 1f).normalized;
    }

    private static readonly float SEA_LEVEL = -0.1f;
    private static readonly Color SAND_COLOR = Color.HSVToRGB(0.13f, 0.61f, 0.79f);
    private static readonly Color LAND_COLOR = new Color(0.651f, 0.40f, 0.314f);
    private static readonly float SAND_THRESHOLD = 0.1f;

    private MeshGenerator.Vertex withNoise(Vector3 vertexPos, float x, float y) {
        float sample = FBMNoise.valueAt(vertexPos / noiseScale + noiseOffset, octaves, lacunarity, persistance);

        MeshGenerator.Vertex vert;
        vert.position = radius * vertexPos * (1 + sample * noiseMagnitude);

        vert.uv = new Vector2(x, y);

        vert.color = Color.Lerp(Color.black, Color.white, sample / noiseMagnitude) * LAND_COLOR;
        if (sample < SEA_LEVEL) {
            vert.color = SAND_COLOR;
        } else if (sample < SAND_THRESHOLD) {
            vert.color = Color.Lerp(SAND_COLOR, vert.color, Mathf.InverseLerp(SEA_LEVEL, SAND_THRESHOLD, sample));
        }

        vert.normal = Vector3.zero;

        return vert;
    }

    private static Vector3 TopFace(float x, float y)    { return centeredNormalizedPosition(x, 1f, y); }
    private static Vector3 BottomFace(float x, float y) { return centeredNormalizedPosition(y, 0f, x); }
    private static Vector3 RightFace(float x, float y)  { return centeredNormalizedPosition(1f, y, x); }
    private static Vector3 LeftFace(float x, float y)   { return centeredNormalizedPosition(0f, x, y); }
    private static Vector3 FrontFace(float x, float y)  { return centeredNormalizedPosition(x, y, 0f); }
    private static Vector3 BackFace(float x, float y)   { return centeredNormalizedPosition(y, x, 1f); }

    private static SurfaceParametrization[] CUBE_SURFACE_PARAMETRIZATIONS = new SurfaceParametrization[] {
        TopFace, BottomFace, RightFace, LeftFace, FrontFace, BackFace
    };
}
