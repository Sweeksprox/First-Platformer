using UnityEngine;
using System.Collections;

[RequireComponent (typeof (BoxCollider2D))]
public class Controller : MonoBehaviour {
	
	//assign the layer which to collide with
	public LayerMask collisionMask;

	//add a thin skin to make the hit box slightly smaller than our object
	const float skinWidth = .015f;
	//number of raycasts for left right up down
	public int horRayCount = 4;
	public int verRayCount = 4;

	//the max angles for slope interactions, don't want to go up vertical walls
	float maxClimbAngle = 80;
	float maxDescendAngle = 80;

	//space the rays, calculated from object width/length over ray count
	float horizontalRaySpacing;
	float verticalRaySpacing;

	//boxcollider component of game object
	BoxCollider2D collider;
	//identifier for struct for top left/right, bottom left/right for casting geometry
	RaycastOrigins raycastOrigins;
	//identifier for struct for collisions
	public CollisionInfo collisions;

	void Start() {
		//get game objects boxcollider component
		collider = GetComponent<BoxCollider2D> ();
		//self explanatory
		CalculateRaySpacing ();
	}

	public void Move(Vector3 velocity) {
		//update ray casting based on position
		UpdateRaycastOrigins ();
		//reset collision info
		collisions.Reset ();
		//hold ref to old velocity
		collisions.velocityOld = velocity;

		if (velocity.y < 0) {
			//if falling/going down, check if on a slope
			DescendSlope(ref velocity);
		}
		if (velocity.x != 0) {
			//if moving left/right, check for walls
			HorizontalCollisions (ref velocity);
		}
		if (velocity.y != 0) {
			//if jumping/falling and not on a slope, check for ceiling/floors
			VerticalCollisions (ref velocity);
		}

		//translate game object
		transform.Translate (velocity.x, velocity.y, 0);
	}

	void HorizontalCollisions(ref Vector3 velocity) {
		//get the sign on our horizontal velocity, -1 for left, +1 for right
		float directionX = Mathf.Sign (velocity.x);
		//raycast based on our velocity, compensate for skin width
		float rayLength = Mathf.Abs (velocity.x) + skinWidth;

		for (int i = 0; i < horRayCount; i ++) {
			//raycast left or right dependent on direction traveling, don't need to be casting behind us
			Vector2 rayOrigin = (directionX == -1)?raycastOrigins.bottomLeft:raycastOrigins.bottomRight;
			//origin ray at bottom corner (left/right), increment up the side of object by ray spacing as the loop continues
			rayOrigin += Vector2.up * (horizontalRaySpacing * i);
			//cast a ray at the origin, in the correct direction, distance rayLength, checking for collisionMask
			RaycastHit2D hit = Physics2D.Raycast(rayOrigin, Vector2.right * directionX, rayLength, collisionMask);

			//draw the the ray in the inspector
			Debug.DrawRay(rayOrigin, Vector2.right * directionX * rayLength,Color.red);

			if (hit) {
				//geometry to get the angle of the hit to check for sloped obstacle
				float slopeAngle = Vector2.Angle(hit.normal, Vector2.up);

				//only need to check for slope on the first ray cast from the origin at the corner
				if (i == 0 && slopeAngle <= maxClimbAngle) {
					if (collisions.goingDown) {
						collisions.goingDown = false;
						velocity = collisions.velocityOld;
					}
					float distanceToSlopeStart = 0;
					if (slopeAngle != collisions.slopeAngleOld) {
						distanceToSlopeStart = hit.distance-skinWidth;
						velocity.x -= distanceToSlopeStart * directionX;
					}
					ClimbSlope(ref velocity, slopeAngle);
					velocity.x += distanceToSlopeStart * directionX;
				}

				//if not climbing OR slope is above maximum climbable
				if (!collisions.goingUp || slopeAngle > maxClimbAngle) {
					//adjust our velocity based on hit distance for smoothing
					velocity.x = (hit.distance - skinWidth) * directionX;
					//don't need to cast future rays further than first hit
					rayLength = hit.distance;

					//if we ARE climbing do maths to get y component of velocity to follow the slope
					if (collisions.goingUp) {
						velocity.y = Mathf.Tan(collisions.slopeAngle * Mathf.Deg2Rad) * Mathf.Abs(velocity.x);
					}
					
					//logic stuff for set collision info
					//left equals bool of directionX == -1
					collisions.left = directionX == -1;
					//right equals bool of directionX == 1
					collisions.right = directionX == 1;
					collisions.hitNormal = hit.normal;
				}
			}
		}
	}

