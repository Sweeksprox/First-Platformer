using UnityEngine;
using System.Collections;

[RequireComponent (typeof(Controller))]
public class Projectile : MonoBehaviour {

	public float speed;

	public float gravity;
	public float friction;

	public float bounceCoefficient;

	Vector2 velocity;

	Controller controller;

	void Start () {
		controller = GetComponent<Controller> ();

		velocity.y = 0;
		velocity.x = speed;
	}

	void Update () {
		if (controller.collisions.below || controller.collisions.above) {
			velocity.y *= Mathf.Sign(velocity.y) * controller.collisions.hitNormal.y * bounceCoefficient;
			velocity.x += Mathf.Sign(velocity.x) * friction;
		}
		if (controller.collisions.left || controller.collisions.right) {
			velocity.x *= Mathf.Sign(velocity.x) * controller.collisions.hitNormal.x * bounceCoefficient;
		}
		velocity.y += gravity * Time.deltaTime;
		controller.Move (velocity * Time.deltaTime);
	}
}
