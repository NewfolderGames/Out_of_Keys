using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerController : MonoBehaviour {

	// Uhh...
	// I don't think anyone will read this script, but...
	// I'm really sorry.

	public enum Command { None, Left, Right, Jump, WallJumpLeft, WallJumpRight, Stop }

	[Header("Components")]

	[SerializeField]
	private Transform spritesTransform;
	[SerializeField]
	private Rigidbody2D rigid2d;
	[SerializeField]
	private Animator animator;
	[SerializeField]
	private PlayerKeyboard keyboard;
	[SerializeField]
	private PlayerStatus status;

	[Header("Settings")]

	[SerializeField]
	private bool isEnabled = false;

	[Header("Command")]

	[SerializeField]
	private Command currentCommand;
	[SerializeField]
	private Vector2 input = Vector2.zero;

	[Header("Movement")]

	[SerializeField]
	private Vector2 horizontalMovement = Vector2.zero;
	[SerializeField]
	private Vector2 verticalMovement = Vector2.zero;
	[SerializeField]
	private float movementSpeed = 1;
	[SerializeField]
	private float minimumMovementDistance = 0.01f;
	[Space()]
	[SerializeField]
	private bool isJumping = false;
	[SerializeField]
	private float jumpForce = 5f;
	[SerializeField]
	private float jumpLightMultiplier = 2.5f;
	[Space()]
	[SerializeField]
	private bool isWall = false;
	[SerializeField]
	private bool isWallJump = false;
	[SerializeField]
	private float wallJumpDirection = 0;
	[Space()]
	[SerializeField]
	private bool isGrounded = false;
	[SerializeField]
	private float groundMin = 0.01f;
	[SerializeField]
	private Vector2 groundNormal = Vector2.zero;
	[SerializeField]
	private float groundNormalMinY = 1.0f;
	[SerializeField]
	private ContactFilter2D filter2D = new ContactFilter2D();
	[SerializeField]
	private float gravityMultiplier = 0.1f;

	private void Awake() {

		this.groundNormal = -Physics2D.gravity.normalized;

		this.status.OnReady += () => this.isEnabled = true;
		this.status.OnDie += () => this.isEnabled = false;
		this.status.OnWin += () => this.isEnabled = false;

	}

	private void FixedUpdate() {

		if (!this.isEnabled) return;

		// Flip sprite object's sides.

		if (input.x != 0) this.spritesTransform.localScale = new Vector3(Mathf.Sign(input.x), 1, 1);

		// ===== Horizontal movement =====

		if (this.isGrounded) this.animator.SetBool("is_running", input.x != 0);

		this.horizontalMovement = new Vector2(this.groundNormal.y, -this.groundNormal.x) * input.x * this.movementSpeed * Time.fixedDeltaTime;

		this.isWall = false;

		// Check collision and move.

		Check(this.horizontalMovement);

		// ===== Vertical movement =====

		// Jumping.

		if (((this.isGrounded && !this.isJumping && !this.isWallJump) || (!this.isGrounded && this.isWall && this.isWallJump)) && input.y > 0) {

			if (this.isWallJump) {
				
				this.verticalMovement = -Physics2D.gravity.normalized * this.gravityMultiplier * this.jumpForce;
				this.input.x = this.wallJumpDirection;

			} else this.verticalMovement += -Physics2D.gravity.normalized * this.gravityMultiplier * this.jumpForce;

			this.isJumping = true;

			this.animator.SetBool("is_running", false);
			this.animator.SetBool("is_jumping", true);

		}

		// Gravity.

		var gravity = Physics2D.gravity * this.gravityMultiplier * Time.fixedDeltaTime;
		// if (Vector2.Dot(this.verticalMovement.normalized, this.groundNormal) < 0 && isJumping && !Input.GetButton("Jump")) gravity *= this.jumpLightMultiplier;

		this.verticalMovement += gravity;

		this.isGrounded = false;
		this.isWallJump = false;
		this.input.y = 0;
		this.wallJumpDirection = 0;

		// Check collision and move.

		Check(this.verticalMovement, true);
		
	}
	
	private void Check(Vector2 movement, bool isVertical = false) {

		// Most of code in here are from Unity's 2D platform script.
		// Modified some to make it work with other? things.

		// Original one has lines of code that helps when player bumps on ceiling.
		// I took it out for some reason. I dunno. I need to sleep.

		var distance = movement.magnitude;

		if (distance >= this.minimumMovementDistance) {

			var isHurt = false;
			var hit2Ds = new List<RaycastHit2D>();
			this.rigid2d.Cast(movement.normalized, this.filter2D, hit2Ds, distance + this.groundMin);

			foreach (var hit in hit2Ds) {

				var normal = hit.normal;

				if (hit.normal.y >= this.groundNormalMinY) {
					
					this.isGrounded = true;
					
					if (!isVertical) this.groundNormal = normal;

					if (this.isJumping) {

						this.isJumping = false;
						this.animator.SetBool("is_jumping", false);

					}

					if (isVertical) this.verticalMovement = Vector2.zero;

				}

				if (Mathf.Abs(hit.normal.x) == 1) {

					this.isWall = true;

				}

				var shorterDistance = hit.distance - this.groundMin;
				if (shorterDistance < distance) distance = shorterDistance;

				if (hit.collider.CompareTag("Hurt")) isHurt = true;

			}
			
			this.rigid2d.position += movement.normalized * distance;

			if (isHurt) this.status.Hurt();

		}

	}

	public void Run(Command command) {

		this.currentCommand = command;

		switch (command) {

			case Command.Jump: this.input.y = 1; break;
			case Command.Left: this.input.x = -1; break;
			case Command.Right: this.input.x = 1; break;
			case Command.Stop: this.input.x = 0; break;
			case Command.WallJumpLeft: {
				this.isWallJump = true;
				this.wallJumpDirection = -1;
				this.input.y = 1;
				break;
			}
			case Command.WallJumpRight: {
				this.isWallJump = true;
				this.wallJumpDirection = 1;
				this.input.y = 1;
				break;
			}

		}

	}

}
