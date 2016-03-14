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

	//for later use to determine facing for object interaction
	bool facingLeft;

	void Start () {
		controller = GetComponent<Controller> ();
		
		//set gravity as a function of height/time, easier to tweak and parameters are less abstract
		gravity = (-2 * jumpHeight) / Mathf.Pow (jumpTime, 2);
		jumpVel = Mathf.Abs (gravity) * jumpTime;
	}

	void Update () {
		if (controller.collisions.above || controller.collisions.below) {
			//if collisions above or below stop moving up/down
			velocity.y = 0;
		}

		//get unity's built in horizontal axis controls, left right arrow standard
		float horizontalInput = Input.GetAxisRaw ("Horizontal");

		if (Input.GetKeyDown(KeyCode.Space) && controller.collisions.below) {
			velocity.y = jumpVel;
		}

		//set target velocity for smoothing/damping
		float targetVelX = horizontalInput * moveSpeed;
		
		//smooth out the horizontal acceleration/decelleration, less jerky and jumpy movement
		velocity.x = Mathf.SmoothDamp (velocity.x, targetVelX, ref smoothVelX, (controller.collisions.below) ? accelGround : accelAir);
		velocity.y += gravity * Time.deltaTime;
		controller.Move (velocity * Time.deltaTime);
	}
}
