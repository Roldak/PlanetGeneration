using UnityEngine;
using System.Collections;

[RequireComponent (typeof(Rigidbody))]
public class GravitySensitive : MonoBehaviour {
    private Rigidbody rb;

    public Vector3 Acceleration { get; private set; }

    void Start() {
        rb = GetComponent<Rigidbody>();
    }

	void FixedUpdate () {
        Acceleration = Vector3.zero;
        foreach(GravityProducer producer in GameObject.FindObjectsOfType<GravityProducer>()) {
            Vector3 r = producer.Center - transform.position;
            float magnitude = r.magnitude;
            Acceleration += GravityProducer.G * producer.mass * r / (magnitude * magnitude * magnitude);
        }
        rb.AddForce(Acceleration, ForceMode.Acceleration);
    }
}
