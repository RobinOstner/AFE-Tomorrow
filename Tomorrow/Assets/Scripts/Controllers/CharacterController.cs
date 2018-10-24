using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterController : MonoBehaviour {

    public static CharacterController instance;

    public float simulationSpeed;

    private Rigidbody2D characterRigidbody;
    private SpriteRenderer spriteRenderer;
    private Animator animator;
    private CharacterAudioManager characterAudioManager;

    [SerializeField]
    private SpriteRenderer weaponSpriteRenderer;
    [SerializeField]
    private SpriteRenderer upperArmSpriteRenderer;

    [SerializeField]
    private LayerMask floorLayerMask;
    [SerializeField]
    private LayerMask wallLayerMask;
    [SerializeField]
    private LayerMask lilithLayerMask;
    [SerializeField]
    private LayerMask playerLayerMask;

    [SerializeField]
    private Transform groundCheckTransform;
    [SerializeField]
    private float groundCheckRadius;
    public bool isGrounded;
    [SerializeField]
    private Transform wallCheckTransform;
    [SerializeField]
    private float wallCheckRadius;
    private bool isTouchingWall;
    private bool isSlidingOnWall;
    private const float wallSlideLock = 0.5f;
    private float wallSlideLockTimer;

    private float horizontalInput;
    private bool playerWantsToWalk;
    private bool isWalking;
    private float currentVelocity;
    [SerializeField]
    private float walkingSpeed;
    [SerializeField]
    private float runningSpeed;
    [SerializeField]
    private float wallSlideSpeed;
    [SerializeField]
    private float wallSlideAcceleration;
    [SerializeField]
    private float acceleration;
    private bool movingBackwards;
    private float targetVelocity;

    private Vector2 additionalVelocity;

    [SerializeField]
    private float jumpForce;
    [SerializeField]
    private float wallJumpForce;
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

    private bool isSliding;
    
    private float wallSlideDirectionLockTimer;
    [SerializeField]
    private float wallSlideDirectionLock;

    void Start () {
        instance = this;

        characterRigidbody = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        animator = GetComponent<Animator>();
        characterAudioManager = GetComponent<CharacterAudioManager>();

        DisableLilithCollision();
	}
	
	void Update ()
    {
        Time.timeScale = simulationSpeed;

        CheckGrounded();
        CheckWall();

        HandleInputs();
        HandleWallSlideLock();
        HandleWallSlideDirectionLock();
        HandleJumps();

        CalculateShootingDirection();
        PositionArms();

        HandleCoolDown();
        HandleWallSlideLock();
        HandleShooting();
        HandleKickBack();
        HandleAnimation();
        HandleAdditionalVelocity();
	}

    void FixedUpdate()
    {
        ApplyMotion();
    }

    private void DisableLilithCollision()
    {
        int lilithLayerValue = (int)Mathf.Log(lilithLayerMask.value, 2);
        int playerLayerValue = (int)Mathf.Log(playerLayerMask.value, 2);
        Physics2D.IgnoreLayerCollision(lilithLayerValue, playerLayerValue);
    }

    private void ApplyMotion()
    {
        float accelerationFactor = isGrounded ? 1f : 1f;
        currentVelocity = Mathf.Lerp(currentVelocity, targetVelocity, Time.deltaTime * acceleration * accelerationFactor);
        Vector2 horizontal = currentVelocity * Vector2.right;

        if(isSlidingOnWall && wallSlideDirectionLockTimer < wallSlideDirectionLock) { horizontal = Vector2.zero; }

        characterRigidbody.velocity = horizontal + Vector2.up * characterRigidbody.velocity.y + additionalVelocity;

        if (isSlidingOnWall)
        {
            float verticalSpeed = Mathf.Lerp(characterRigidbody.velocity.y, -wallSlideSpeed, Time.deltaTime * wallSlideAcceleration);
            characterRigidbody.velocity = Vector2.right * characterRigidbody.velocity.x + Vector2.up * verticalSpeed;
        }
    }

    private void HandleAdditionalVelocity()
    {
        if (isGrounded) { additionalVelocity /= 4; }
        if (isSlidingOnWall) { additionalVelocity = Vector2.zero; }

        additionalVelocity = Vector2.Lerp(additionalVelocity, Vector2.zero, Time.deltaTime * 2);
    }

    private void HandleWallSlideLock()
    {
        wallSlideLockTimer -= Time.deltaTime;
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

    private void HandleWallSlideDirectionLock()
    {
        if(horizontalInput <= 0.5f && horizontalInput >= -0.5f)
        {
            wallSlideDirectionLockTimer = 0;
        }

        if(horizontalInput == -1 && facingRight && isSlidingOnWall || horizontalInput == 1 && !facingRight && isSlidingOnWall)
        {
            wallSlideDirectionLockTimer += Time.deltaTime;
        }
        else
        {
            wallSlideDirectionLockTimer = 0;
        }
    }

    private void HandleAnimation()
    {
        string clipName = animator.GetCurrentAnimatorClipInfo(0)[0].clip.name;

        if (isGrounded)
        {
            float absoluteSpeed = Mathf.Abs(currentVelocity);

            if (absoluteSpeed < 0.1f) { absoluteSpeed = 0; }

            if (absoluteSpeed < (walkingSpeed + runningSpeed)/2)
            {
                animator.speed = absoluteSpeed < 0.01f ? 1 : absoluteSpeed/walkingSpeed;
            }
            else
            {
                animator.speed = absoluteSpeed < 0.01f ? 1 : absoluteSpeed / runningSpeed;
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

        if(ShouldPlaySlideAnimation())
        {
            isSliding = true;
            characterAudioManager.PlaySlidingSound();
            animator.Play("Slide", 0);
            animator.speed = 1;
        }
    }

    private bool ShouldPlaySlideAnimation()
    {
        bool wantsStop = Mathf.Abs(targetVelocity) <= 0.2f && Mathf.Abs(currentVelocity) > (walkingSpeed + runningSpeed)*3/4;

        bool wantsWalk = playerWantsToWalk && Mathf.Abs(currentVelocity) > (walkingSpeed + runningSpeed)/2;

        return !isSliding && isGrounded && (wantsStop || wantsWalk);
    }

    private void HandleJumps()
    {
        if (wantsToJump && isGrounded)
        {
            characterRigidbody.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
            animator.Play("Jump");
            isSliding = false;
            characterAudioManager.PlayJumpSound();
        }
        else if (wantsToJump && extraJumps > 0)
        {
            if (isSlidingOnWall)
            {
                Vector2 wallForce = Vector2.left * wallJumpForce;
                wallForce *= facingRight ? 1 : -1;
                additionalVelocity += wallForce;
                wallSlideLockTimer = wallSlideLock;
                isSlidingOnWall = false;
                animator.Play("Jump");
            }

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
        Physics2D.IgnoreCollision(GetComponent<Collider2D>(), bullet.GetComponent<Collider2D>());
        bullet.Initialize(shootingDirection, bulletSpeed);

        kickBackPosition = -shootingDirection.normalized * kickBackAmount;

        characterAudioManager.PlayShootingSound();
    }

    private void HandleCoolDown()
    {
        shootingCoolDownTimer -= Time.deltaTime;
        shootingCoolDownTimer = Mathf.Clamp(shootingCoolDownTimer, 0, shootingCoolDown);
    }

    private void CheckGrounded()
    {
        isGrounded = Physics2D.OverlapCircle(groundCheckTransform.position, groundCheckRadius, floorLayerMask);

        if(!animator.GetBool("Grounded") && isGrounded)
        {
            characterAudioManager.PlayLandingSound();
        }

        animator.SetBool("Grounded", isGrounded);

        if (isGrounded)
        {
            extraJumps = maxExtraJumps;
        }
    }

    private void CheckWall()
    {
        isTouchingWall = Physics2D.OverlapCircle(wallCheckTransform.position, wallCheckRadius, wallLayerMask);
        
        bool before = isSlidingOnWall;
        
        isSlidingOnWall = isTouchingWall && !isGrounded && wallSlideLockTimer <= 0;
        
        if(!before && isSlidingOnWall)
        {
            animator.Play("Wall Slide");
        }

        if (isSlidingOnWall)
        {
            extraJumps = maxExtraJumps;
        }

        animator.SetBool("Wall Slide", isSlidingOnWall);
    }

    public void FlipCharacter()
    {
        facingRight = !facingRight;
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
        if (!isSlidingOnWall && (shootingDirection.x < 0 && facingRight || shootingDirection.x >= 0 && !facingRight))
        {
            FlipCharacter();
        }

        float shootingDirectionAngle = Vector3.Angle(shootingDirection, Vector3.down);
        if (shootingDirection.x < 0)
        {
            shootingDirectionAngle *= -1;
        }

        upperAngleOffset = Mathf.Cos((shootingDirectionAngle - 90) / 180 * Mathf.PI) * 80;

        if (isSlidingOnWall) { upperAngleOffset /= 2; }

        lowerArmAngle = shootingDirectionAngle;
        upperArmAngle = shootingDirectionAngle - upperAngleOffset;

        Vector3 upperDirection = Quaternion.Euler(0, 0, -upperAngleOffset) * shootingDirection;
        upperArm.transform.position = (Vector3)kickBackPosition + shoulder.transform.position + upperArm.transform.localScale.y * upperDirection / 2f;
        upperArm.transform.rotation = Quaternion.identity;
        upperArm.transform.Rotate(Vector3.forward, upperArmAngle);

        Vector3 lowerDirection = shootingDirection;
        lowerArm.transform.position = elbow.transform.position + lowerArm.transform.localScale.y * lowerDirection / 2f;
        lowerArm.transform.rotation = Quaternion.identity;
        lowerArm.transform.Rotate(Vector3.forward, lowerArmAngle);
    }

    public void StoppedSliding()
    {
        isSliding = false;
    }
}
