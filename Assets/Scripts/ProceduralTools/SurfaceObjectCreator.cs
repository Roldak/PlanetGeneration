using UnityEngine;

public class SurfaceObjectCreator {
    private GameObject obj;
    private SurfaceGenerator data;

    private MeshGenerator gen;
    private Mesh mesh;

    public SurfaceObjectCreator(SurfaceGenerator data, MeshGenerator gen) {
        this.data = data;
        this.gen = gen;
    }

    public void CreateObject(string name, Transform parent) {
        obj = new GameObject(name);
        obj.transform.parent = parent;
    }

    public void CreateMesh() {
        mesh = new Mesh();

        mesh.vertices = data.vertices;
        mesh.triangles = data.indices;
        mesh.normals = data.normals;
        mesh.uv = data.uvs;
        mesh.Optimize();
        mesh.RecalculateBounds();
        //mesh.RecalculateNormals();
    }

    public void AssignMesh() {
        MeshFilter mf = obj.AddComponent<MeshFilter>();
        mf.sharedMesh = mesh;
    }

    public void AssignMaterial() {
        Renderer r = obj.AddComponent<MeshRenderer>();
        r.sharedMaterial = new Material(gen.defaultRendererMaterial());

        if (data.colors != null) {
            Texture2D tex = new Texture2D(data.resolutionX, data.resolutionY);
            tex.SetPixels(data.colors);
            tex.filterMode = FilterMode.Trilinear;
            tex.wrapMode = TextureWrapMode.Clamp;
            tex.Apply();

            r.sharedMaterial.mainTexture = tex;
        }
    }

    public void AssignCollider() {
        if (gen.shouldGenerateColliders()) {
            MeshCollider col = obj.AddComponent<MeshCollider>();
            col.sharedMesh = null;
            col.sharedMesh = mesh;
            col.sharedMaterial = gen.defaultPhysicMaterial();
        }
    }

    public void AssignMeshSplitter(int lodLevel, SurfaceGenerator.SurfaceParametrization faceParametrization) {
        MeshSplitter splitter = obj.AddComponent<MeshSplitter>();
        splitter.meshGenerator = gen;
        splitter.faceParametrization = faceParametrization;
        splitter.level = lodLevel;
    }

    public GameObject getObject() {
        return obj;
    }
}