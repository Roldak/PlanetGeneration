using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;

public class PlanetMeshGenerator : MonoBehaviour, MeshGenerator {
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
            surface.CreateObject("part" + i, transform);
            surface.CreateMesh();
            surface.AssignMesh();
            surface.AssignMaterial();
            surface.AssignCollider();
            surface.AssignMeshSplitter(0, CUBE_SURFACE_PARAMETRIZATIONS[i]);
            
            childs.Add(surface.getObject());

            ++i;
        }
        UnityEngine.Debug.Log(watch.Elapsed.TotalSeconds);
    }

    public Material defaultRendererMaterial() {
        return terrainMaterial;
    }

    public PhysicMaterial defaultPhysicMaterial() {
        return terrainPhysicMaterial;
    }

    public bool shouldGenerateColliders() {
        return generateColliders;
    }

    public virtual SurfaceObjectCreator[] GenerateMeshDataFromSurfaceParametrizations(SurfaceGenerator.SurfaceParametrization[] faceParametrization) {
        int N = faceParametrization.GetLength(0);

        SurfaceObjectCreator[] objectCreators = new SurfaceObjectCreator[N];

        Action<int> generateFace = (int i) => {
            SurfaceGenerator gen = new SurfaceGenerator();
            SurfaceGenerator.SurfaceParametrization face = faceParametrization[i];

            gen.setParameters(X, Y, true, false);
            gen.setVerticesOutputArrays(new Vector3[X * Y], new Vector3[X * Y], new Vector2[X * Y], 0);
            gen.setIndicesOutputArray(new int[(X - 1) * (Y - 1) * 2 * 3], 0);
            gen.setColorsOutputArray(new Color[X * Y], 0);
            gen.Generate((float x, float y) => withNoise(face(x, y), x, y));

            objectCreators[i] = new SurfaceObjectCreator(gen, this);
        };

        if (enableMultithreading) {
            Parallel.For(0, N, 1, generateFace);
        } else {
            for (int i = 0; i < N; ++i) {
                generateFace(i);
            }
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

    private SurfaceGenerator.Vertex withNoise(Vector3 vertexPos, float x, float y) {
        float sample = FBMNoise.valueAt(vertexPos / noiseScale + noiseOffset, octaves, lacunarity, persistance);

        SurfaceGenerator.Vertex vert;
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

    private static SurfaceGenerator.SurfaceParametrization[] CUBE_SURFACE_PARAMETRIZATIONS = new SurfaceGenerator.SurfaceParametrization[] {
        TopFace, BottomFace, RightFace, LeftFace, FrontFace, BackFace
    };
}
