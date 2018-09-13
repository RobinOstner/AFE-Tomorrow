using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterController : MonoBehaviour {

    private Rigidbody2D characterRigidbody;
    private SpriteRenderer spriteRenderer;
    private Animator animator;

    [SerializeField]
    private LayerMask floorLayerMask;

    [SerializeField]
    private Transform groundCheckTransform;
    [SerializeField]
    private float groundCheckRadius;
    private bool isGrounded;

    private float horizontalInput;
    private float currentSpeed;
    [SerializeField]
    private float horizontalSpeed;

    [SerializeField]
    private float jumpForce;
    private bool wantsToJump;
    [SerializeField]
    private int maxExtraJumps;
    private int extraJumps;

    private bool facingRight;

    
	void Start () {
        characterRigidbody = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        animator = GetComponent<Animator>();
	}
	
	void Update ()
    {
        CheckGrounded();

        HandleInputs();
        HandleJumps();

        GetMotionData();
	}

    void FixedUpdate()
    {
        ApplyMotion();
    }

    private void HandleInputs()
    {
        horizontalInput = Input.GetAxis("Horizontal");
        wantsToJump = Input.GetButtonDown("Jump");

        float absoluteHorizontal = Mathf.Abs(horizontalInput);
        animator.SetFloat("xAxis", absoluteHorizontal);
        animator.speed = absoluteHorizontal;

        if (!facingRight && horizontalInput > 0 || facingRight && horizontalInput < 0)
        {
            FlipCharacter();
        }
    }

    private void ApplyMotion()
    {
        Vector2 horizontalVelocity = Vector2.right * horizontalInput * horizontalSpeed;
        characterRigidbody.velocity = horizontalVelocity + Vector2.up * characterRigidbody.velocity.y;
    }

    private void HandleJumps()
    {
        if (wantsToJump && isGrounded)
        {
            characterRigidbody.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
        }
        else if (wantsToJump && extraJumps > 0)
        {
            characterRigidbody.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
            extraJumps--;
        }
    }

    private void CheckGrounded()
    {
        isGrounded = Physics2D.OverlapCircle(groundCheckTransform.position, groundCheckRadius, floorLayerMask);

        if (isGrounded)
        {
            extraJumps = maxExtraJumps;
        }
    }

    private void GetMotionData()
    {
        currentSpeed = characterRigidbody.velocity.magnitude;
    }

    private void FlipCharacter()
    {
        facingRight = !facingRight;
        spriteRenderer.flipX = !facingRight;
    }
}
