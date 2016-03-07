using UnityEngine;
using System.Collections;
using System.Threading;

[RequireComponent(typeof(Renderer))]
public class MeshSplitter : MonoBehaviour {
    public static readonly float REPEAT_RATE = 0.3f;

    public MeshGenerator meshGenerator;
    public SurfaceGenerator.SurfaceParametrization faceParametrization;
    public int level = 0;

    public bool CanSplit { get { return gameObject.activeSelf; } }

    new private Renderer renderer;
    private GameObject[] childs = null;
    private bool busy = false;

    private SurfaceGenerator.SurfaceParametrization[] childrenFaceParametrizations;
    private SurfaceObjectCreator[] objectCreators = null;
    private Thread objectCreatorsThread = null;

    void Start() {
        childrenFaceParametrizations = new SurfaceGenerator.SurfaceParametrization[4];
        childrenFaceParametrizations[0] = (float x, float y) => faceParametrization(x * 0.5f, y * 0.5f);
        childrenFaceParametrizations[1] = (float x, float y) => faceParametrization(x * 0.5f + 0.5f, y * 0.5f);
        childrenFaceParametrizations[2] = (float x, float y) => faceParametrization(x * 0.5f, y * 0.5f + 0.5f);
        childrenFaceParametrizations[3] = (float x, float y) => faceParametrization(x * 0.5f + 0.5f, y * 0.5f + 0.5f);

        renderer = GetComponent<Renderer>();
        restart();

        if (meshGenerator.getLODLevelToPrecompute() > level) {
            // force creation of children but deactivate them directly
            prepareSplit();
            IEnumerator num = split();
            while (num.MoveNext()) { }
        }
    }

    void OnDestroy() {
        if (childs != null) {
            foreach (GameObject child in childs) {
                Destroy(child);
            }
        }
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
                        //Debug.Log(name + " is preparing to split");
                    }

                    if (levelInfo.triggerRadius * levelInfo.triggerRadius > dist2 && levelInfo.triggerLevel > level) {
                        StartCoroutine(split());
                        return;
                        //Debug.Log(name + " is splitting");
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
            }
            merge();
            //Debug.Log(name + " is merging");
        }
    }

    private void prepareSplit() {
        if (objectCreators != null || objectCreatorsThread != null) {
            return;
        }

        objectCreatorsThread = new Thread(() => {
            objectCreators = meshGenerator.GenerateMeshDataFromSurfaceParametrizations(childrenFaceParametrizations);
        });

        objectCreatorsThread.Start();
    }

    private IEnumerator split() {
        busy = true;

        if (childs != null) {
            foreach (GameObject child in childs) {
                MeshSplitter ms = child.GetComponent<MeshSplitter>();
                if (ms) {
                    ms.restart();
                }
            }
        } else {
            objectCreatorsThread.Join();

            GameObject[] tmp = new GameObject[4];
            yield return null;

            for (int i = 0; i < 4; i++) {
                objectCreators[i].CreateObject(name + i, transform.parent);
                objectCreators[i].CreateMesh();

                yield return null;

                objectCreators[i].AssignMesh();
                objectCreators[i].AssignMaterial();

                yield return null;

                objectCreators[i].AssignCollider();
                objectCreators[i].AssignMeshSplitter(level + 1, childrenFaceParametrizations[i]);

                tmp[i] = objectCreators[i].getObject();

                yield return null;
            }

            childs = tmp;
        }

        gameObject.SetActive(false);

        busy = false;
    }

    private void merge() {
        busy = true;

        gameObject.SetActive(true);
        
        foreach (GameObject child in childs) {
            MeshSplitter ms = child.GetComponent<MeshSplitter>();
            if (ms) {
                ms.disableRecursively();
            }
        }

        busy = false;
    }

    private void disableRecursively() {
        CancelInvoke();
        gameObject.SetActive(false);
        if (childs != null) {
            foreach (GameObject child in childs) {
                MeshSplitter ms = child.GetComponent<MeshSplitter>();
                if (ms) {
                    ms.disableRecursively();
                }
            }
        }
    }

    private void restart() {
        gameObject.SetActive(true);
        busy = false;
        CancelInvoke();
        InvokeRepeating("CheckCanSplitOrMerge", REPEAT_RATE, REPEAT_RATE);
    }
}
