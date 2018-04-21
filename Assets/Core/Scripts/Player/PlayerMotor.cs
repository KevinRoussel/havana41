﻿using UnityEngine;
using Prime31;
using UnityEngine.Events;

[RequireComponent(typeof(CharacterController2D))]
public class PlayerMotor : MonoBehaviour {

	public float speed = 7f;
	public float gravity = 30f;
	public float fallCoef = 2f;
	public float jumpForce = 14f;
	public LayerMask platformLayer;

	[ReadOnly] public bool grounded;
	[ReadOnly] public Vector3 velocity = Vector3.zero;
	[ReadOnly] public bool flipX;
	[ReadOnly] public bool isGroundedOnPlatform;

	[SerializeField] UnityEvent onJump;
	[SerializeField] UnityEvent onPlayerLand;

	CharacterController2D controller;
	GameObject gun;

	void Start () {
		controller = GetComponent<CharacterController2D>();
		gun = transform.Find("Gun").gameObject;

		controller.onControllerCollidedEvent += (RaycastHit2D ray) => {
			isGroundedOnPlatform = ((1 << ray.transform.gameObject.layer) & platformLayer.value) != 0;
		};
	}

	void Update () {
		// Handle velocities
		velocity.x = Input.GetAxis("Horizontal") * speed;

		if (controller.collisionState.becameGroundedThisFrame) {
			onPlayerLand.Invoke();
		}

		if (controller.isGrounded) {
			grounded = true;
			velocity.y = -gravity * Time.deltaTime;

			if (Input.GetButtonDown("Jump")) {
				velocity.y = jumpForce;
				onJump.Invoke();
			}
		} else {
			grounded = false;
			isGroundedOnPlatform = false;

			// If it is falling
			if (velocity.y < 0) {
				velocity.y -= gravity * Time.deltaTime * fallCoef;
			} else {
				velocity.y -= gravity * Time.deltaTime;
			}
		}

		controller.move(velocity * Time.deltaTime);

		// Flip model if needed
		if (velocity.x > 0 && flipX) {
			flipX = false;
			transform.eulerAngles = new Vector3(0, 0, 0);
		} else if (velocity.x < 0 && !flipX) {
			flipX = true;
			transform.eulerAngles = new Vector3(0, 180, 0);
		}

		// Set gun position
		SetGunPosition();
	}

	void SetGunPosition() {
		float upDown = Input.GetAxis("Vertical");

		Vector3 gunRight;

		if (upDown > 0) {
			gunRight = transform.up;
		} else if (upDown < 0) {
			gunRight = -transform.up;
		} else {
			gunRight = transform.right;
		}

		// Velocity x is representative of the input on horizontal axis.
		if (velocity.x != 0) {
			gunRight += transform.right;
			gunRight.Normalize();
		}

		gun.transform.right = gunRight;
	}
}
