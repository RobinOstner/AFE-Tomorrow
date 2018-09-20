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
    private float currentSpeed;
    [SerializeField]
    private float horizontalSpeed;

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
        wantsToShoot = Input.GetButton("Fire1");

        if (isGrounded)
        {
            float absoluteHorizontal = Mathf.Abs(horizontalInput);
            animator.speed = absoluteHorizontal < 0.01f ? 1 : absoluteHorizontal;

            if (shootingDirection.x > 0 && horizontalInput > 0 || shootingDirection.x < 0 && horizontalInput < 0)
            {
                animator.SetFloat("xAxis", absoluteHorizontal);
            }
            else
            {
                animator.SetFloat("xAxis", -absoluteHorizontal);
            }
        }
        else
        {
            animator.speed = 1;
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

    private void GetMotionData()
    {
        currentSpeed = characterRigidbody.velocity.magnitude;
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
