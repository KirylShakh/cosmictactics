using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpectatorManager : MonoBehaviour {

	public float speed = 10f;

	private Rigidbody rb;
	private Vector3 velocity;

	void Start() {
		rb = GetComponent<Rigidbody>();
	}

	void Update() {
		velocity = new Vector3(Input.GetAxisRaw("Horizontal"), 0, Input.GetAxisRaw("Vertical")).normalized * speed;
	}

	void FixedUpdate() {
		rb.MovePosition(rb.position + velocity * Time.fixedDeltaTime);
	}
}
