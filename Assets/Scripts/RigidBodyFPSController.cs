using UnityEngine;
using System.Collections;

[RequireComponent (typeof(Rigidbody))]
public class RigidBodyFPSController : MonoBehaviour {
    private static readonly float Epsilon = 0.00001f;

    public Camera FPSCamera;
    public float movingSpeed = 1f;
    public float mouseSensitivity = 2.0f;

    private Rigidbody rb;
    private GravityProducer land;
    private Vector3 up = Vector3.up;

	// Use this for initialization
	void Start () {
        rb = GetComponent<Rigidbody>();
	}

    void OnTriggerEnter(Collider col) {
        GravityProducer producer = col.GetComponent<GravityProducer>();
        if (producer) {
            land = producer;
        }
    }

    void OnTriggerExit(Collider col) {
        if (col.gameObject == land.gameObject) {
            land = null;
        }
    }

    void FixedUpdate() {
        handleCameraRotation();
        handleCameraMovement();
        handleGravityForce();
    }

    private void handleCameraRotation() {
        float h = Input.GetAxis("Mouse X");
        float v = Input.GetAxis("Mouse Y");

        transform.RotateAround(FPSCamera.transform.position, up, h * mouseSensitivity);
        FPSCamera.transform.RotateAround(FPSCamera.transform.position, FPSCamera.transform.TransformVector(Vector3.right), -v * mouseSensitivity);
    }

    private void handleCameraMovement() {
        if (land != null) {
            rb.velocity = Vector3.zero;
            Vector3 dir = Vector3.zero;

            if (Input.GetKey(KeyCode.UpArrow) || Input.GetKey(KeyCode.W)) {
                dir += Vector3.forward;
            }
            if (Input.GetKey(KeyCode.DownArrow) || Input.GetKey(KeyCode.S)) {
                dir += Vector3.back;
            }
            if (Input.GetKey(KeyCode.LeftArrow) || Input.GetKey(KeyCode.A)) {
                dir += Vector3.left;
            }
            if (Input.GetKey(KeyCode.RightArrow) || Input.GetKey(KeyCode.D)) {
                dir += Vector3.right;
            }

            if (dir != Vector3.zero) {
                rb.velocity = transform.TransformVector(dir).normalized * movingSpeed;
            }
        }
    }

    private void handleGravityForce() {
        if (land) {
            up = (transform.position - land.transform.position).normalized;

            Vector3 axis;
            float angleRad;

            FromToAxisAngle(transform.TransformVector(Vector3.up), up, out axis, out angleRad);
            transform.Rotate(axis, angleRad * Mathf.Rad2Deg, Space.World);
        }
    }

    private static void FromToAxisAngle(Vector3 a, Vector3 b, out Vector3 axis, out float angleRad) {
        float dot = Vector3.Dot(a, b);

        if (dot > 1.0f - Epsilon) { // parallel in the same direction, rotate by 0° around a random axis
            axis = new Vector3(1, 0, 0);
            angleRad = 0;
        } else if (dot < -1.0f + Epsilon) { // parallel in the opposite direction, rotate by 180° around a random axis
            axis = new Vector3(1, 0, 0);
            angleRad = Mathf.PI;
        } else {
            axis = Vector3.Cross(a, b).normalized;
            angleRad = Mathf.Acos(dot);
        }
    }
}
