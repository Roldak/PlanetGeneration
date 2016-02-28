using UnityEngine;
using System.Collections;

public class Sun : MonoBehaviour {
    public Transform revolutionCenter;
    public float revolutionSpeed = 1f;
    public float distance = 10f;

    public bool stayAbovePlayer = false;
    public GameObject player;

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
        if (stayAbovePlayer) {
            transform.forward = -player.transform.up;
        } else {
            float t = Time.realtimeSinceStartup * revolutionSpeed;
            transform.position = new Vector3(Mathf.Cos(t), 0f, Mathf.Sin(t)) * distance;
            transform.LookAt(revolutionCenter);
        }
	}
}
