using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class MeshSplitTriggerer : MonoBehaviour {
    [System.Serializable]
    public class TriggererInfo {
        public int triggerLevel = 0;
        public float triggerRadius = 0f;
    }

    public List<TriggererInfo> levels;
}
