using UnityEngine;
using System.Collections;

[RequireComponent (typeof(Renderer))]
public class Perlin3DViewer : MonoBehaviour {

	public int width = 100;
	public int height = 100;
	public float scale = 30f;
	public float offsetX = 0f;
	public float offsetY = 0f;
	public float timeStep = 0.1f;

	private Texture2D tex;
	private float time;

	// Use this for initialization
	void Start () {
		tex = new Texture2D(width, height);
		time = 0f;
		InvokeRepeating("UpdateNoise", 0f, timeStep);
	}

	void Update() {
		if (Input.GetMouseButton(0)) {
			offsetX += Input.GetAxis("Mouse X");
			offsetY += Input.GetAxis("Mouse Y");
		}

		scale += Input.GetAxis("Mouse ScrollWheel");
		Debug.Log(scale);
	}

	void UpdateNoise() {
		time += timeStep;
		Color[] colors = new Color[width * height];
		int index = 0;
		float s = 1f / scale;
		for (int y = 0; y < height; ++y) {
			for (int x = 0; x < width; ++x, ++index) {
				colors[index] = Color.Lerp(Color.black, Color.white, PerlinNoise.valueAt(x * s + offsetX, y * s + offsetY, time) * 0.5f + 0.5f);
			}
		}

		tex.SetPixels(colors);
		tex.Apply();
		GetComponent<Renderer>().sharedMaterial.mainTexture = tex;
	}
}
