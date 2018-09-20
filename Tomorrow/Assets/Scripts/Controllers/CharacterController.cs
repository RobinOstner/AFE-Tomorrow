using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterController : MonoBehaviour {

    private Rigidbody2D characterRigidbody;
    private SpriteRenderer spriteRenderer;
    private Animator animator;

    [SerializeField]
    private SpriteRenderer weaponSpriteRenderer;
    [SerializeField]
    private SpriteRenderer upperArmSpriteRenderer;

    [SerializeField]
    private LayerMask floorLayerMask;

    [SerializeField]
    private Transform groundCheckTransform;
    [SerializeField]
    private float groundCheckRadius;
    private bool isGrounded;

    private float horizontalInput;
    private bool playerWantsToWalk;
    private bool isWalking;
    private float currentVelocity;
    [SerializeField]
    private float walkingSpeed;
    [SerializeField]
    private float runningSpeed;
    [SerializeField]
    private float acceleration;
    private bool movingBackwards;
    private float targetVelocity;

    [SerializeField]
    private float jumpForce;
    private bool wantsToJump;
    [SerializeField]
    private int maxExtraJumps;
    private int extraJumps;

    private bool facingRight = true;

    private Vector3 mousePosition;
    private Vector3 mousePositionWorld;
    private Vector3 shootingDirection;

    [SerializeField]
    private GameObject shoulder;
    [SerializeField]
    private GameObject elbow;
    [SerializeField]
    private GameObject hand;
    [SerializeField]
    private GameObject upperArm;
    [SerializeField]
    private GameObject lowerArm;

    private float upperArmAngle;
    private float lowerArmAngle;
    private float upperAngleOffset;

    [SerializeField]
    private GameObject bulletPrefab;
    private bool wantsToShoot;
    [SerializeField]
    private float shootingCoolDown;
    private float shootingCoolDownTimer;
    [SerializeField]
    private float bulletSpeed;
    
    private Vector2 kickBackPosition;
    public float kickBackAmount;
    public float kickBackSpeed;

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

        CalculateShootingDirection();
        PositionArms();

        HandleCoolDown();
        HandleShooting();
        HandleKickBack();
        HandleAnimation();
	}

    void FixedUpdate()
    {
        ApplyMotion();
    }

    private void HandleInputs()
    {
        horizontalInput = Input.GetAxis("Horizontal");
        wantsToJump = Input.GetButtonDown("Jump");
        wantsToShoot = Input.GetButton("Fire1");
        playerWantsToWalk = Input.GetButton("Run");

        targetVelocity = horizontalInput;
        isWalking = (playerWantsToWalk || movingBackwards) && isGrounded;
        targetVelocity *= isWalking ? walkingSpeed : runningSpeed;
    }

    private void ApplyMotion()
    {
        currentVelocity = Mathf.Lerp(currentVelocity, targetVelocity, Time.deltaTime * acceleration);
        characterRigidbody.velocity = currentVelocity * Vector2.right + Vector2.up * characterRigidbody.velocity.y;
    }

    private void HandleAnimation()
    {
        if (isGrounded)
        {
            float absoluteSpeed = Mathf.Abs(currentVelocity);
            if(absoluteSpeed < walkingSpeed)
            {
                isWalking = true;
            }
            absoluteSpeed /= isWalking ? walkingSpeed : runningSpeed;

            animator.speed = absoluteSpeed < 0.01f ? 1 : absoluteSpeed;

            if (!isWalking)
            {
                absoluteSpeed *= 2;
            }

            movingBackwards = !(shootingDirection.x > 0 && currentVelocity > 0 || shootingDirection.x < 0 && currentVelocity < 0);
            if (!movingBackwards)
            {
                animator.SetFloat("xAxis", absoluteSpeed);
            }
            else
            {
                animator.SetFloat("xAxis", -absoluteSpeed);
            }
        }
        else
        {
            animator.speed = 1;
        }
    }

    private void HandleJumps()
    {
        if (wantsToJump && isGrounded)
        {
            characterRigidbody.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
            animator.Play("Jump");
        }
        else if (wantsToJump && extraJumps > 0)
        {
            characterRigidbody.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
            extraJumps--;
        }
    }

    private void HandleShooting()
    {
        if(wantsToShoot && shootingCoolDownTimer == 0)
        {
            Shoot();
            shootingCoolDownTimer = shootingCoolDown;
        }
    }

    private void HandleKickBack()
    {
        kickBackPosition = Vector2.Lerp(kickBackPosition, Vector2.zero, Time.deltaTime * kickBackSpeed);
    }

    private void Shoot()
    {
        BulletController bullet = Instantiate(bulletPrefab, hand.transform.position, Quaternion.identity).GetComponent<BulletController>();
        bullet.Initialize(shootingDirection, bulletSpeed);
        Physics2D.IgnoreCollision(GetComponent<Collider2D>(), bullet.GetComponent<Collider2D>());

        kickBackPosition = -shootingDirection.normalized * kickBackAmount;
    }

    private void HandleCoolDown()
    {
        shootingCoolDownTimer -= Time.deltaTime;
        shootingCoolDownTimer = Mathf.Clamp(shootingCoolDownTimer, 0, shootingCoolDown);
    }

    private void CheckGrounded()
    {
        isGrounded = Physics2D.OverlapCircle(groundCheckTransform.position, groundCheckRadius, floorLayerMask);

        animator.SetBool("Grounded", isGrounded);

        if (isGrounded)
        {
            extraJumps = maxExtraJumps;
        }
    }

    public void FlipCharacter()
    {
        facingRight = !facingRight;
        /*
        spriteRenderer.flipX = !facingRight;
        weaponSpriteRenderer.flipY = !facingRight;
        upperArmSpriteRenderer.flipX = !facingRight;
        shoulder.transform.position -= Vector3.right*(shoulder.transform.position.x-transform.position.x)*2;
        */
        transform.localScale = facingRight ? new Vector3(1, 1, 1) : new Vector3(-1,1,1);
    }

    private void CalculateShootingDirection()
    {
        mousePosition = Input.mousePosition;
        mousePosition.z = -Camera.main.transform.position.z;
        mousePositionWorld = Camera.main.ScreenToWorldPoint(mousePosition);

        shootingDirection = (mousePositionWorld - transform.position).normalized;
    }

    private void PositionArms()
    {
        float shootingDirectionAngle = Vector3.Angle(shootingDirection, Vector3.down);
        if (shootingDirection.x < 0)
        {
            shootingDirectionAngle *= -1;
        }

        upperAngleOffset = Mathf.Cos((shootingDirectionAngle - 90) / 180 * Mathf.PI) * 80;

        lowerArmAngle = shootingDirectionAngle;
        upperArmAngle = shootingDirectionAngle - upperAngleOffset;

        Vector3 upperDirection = Quaternion.Euler(0, 0, -upperAngleOffset) * shootingDirection;
        upperArm.transform.position = (Vector3)kickBackPosition + shoulder.transform.position + upperArm.transform.localScale.y * upperDirection / 2f;
        upperArm.transform.rotation = Quaternion.identity;
        upperArm.transform.Rotate(Vector3.forward, upperArmAngle);

        Vector3 lowerDirection = shootingDirection;
        lowerArm.transform.position = elbow.transform.position + lowerArm.transform.localScale.y * shootingDirection / 2f;
        lowerArm.transform.rotation = Quaternion.identity;
        lowerArm.transform.Rotate(Vector3.forward, lowerArmAngle);

        if (shootingDirection.x < 0 && facingRight || shootingDirection.x >= 0 && !facingRight)
        {
            FlipCharacter();
        }
    }
}
