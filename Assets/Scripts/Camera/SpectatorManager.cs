using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpectatorManager : MonoBehaviour {

	public float speed = 10f;
	public float controlDistance = 100f;

	private Rigidbody rb;
	private Vector3 velocity;
	private Selector selector;

    void Start() {
		rb = GetComponent<Rigidbody>();
		selector = new Selector(rb, controlDistance);
	}

	void Update() {
		velocity = new Vector3(Input.GetAxisRaw("Horizontal"), 0, Input.GetAxisRaw("Vertical")).normalized * speed;
		selector.Update();
    }

	void FixedUpdate() {
		rb.MovePosition(rb.position + velocity * Time.fixedDeltaTime);
	}
}

public class Selector {

	private Rigidbody rb;
	private float maxDistance;

	private float delta = 0.5f;
    private float lastClickTime = -10f;

	public Selector(Rigidbody rigidbody, float distance) {
		rb = rigidbody;
		maxDistance = distance;
	}

	public void Update() {
		if (Input.GetMouseButtonDown(0)) {
            float timeDelta = Time.time - lastClickTime;

            if (timeDelta < delta) {
                Select();
                lastClickTime = 0f;
            }
            else {
                lastClickTime = Time.time;
            }
        }
	}

	private void Select() {
		Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, maxDistance)) {
            if (hit.transform.gameObject.CompareTag("Unit")) {
                rb.MovePosition(hit.transform.position);
            }
        }
	}
}
