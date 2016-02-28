using UnityEngine;
using System.Collections;

[RequireComponent (typeof(Rigidbody))]
public class RigidBodyFPSController : MonoBehaviour {
    private static readonly float Epsilon = 0.00001f;

    public Camera FPSCamera;
    public CapsuleCollider bodyCollider;
    public float movingSpeed = 1f;
    public float jumpImpulse = 2f;
    public float mouseSensitivity = 2.0f;

    private Rigidbody rb;
    private GravityProducer land = null;

    private int landRefs = 0;

	// Use this for initialization
	void Start () {
        rb = GetComponent<Rigidbody>();
	}

    void OnTriggerEnter(Collider col) {
        GravityProducer producer = col.GetComponentInParent<GravityProducer>();
        if (producer) {
            if (land && producer == land) {
                ++landRefs;
            } else {
                land = producer;
                landRefs = 1;
            }
        }
    }

    void OnTriggerExit(Collider col) {
        if (land) {
            if (col.GetComponentInParent<GravityProducer>() == land) {
                if ((--landRefs) <= 0) {
                    land = null;
                }
            }
        }
    }

    void FixedUpdate() {
        handleCameraMovement();
        handleBodyMovement();
        handleGravityForce();
    }

    private void handleCameraMovement() {
        float h = Input.GetAxis("Mouse X");
        float v = Input.GetAxis("Mouse Y");

        transform.RotateAround(FPSCamera.transform.position, transform.up, h * mouseSensitivity);
        FPSCamera.transform.RotateAround(FPSCamera.transform.position, FPSCamera.transform.right, -v * mouseSensitivity);
    }

    private void handleBodyMovement() {
        float verticalVelocity = Vector3.Dot(transform.InverseTransformVector(rb.velocity), Vector3.up);
        
        if (Mathf.Abs(verticalVelocity) < 0.1f && land != null) {
            // stick to ground
            /*RaycastHit hitinfo;
            if (land.GetComponent<Collider>().Raycast(new Ray(transform.position, -transform.up), out hitinfo, 10f)) {
                float dist = (hitinfo.point - transform.position).magnitude;
                transform.position -= transform.up.normalized * (dist - bodyCollider.height * 0.5f);
            }*/
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
            if (Input.GetKeyDown(KeyCode.Space)) {
                dir += Vector3.up * jumpImpulse;
            }

            if (dir != Vector3.zero) {
                rb.velocity = transform.TransformVector(dir).normalized * movingSpeed;
            }
        }
    }

    private void handleGravityForce() {
        if (land) {
            Vector3 newup = (transform.position - land.Center).normalized;

            Vector3 axis;
            float angleRad;

            FromToAxisAngle(transform.up, newup, out axis, out angleRad);
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
