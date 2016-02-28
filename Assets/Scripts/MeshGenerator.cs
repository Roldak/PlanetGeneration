using UnityEngine;
using System;
using System.Collections;

public class MeshGenerator {

    public delegate Vertex VertexParametrization(float x, float y);

    public struct Vertex {
        public Vector3 position;
        public Vector3 normal;
        public Vector2 uv;
        public Color color;
    }

    public int resolutionX;
    public int resolutionY;
    public bool autoGenerateNormalsArray;
    public bool makeClockwiseTriangles;
    public float derivativeResolution;

    public Vector3[] vertices;
    public Vector3[] normals;
    public Vector2[] uvs;
    public int vertexOffset;

    public Color[] colors;
    public int colorOffset;

    public int[] indices;
    public int indexOffset;

    public void setParameters(int resolutionX, int resolutionY, bool autoGenerateNormalsArray, bool makeClockwiseTriangles, float derivativeResolution = 0.01f) {
        this.resolutionX = resolutionX;
        this.resolutionY = resolutionY;
        this.autoGenerateNormalsArray = autoGenerateNormalsArray;
        this.makeClockwiseTriangles = makeClockwiseTriangles;
        this.derivativeResolution = derivativeResolution;
    }

    public void setVerticesOutputArrays(Vector3[] vertices, Vector3[] normals, Vector2[] uvs, int vertexOffset) {
        this.vertices = vertices;
        this.normals = normals;
        this.uvs = uvs;
        this.vertexOffset = vertexOffset;
    }

    public void setColorsOutputArray(Color[] colors, int colorOffset) {
        this.colors = colors;
        this.colorOffset = colorOffset;
    }

    public void setIndicesOutputArray(int[] indices, int indexOffset) {
        this.indices = indices;
        this.indexOffset = indexOffset;
    }

    public void Generate(VertexParametrization surfaceParam) {
        float oneOverResXMinusOne = 1f / (resolutionX - 1f);
        float oneOverResYMinusOne = 1f / (resolutionY - 1f);

        for (int y = 0; y < resolutionY; ++y) {
            float normY = y * oneOverResYMinusOne;

            for (int x = 0; x < resolutionX; ++x) {
                float normX = x * oneOverResXMinusOne;

                Vertex vert = surfaceParam(normX, normY);

                if (vertices != null) {
                    vertices[vertexOffset] = vert.position;
                }

                if (normals != null) {
                    if (autoGenerateNormalsArray) {
                        normals[vertexOffset] = Vector3.Cross(
                            computeSurfaceYDerivative(surfaceParam, normX, normY, vert.position, derivativeResolution),
                            computeSurfaceXDerivative(surfaceParam, normX, normY, vert.position, derivativeResolution)
                        ).normalized;
                    } else {
                        normals[vertexOffset] = vert.normal;
                    }
                }

                if (uvs != null) {
                    uvs[vertexOffset] = vert.uv;
                }

                if (colors != null) {
                    colors[colorOffset++] = vert.color;
                }

                if (indices != null && x < resolutionX - 1 && y < resolutionY - 1) {
                    if (makeClockwiseTriangles) {
                        makeTriangle(vertexOffset, vertexOffset + 1, vertexOffset + resolutionX);
                        makeTriangle(vertexOffset + 1, vertexOffset + resolutionX + 1, vertexOffset + resolutionX);
                    } else {
                        makeTriangle(vertexOffset + resolutionX, vertexOffset + 1, vertexOffset);
                        makeTriangle(vertexOffset + resolutionX, vertexOffset + resolutionX + 1, vertexOffset + 1);
                    }
                }

                ++vertexOffset;
            }
        }
    }

    private static Vector3 computeSurfaceXDerivative(VertexParametrization surfaceParam, float x, float y, Vector3 surfaceParamOfxy, float h) {
        return (surfaceParam(x + h, y).position - surfaceParamOfxy) / h;
    }

    private static Vector3 computeSurfaceYDerivative(VertexParametrization surfaceParam, float x, float y, Vector3 surfaceParamOfxy, float h) {
        return (surfaceParam(x, y + h).position - surfaceParamOfxy) / h;
    }

    private void makeTriangle(int a, int b, int c) {
        indices[indexOffset + 0] = a;
        indices[indexOffset + 1] = b;
        indices[indexOffset + 2] = c;
        indexOffset += 3;
    }
}
