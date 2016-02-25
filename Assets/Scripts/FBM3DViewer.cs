using UnityEngine;
using System.Collections;

[RequireComponent (typeof(Renderer))]
public class FBM3DViewer : MonoBehaviour {

	public int width = 100;
	public int height = 100;
	public float scale = 30f;
	public float offsetX = 0f;
	public float offsetY = 0f;
	public float timeStep = 0.1f;

	[Range(1, 8)]
	public int octaves = 5;
	public float lacunarity = 2f;
	public float persistance = 0.5f;

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
	}
	
	void UpdateNoise() {
		time += timeStep;
		Color[] colors = new Color[width * height];
		int index = 0;
		float s = 1f / scale;

		for (int y = 0; y < height; ++y) {
			for (int x = 0; x < width; ++x, ++index) {
				colors[index] = Color.Lerp(Color.black, Color.white, FBMNoise.valueAt(x * s + offsetX, y * s + offsetY, time, octaves, lacunarity, persistance) * 0.5f + 0.5f);
			}
		}
		
		tex.SetPixels(colors);
		tex.Apply();
		GetComponent<Renderer>().material.mainTexture = tex;
	}
}
