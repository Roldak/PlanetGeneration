using UnityEngine;
using UnityEditor;
using System.Collections;

[CustomEditor (typeof(PlanetMeshGenerator))]
public class PlanetMeshGeneratorEditor : Editor {
	public override void OnInspectorGUI () {
		DrawDefaultInspector();

		if (GUILayout.Button("Generate")) {
			(target as PlanetMeshGenerator).Generate();
		}
	}
}
