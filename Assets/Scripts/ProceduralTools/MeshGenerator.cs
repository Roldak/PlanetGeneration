using UnityEngine;
using System.Collections;

public interface MeshGenerator {
    Material defaultRendererMaterial();
    PhysicMaterial defaultPhysicMaterial();
    bool shouldGenerateColliders();
    SurfaceObjectCreator[] GenerateMeshDataFromSurfaceParametrizations(SurfaceGenerator.SurfaceParametrization[] faceParametrization);
}
