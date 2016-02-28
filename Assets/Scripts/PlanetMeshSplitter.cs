using UnityEngine;
using System.Collections;

[RequireComponent (typeof(Renderer))]
public class PlanetMeshSplitter : MonoBehaviour {
    public static readonly float REPEAT_RATE = 0.3f;

    public PlanetMeshGenerator planetGenerator;
    public PlanetMeshGenerator.FaceParametrization faceParametrization;
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
        
        PlanetMeshGenerator.FaceParametrization[] faceParametrizations = new PlanetMeshGenerator.FaceParametrization[4];

        faceParametrizations[0] = (float x, float y) => faceParametrization(x * 0.5f, y * 0.5f);
        faceParametrizations[1] = (float x, float y) => faceParametrization(x * 0.5f + 0.5f, y * 0.5f);
        faceParametrizations[2] = (float x, float y) => faceParametrization(x * 0.5f, y * 0.5f + 0.5f);
        faceParametrizations[3] = (float x, float y) => faceParametrization(x * 0.5f + 0.5f, y * 0.5f + 0.5f);

        childs = planetGenerator.GenerateFromFaceParametrizations(faceParametrizations, name, level + 1);
    }

    private void merge() {
        gameObject.SetActive(true);
        
        foreach (GameObject obj in childs) {
            Destroy(obj);
        }
    }
}