	bool Titanic(float iceberg) {
		bool floats = false;
		return floats;
	}

	void VerticalCollisions(ref Vector3 velocity) {
		//get the sign of velocity to determine direction, -1 for down, +1 for up
		float directionY = Mathf.Sign (velocity.y);
		//raylength as a function of y velocity, compensate for skin width
		float rayLength = Mathf.Abs (velocity.y) + skinWidth;

		for (int i = 0; i < verRayCount; i ++) {
			//check for direction, only cast out our rays in direciton of motion, don't need to check collisions behind us
			Vector2 rayOrigin = (directionY == -1)?raycastOrigins.bottomLeft:raycastOrigins.topLeft;
			//origin ray in whichever corner based on direction, iterate over the loop for future ray origins
			rayOrigin += Vector2.right * (verticalRaySpacing * i + velocity.x);
			//cast out a ray at the current origin, in the direciton of motion, a distance rayLength, checking for layer
			RaycastHit2D hit = Physics2D.Raycast(rayOrigin, Vector2.up * directionY, rayLength, collisionMask);

			//show the ray in the inspector
			Debug.DrawRay(rayOrigin, Vector2.up * directionY * rayLength,Color.red);

			if (hit) {
				//adjust y velocity based on hit distance for smoothin
				velocity.y = (hit.distance - skinWidth) * directionY;
				//don't need to cast future rays further than our first hit
				rayLength = hit.distance;

				
				if (collisions.goingUp) {
					//maths to get x component of y velocity if climbing
					velocity.x = velocity.y / Mathf.Tan(collisions.slopeAngle * Mathf.Deg2Rad) * Mathf.Sign(velocity.x);
				}

				//logic stuff for collision info
				//below equals bool of directionY == -1
				collisions.below = directionY == -1;
				//above equals bool of directionY == 1
				collisions.above = directionY == 1;
				collisions.hitNormal = hit.normal;
			}
		}

		if (collisions.goingUp) {
			//get the sign of x velocity to determine direction, -1 for left, +1 for right
			float directionX = Mathf.Sign(velocity.x);
			//raycast length off of x velocity
			rayLength = Mathf.Abs(velocity.x) + skinWidth;
			//determine direction for raycast origin
			Vector2 rayOrigin = ((directionX == -1)?raycastOrigins.bottomLeft:raycastOrigins.bottomRight) + Vector2.up * velocity.y;
			//raycast from origin, direciton of motion, a distance rayLength, checking for collisions with layer
			RaycastHit2D hit = Physics2D.Raycast(rayOrigin,Vector2.right * directionX,rayLength,collisionMask);

			if (hit) {
				//geometry for slope angle of the surface
				float slopeAngle = Vector2.Angle(hit.normal,Vector2.up);
				//if the angle changed from the angle we have stored
				if (slopeAngle != collisions.slopeAngle) {
					//set velocity based on hit distance
					velocity.x = (hit.distance - skinWidth) * directionX;
					//assign new slope angle
					collisions.slopeAngle = slopeAngle;
					collisions.hitNormal = hit.normal;
				}
			}
		}
	}

	void ClimbSlope(ref Vector3 velocity, float slopeAngle) {
		//absolute value of x velocity, need absolute value since we are climbing and don't want to go down
		float moveDistance = Mathf.Abs (velocity.x);
		//maths for y component of velocity to climb the slope
		float climbVelocityY = Mathf.Sin (slopeAngle * Mathf.Deg2Rad) * moveDistance;

		if (velocity.y <= climbVelocityY) {
			//if y component is lower than it needs to be to correctly climb the slope, set it equal
			velocity.y = climbVelocityY;
			//correct the x velocity
			velocity.x = Mathf.Cos (slopeAngle * Mathf.Deg2Rad) * moveDistance * Mathf.Sign (velocity.x);
			//set collision info
			collisions.below = true;
			collisions.goingUp = true;
			collisions.slopeAngle = slopeAngle;
		}
	}

