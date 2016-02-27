using UnityEngine;
using System.Collections;

[RequireComponent (typeof(Renderer))]
public class PlanetMeshSplitter : MonoBehaviour {
    public static readonly float REPEAT_RATE = 0.3f;

    public PlanetMeshGenerator planetGenerator;
    public MeshGenerator.VertexParametrization parametrization;
    public int level = 0;

    public bool CanSplit { get { return gameObject.activeSelf;  } }

    new private Renderer renderer;

    private GameObject[] childs;

	void Awake () {
        renderer = GetComponent<Renderer>();
        InvokeRepeating("CheckCanSplitOrMerge", REPEAT_RATE, REPEAT_RATE);
	}
	
	void CheckCanSplitOrMerge() {
        if (CanSplit) {
            foreach (GameObject obj in GameObject.FindGameObjectsWithTag("MeshSplitTriggerer")) {
                MeshSplitTriggerer triggerer = obj.GetComponent<MeshSplitTriggerer>();
                float dist2 = renderer.bounds.SqrDistance(obj.transform.position);
                foreach (MeshSplitTriggerer.TriggererInfo levelInfo in triggerer.levels) {
                    if (levelInfo.triggerRadius * levelInfo.triggerRadius > dist2 && levelInfo.triggerLevel > level) {
                        split();
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

    private void split() {
        gameObject.SetActive(false);
        
        MeshGenerator.VertexParametrization[] parametrizations = new MeshGenerator.VertexParametrization[4];

        parametrizations[0] = (float x, float y) => parametrization(x * 0.5f, y * 0.5f);
        parametrizations[1] = (float x, float y) => parametrization(x * 0.5f + 0.5f, y * 0.5f);
        parametrizations[2] = (float x, float y) => parametrization(x * 0.5f, y * 0.5f + 0.5f);
        parametrizations[3] = (float x, float y) => parametrization(x * 0.5f + 0.5f, y * 0.5f + 0.5f);

        childs = planetGenerator.GenerateFromParametrizations(parametrizations, name, level + 1);
    }

    private void merge() {
        gameObject.SetActive(true);
        
        foreach (GameObject obj in childs) {
            Destroy(obj);
        }
    }
}
