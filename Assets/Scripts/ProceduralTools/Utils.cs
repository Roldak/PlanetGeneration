using UnityEngine;
using System.Collections;

public static class Utils {
    // Input in [0, 1] range, output in [-1, 1] range
    public static Vector3 centeredNormalizedPosition(float x, float y, float z) {
        return new Vector3(x * 2f - 1f, y * 2f - 1f, z * 2f - 1f).normalized;
    }

    private static Vector3 TopFace(float x, float y) { return centeredNormalizedPosition(x, 1f, y); }
    private static Vector3 BottomFace(float x, float y) { return centeredNormalizedPosition(y, 0f, x); }
    private static Vector3 RightFace(float x, float y) { return centeredNormalizedPosition(1f, y, x); }
    private static Vector3 LeftFace(float x, float y) { return centeredNormalizedPosition(0f, x, y); }
    private static Vector3 FrontFace(float x, float y) { return centeredNormalizedPosition(x, y, 0f); }
    private static Vector3 BackFace(float x, float y) { return centeredNormalizedPosition(y, x, 1f); }

    public static readonly SurfaceGenerator.SurfaceParametrization[] SPHERE_SURFACE_PARAMETRIZATIONS = new SurfaceGenerator.SurfaceParametrization[] {
        TopFace, BottomFace, RightFace, LeftFace, FrontFace, BackFace
    };
}
