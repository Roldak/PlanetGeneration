using UnityEngine;
using System.Collections;

public interface MeshGenerator {
    Material defaultRendererMaterial();
    PhysicMaterial defaultPhysicMaterial();
    bool shouldGenerateColliders();
    int getLODLevelToPrecompute();
    SurfaceObjectCreator[] GenerateMeshDataFromSurfaceParametrizations(SurfaceGenerator.SurfaceParametrization[] faceParametrization);
}