	void DescendSlope(ref Vector3 velocity) {
		//get sign of x velocity to determine direction
		float directionX = Mathf.Sign (velocity.x);
		//don't need to cast backwards
		Vector2 rayOrigin = (directionX == -1) ? raycastOrigins.bottomRight : raycastOrigins.bottomLeft;
		//cast the ray
		RaycastHit2D hit = Physics2D.Raycast (rayOrigin, -Vector2.up, Mathf.Infinity, collisionMask);

		if (hit) {
			//geometry for the slope angle
			float slopeAngle = Vector2.Angle(hit.normal, Vector2.up);
			if (slopeAngle != 0 && slopeAngle <= maxDescendAngle) {
				//if it's an acceptable slope
				if (Mathf.Sign(hit.normal.x) == directionX) {
					//if the normal is is our direction of motion it means we are descending
					if (hit.distance - skinWidth <= Mathf.Tan(slopeAngle * Mathf.Deg2Rad) * Mathf.Abs(velocity.x)) {
						//if the distance away is less than the distance we can move to it this frame
						//absolute value of x velocity
						float moveDistance = Mathf.Abs(velocity.x);
						//math for y component of velocity
						float descendVelocityY = Mathf.Sin (slopeAngle * Mathf.Deg2Rad) * moveDistance;
						//math for x component of velocity
						velocity.x = Mathf.Cos (slopeAngle * Mathf.Deg2Rad) * moveDistance * Mathf.Sign (velocity.x);
						//going downhill so it's negative
						velocity.y -= descendVelocityY;

						//set collision info
						collisions.slopeAngle = slopeAngle;
						collisions.goingDown = true;
						collisions.below = true;
						collisions.hitNormal = hit.normal;
					}
				}
			}
		}
	}

	void UpdateRaycastOrigins() {
		//update the coordinates for raycasting as game object moves
		//get the dimensions of game object
		Bounds bounds = collider.bounds;
		//shrink height and width by skin width time two because skin on all sides
		bounds.Expand (skinWidth * -2);

		//set new origins
		raycastOrigins.bottomLeft = new Vector2 (bounds.min.x, bounds.min.y);
		raycastOrigins.bottomRight = new Vector2 (bounds.max.x, bounds.min.y);
		raycastOrigins.topLeft = new Vector2 (bounds.min.x, bounds.max.y);
		raycastOrigins.topRight = new Vector2 (bounds.max.x, bounds.max.y);
	}

	void CalculateRaySpacing() {
		//get the dimensions of the game object
		Bounds bounds = collider.bounds;
		//shrink the dimenions by a length of skinwidth times two, two because the skin is on both sides
		bounds.Expand (skinWidth * -2);

		//clamp the ray counts between 2 and 2,147,483,647
		horRayCount = Mathf.Clamp (horRayCount, 2, int.MaxValue);
		verRayCount = Mathf.Clamp (verRayCount, 2, int.MaxValue);

		//maths for spacing between ray origins
		horizontalRaySpacing = bounds.size.y / (horRayCount - 1);
		verticalRaySpacing = bounds.size.x / (verRayCount - 1);
	}

	struct RaycastOrigins {
		//struct for the raycast origins for spacing raycasts along an edge
		public Vector2 topLeft;
		public Vector2 topRight;
		public Vector2 bottomLeft;
		public Vector2 bottomRight;
	}

	public struct CollisionInfo {
		//struct for holding all collision info with terrain obstacles
		public bool above;
		public bool below;
		public bool left;
		public bool right;

		public bool goingUp;
		public bool goingDown;
		public float slopeAngle;
		public float slopeAngleOld;
		public Vector3 velocityOld;
		public Vector3 hitNormal;

		//reset all collision info
		public void Reset() {
			above = false;
			below = false;
			left = false;
			right = false;
			goingUp = false;
			goingDown = false;

			slopeAngleOld = slopeAngle;
			slopeAngle = 0;
		}
	}
}
