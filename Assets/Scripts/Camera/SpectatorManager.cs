using UnityEngine;


public class SpectatorManager : MonoBehaviour
{
	public float speed = 10f;
	public float controlDistance = 100f;

    private Rigidbody rb;
	private Vector3 velocity;

    private float delta = 0.5f;
    private float lastClickTime = -10f;
    
    void Start() {
		rb = GetComponent<Rigidbody>();
	}

	void Update() {
        HandleInput();
    }

	void FixedUpdate() {
		rb.MovePosition(rb.position + velocity * Time.fixedDeltaTime);
	}

    private void HandleInput() {
        velocity = new Vector3(Input.GetAxisRaw("Horizontal"), 0, Input.GetAxisRaw("Vertical")).normalized * speed;

        if (Input.GetMouseButtonDown(0)) {
            CheckDoubleClick();
        }
    }

    private void CheckDoubleClick() {
        float timeDelta = Time.time - lastClickTime;

        if (timeDelta < delta) {
            Jump();
            lastClickTime = 0f;
        }
        else {
            lastClickTime = Time.time;
        }
    }

	private void Jump() {
		Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, controlDistance)) {
            if (hit.transform.gameObject.CompareTag("Unit")) {
                rb.MovePosition(hit.transform.position);
            }
        }
	}
}
