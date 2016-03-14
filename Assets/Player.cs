using UnityEngine;
using System.Collections;

[RequireComponent (typeof(Controller))]
public class Player : MonoBehaviour {

	public float moveSpeed;

	public float jumpHeight;
	public float jumpTime;
	public float accelAir;
	public float accelGround;

	float gravity;
	float jumpVel;
	Vector2 velocity;
	float smoothVelX;

	Controller controller;

	public GameObject projectile;

	void Start () {
		controller = GetComponent<Controller> ();

		gravity = (-2 * jumpHeight) / Mathf.Pow (jumpTime, 2);
		jumpVel = Mathf.Abs (gravity) * jumpTime;
		print (gravity);
	}

	void Update () {
		if (controller.collisions.above || controller.collisions.below) {
			velocity.y = 0;
		}
		if (Input.GetKeyDown(KeyCode.F)) {
			float direction = Mathf.Sign(velocity.x);
			GameObject rock = Instantiate (projectile, transform.position + direction * transform.right, Quaternion.identity) as GameObject;
			rock.GetComponent<Projectile>().speed *= direction;
		}

		float horizontalInput = Input.GetAxisRaw ("Horizontal");

		if (Input.GetKeyDown(KeyCode.Space) && controller.collisions.below) {
			velocity.y = jumpVel;
		}

		float targetVelX = horizontalInput * moveSpeed;
		velocity.x = Mathf.SmoothDamp (velocity.x, targetVelX, ref smoothVelX, (controller.collisions.below) ? accelGround : accelAir);
		velocity.y += gravity * Time.deltaTime;
		controller.Move (velocity * Time.deltaTime);
	}
}
