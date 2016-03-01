using UnityEngine;
using System.Collections;
using System;

public class WaterSphereGenerator : MonoBehaviour, MeshGenerator {
    public float radius = 1f;
    public int X = 64;
    public int Y = 64;

    public Material waterMaterial;
    public bool enableMultithreading = true;

    void Start() {
        Generate();
    }

    public PhysicMaterial defaultPhysicMaterial() {
        return null;
    }

    public Material defaultRendererMaterial() {
        return waterMaterial;
    }

    public void Generate() {
        SurfaceObjectCreator[] surfaces = GenerateMeshDataFromSurfaceParametrizations(Utils.SPHERE_SURFACE_PARAMETRIZATIONS);
        
        int i = 0;
        foreach (SurfaceObjectCreator surface in surfaces) {
            surface.CreateObject("part" + i, transform);
            surface.CreateMesh();
            surface.AssignMesh();
            surface.AssignMaterial();
            surface.AssignCollider();
            surface.AssignMeshSplitter(0, Utils.SPHERE_SURFACE_PARAMETRIZATIONS[i]);
            
            ++i;
        }
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
            gen.Generate((float x, float y) => {
                SurfaceGenerator.Vertex vert;
                vert.position = radius * face(x, y);
                vert.normal = Vector3.zero;
                vert.uv = new Vector2(x, y);
                vert.color = Color.white;
                return vert;
            });

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

    public bool shouldGenerateColliders() {
        return false;
    }
}
