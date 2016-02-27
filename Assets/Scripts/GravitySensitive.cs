﻿using UnityEngine;
using System.Collections;

[RequireComponent (typeof(Rigidbody))]
public class GravitySensitive : MonoBehaviour {
    private Rigidbody rb;

    void Start() {
        rb = GetComponent<Rigidbody>();
    }

	void FixedUpdate () {
        foreach(GravityProducer producer in GameObject.FindObjectsOfType<GravityProducer>()) {
            Vector3 r = producer.Center - transform.position;
            float magnitude = r.magnitude;
            Vector3 force = GravityProducer.G * producer.mass * r / (magnitude * magnitude * magnitude);
            rb.AddForce(force, ForceMode.Acceleration);
        }
	}
}