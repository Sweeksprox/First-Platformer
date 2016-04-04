using UnityEngine;
using System.Collections;

[RequireComponent (typeof(Controller))]
public class Enemy : MonoBehaviour {
	
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
	EnemyInfo info;
	
	Public GameObject player;
	
	void Start () {
		controller = GetComponent<Controller> ();
		
		//further implementation incoming
		/*
		enemy = GetComponent<EnemyInfo> ();
		enemy.health = health;
		enemy.rage = rage;
		*/
		
		gravity = (-2 * jumpHeight) / Mathf.Pow (jumpTime, 2);
		jumpVel = Mathf.Abs (gravity) * jumpTime;
		
		
	}
	
	void Update () {
		if (controller.collisions.above || controller.collisions.below) {
			velocity.y = 0;
		}
		
		float facing = PlayerDirection(player.transform.position);
		float targetVelX = horizontalInput * moveSpeed * facing;

		velocity.x = Mathf.SmoothDamp (velocity.x, targetVelX, ref smoothVelX, (controller.collisions.below) ? accelGround : accelAir);
		velocity.y += gravity * Time.deltaTime;
		controller.Move (velocity * Time.deltaTime);
	}
	
	float PlayerDirection (Vector3 pos) {
		float posX = pos.x;
		float selfX = transform.position.x;
		float diff = posX - selfX;
		float facing = Mathf.Sign(diff);
		return facing;
	}
}
