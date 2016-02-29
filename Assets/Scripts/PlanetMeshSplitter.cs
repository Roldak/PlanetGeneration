using UnityEngine;
using System.Collections;
using System.Threading;

[RequireComponent (typeof(Renderer))]
public class PlanetMeshSplitter : MonoBehaviour {
    public static readonly float REPEAT_RATE = 0.3f;

    public PlanetMeshGenerator planetGenerator;
    public PlanetMeshGenerator.SurfaceParametrization faceParametrization;
    public int level = 0;

    public bool CanSplit { get { return gameObject.activeSelf;  } }

    new private Renderer renderer;
    private GameObject[] childs;
    private bool busy = false;

    private PlanetMeshGenerator.SurfaceParametrization[] faceParametrizations;
    private PlanetMeshGenerator.SurfaceObjectCreator[] objectCreators = null;
    private Thread objectCreatorsThread = null;

    void Start() {
        faceParametrizations = new PlanetMeshGenerator.SurfaceParametrization[4];
        faceParametrizations[0] = (float x, float y) => faceParametrization(x * 0.5f, y * 0.5f);
        faceParametrizations[1] = (float x, float y) => faceParametrization(x * 0.5f + 0.5f, y * 0.5f);
        faceParametrizations[2] = (float x, float y) => faceParametrization(x * 0.5f, y * 0.5f + 0.5f);
        faceParametrizations[3] = (float x, float y) => faceParametrization(x * 0.5f + 0.5f, y * 0.5f + 0.5f);
    }

    void Awake () {
        renderer = GetComponent<Renderer>();
        InvokeRepeating("CheckCanSplitOrMerge", REPEAT_RATE, REPEAT_RATE);
	}
	
	void CheckCanSplitOrMerge() {
        if (busy) {
            return;
        }

        if (CanSplit) {
            foreach (GameObject obj in GameObject.FindGameObjectsWithTag("MeshSplitTriggerer")) {
                MeshSplitTriggerer triggerer = obj.GetComponent<MeshSplitTriggerer>();
                float dist2 = renderer.bounds.SqrDistance(obj.transform.position);
                foreach (MeshSplitTriggerer.TriggererInfo levelInfo in triggerer.levels) {
                    if (levelInfo.triggerRadius * levelInfo.triggerRadius * 2 > dist2 && levelInfo.triggerLevel > level) {
                        prepareSplit();
                        Debug.Log(name + " is preparing to split");
                    }

                    if (levelInfo.triggerRadius * levelInfo.triggerRadius > dist2 && levelInfo.triggerLevel > level) {
                        StartCoroutine(split());
                        Debug.Log(name + " is splitting");
                    }
                }
            }
        } else {
            foreach (GameObject obj in GameObject.FindGameObjectsWithTag("MeshSplitTriggerer")) {
                MeshSplitTriggerer triggerer = obj.GetComponent<MeshSplitTriggerer>();
                float dist2 = renderer.bounds.SqrDistance(obj.transform.position);
                foreach (MeshSplitTriggerer.TriggererInfo levelInfo in triggerer.levels) {
                    if (levelInfo.triggerRadius * levelInfo.triggerRadius * 2 > dist2 && levelInfo.triggerLevel > level) {
                        return;
                    }
                }
                merge();
                Debug.Log(name + " is merging");
            }
        }
	}

    private void prepareSplit() {
        if (objectCreators != null) {
            return;
        }

        objectCreatorsThread = new Thread(() => {
            objectCreators = planetGenerator.GenerateMeshDataFromSurfaceParametrizations(faceParametrizations);
        });

        objectCreatorsThread.Start();
    }

    private IEnumerator split() {
        busy = true;

        objectCreatorsThread.Join();

        childs = new GameObject[4];
        yield return null;

        for (int i = 0; i < 4; i++) {
            objectCreators[i].CreateObject(name + i);
            objectCreators[i].CreateMesh();

            yield return null;

            objectCreators[i].AssignMesh();
            objectCreators[i].AssignMaterial();

            yield return null;

            objectCreators[i].AssignCollider();
            objectCreators[i].AssignMeshSplitter(level + 1);

            childs[i] = objectCreators[i].getObject();

            yield return null;
        }

        gameObject.SetActive(false);

        busy = false;
    }

    private void merge() {
        busy = true;

        gameObject.SetActive(true);
        
        foreach (GameObject obj in childs) {
            Destroy(obj);
        }

        busy = false;
    }
}
