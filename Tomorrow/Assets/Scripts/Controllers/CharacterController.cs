﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterController : MonoBehaviour {

    public static CharacterController instance;

    public float simulationSpeed;

    private Rigidbody2D rigidbody;
    private SpriteRenderer spriteRenderer;
    private Animator animator;
    private CharacterAudioManager characterAudioManager;


    [Header("Environment")]
    [SerializeField]
    private float groundCheckRadius;
    [SerializeField]
    private float wallCheckRadius;
    public bool isGrounded;
    private bool isTouchingWall;
    private bool isSlidingOnWall;
    private const float wallSlideLock = 0.5f;
    private float wallSlideLockTimer;

    private float horizontalInput;
    private bool playerWantsToWalk;
    private bool isWalking;
    private float currentVelocity;
    private bool wantsToShoot;

    private bool wantsToJump;
    private bool movingBackwards;
    private bool facingRight = true;
    private bool isBraking
    {
        get
        {
            return (targetVelocity > 0 && currentVelocity < 0) || (targetVelocity < 0 && currentVelocity > 0) || targetVelocity == 0 || (targetVelocity > 0 && currentVelocity > 0 && currentVelocity > targetVelocity) || (targetVelocity < 0 && currentVelocity < 0 && currentVelocity < targetVelocity);
        }
    }
    private bool isSliding;

    [Header("Movement Settings")]
    [SerializeField]
    private float walkingSpeed;
    [SerializeField]
    private float runningSpeed;
    [SerializeField]
    private float acceleration;
    [SerializeField]
    private float breakSpeed;
    [SerializeField]
    private float jumpForce;
    private float maxAirVelocity;
    [SerializeField]
    private float maxAirVelocityLerpSpeed;
    [SerializeField]
    private float fallSpeedMultiplier;
    [SerializeField]
    private float wallJumpForce;
    [SerializeField]
    private float wallJumpUpwardsForce;

    [SerializeField]
    private float wallSlideSpeed;
    [SerializeField]
    private float wallSlideAcceleration;
    private float wallSlideDirectionLockTimer;
    [SerializeField]
    private float wallSlideDirectionLock;

    private float targetVelocity;

    private Vector2 additionalVelocity;

    [SerializeField]
    private int maxExtraJumps;
    private int extraJumps;

    [SerializeField]
    private float shootingCoolDown;
    private float shootingCoolDownTimer;
    [SerializeField]
    private float bulletSpeed;

    private Vector2 kickBackPosition;
    public float kickBackAmount;
    public float kickBackSpeed;

    [SerializeField]
    private float terminalVelocity;

    private Vector3 mousePosition;
    private Vector3 mousePositionWorld;
    private Vector3 shootingDirection;

    [Header("Externals")]
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
    private Transform wallCheckTransform;
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
    [SerializeField]
    private GameObject bulletPrefab;

    private float upperArmAngle;
    private float lowerArmAngle;
    private float upperAngleOffset;


    void Start () {
        instance = this;

        rigidbody = GetComponent<Rigidbody2D>();
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

    void OnGUI()
    {
        GUI.color = Color.black;
        GUI.Label(new Rect(10, 10, 300, 20), "Current Speed: " + (int)currentVelocity + "\t/\t" + (int)rigidbody.velocity.y);
        GUI.Label(new Rect(10, 30, 300, 20), "Target Speed: " + (int)targetVelocity);
        GUI.Label(new Rect(10, 50, 300, 20), "Braking: " + isBraking);
    }

    private void DisableLilithCollision()
    {
        int lilithLayerValue = (int)Mathf.Log(lilithLayerMask.value, 2);
        int playerLayerValue = (int)Mathf.Log(playerLayerMask.value, 2);
        Physics2D.IgnoreLayerCollision(lilithLayerValue, playerLayerValue);
    }

    private void ApplyMotion()
    {
        float accelerationFactor = isBraking ? breakSpeed : acceleration;
        Vector2 horizontal = currentVelocity * Vector2.right;

        if(currentVelocity < targetVelocity && !isBraking)
        {
            float newVelocity = currentVelocity + acceleration * Time.deltaTime;
            if (newVelocity > targetVelocity) { currentVelocity = targetVelocity; }
            else
            {
                currentVelocity = newVelocity;
            }
        }
        if(currentVelocity > targetVelocity && !isBraking)
        {
            float newVelocity = currentVelocity - acceleration * Time.deltaTime;
            if (newVelocity < targetVelocity) { currentVelocity = targetVelocity; }
            else
            {
                currentVelocity = newVelocity;
            }
        }
        if(currentVelocity > targetVelocity && isBraking)
        {
            float newVelocity = currentVelocity - breakSpeed * Time.deltaTime;
            if (newVelocity < targetVelocity) { currentVelocity = targetVelocity; }
            else
            {
                currentVelocity = newVelocity;
            }
        }
        if(currentVelocity < targetVelocity && isBraking)
        {
            float newVelocity = currentVelocity + breakSpeed * Time.deltaTime;
            if (newVelocity > targetVelocity) { currentVelocity = targetVelocity; }
            else
            {
                currentVelocity = newVelocity;
            }
        }

        if (isGrounded)
        {
            horizontal = currentVelocity * Vector2.right;
            maxAirVelocity = Mathf.Abs(currentVelocity);
        }
        else
        {
            maxAirVelocity = Mathf.Lerp(maxAirVelocity, runningSpeed, Time.deltaTime * maxAirVelocityLerpSpeed);
            currentVelocity = currentVelocity > 0 ? Mathf.Min(maxAirVelocity, currentVelocity) : Mathf.Max(-maxAirVelocity, currentVelocity);
            horizontal = currentVelocity * Vector2.right;
        }

        if(isSlidingOnWall && wallSlideDirectionLockTimer < wallSlideDirectionLock) { horizontal = Vector2.zero; }

        rigidbody.velocity = horizontal + Vector2.up * rigidbody.velocity.y + additionalVelocity;


        if (isSlidingOnWall)
        {
            //float verticalSpeed = Mathf.Lerp(rigidbody.velocity.y, -wallSlideSpeed, Time.deltaTime * wallSlideAcceleration);
            //verticalSpeed = Mathf.Min(verticalSpeed, 2);
            //rigidbody.velocity = Vector2.right * rigidbody.velocity.x + Vector2.up * verticalSpeed;
            rigidbody.velocity += Physics2D.gravity * (wallSlideAcceleration - 1) * Time.deltaTime;
        }
        else
        {
            if (rigidbody.velocity.y < 0)
            {
                rigidbody.velocity += Physics2D.gravity * (fallSpeedMultiplier - 1) * Time.deltaTime;
            }
            if (rigidbody.velocity.y >= 0 && !Input.GetButton("Jump"))
            {
                rigidbody.velocity += Physics2D.gravity * (fallSpeedMultiplier - 1) * Time.deltaTime;
            }
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

            if (absoluteSpeed < 4)
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
        bool wantsStop = Mathf.Abs(targetVelocity) <= 0.2f && Mathf.Abs(currentVelocity) > walkingSpeed + (runningSpeed - walkingSpeed)*3/4f;

        bool wantsWalk = playerWantsToWalk && Mathf.Abs(currentVelocity) > walkingSpeed + (runningSpeed - walkingSpeed) / 2f;

        return !isSliding && isGrounded && (wantsStop || wantsWalk);
    }

    private void HandleJumps()
    {
        if (wantsToJump && isGrounded)
        {
            rigidbody.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
            animator.Play("Jump");
            isSliding = false;
            characterAudioManager.PlayJumpSound();
        }
        else if (wantsToJump && isSlidingOnWall)
        {
            if (isSlidingOnWall)
            {
                Vector2 wallForce = Vector2.left * wallJumpForce;
                wallForce *= facingRight ? 1 : -1;
                currentVelocity = wallForce.x;
                //additionalVelocity += wallForce;
                float appliedForce = wallJumpUpwardsForce - Mathf.Max(rigidbody.velocity.y, 0);
                appliedForce = appliedForce > 0 ? appliedForce : 0;
                rigidbody.AddForce(Vector2.up * appliedForce, ForceMode2D.Impulse);
                wallSlideLockTimer = wallSlideLock;
                maxAirVelocity = runningSpeed;
                isSlidingOnWall = false;
                animator.Play("Jump");
                characterAudioManager.PlayWallJumpSound();
            }
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
            if(rigidbody.velocity.y < -terminalVelocity)
            {
                transform.position = Respawn.lastRespawnPosition;
                rigidbody.velocity = Vector2.zero;
            }
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
            rigidbody.velocity = Vector2.up * rigidbody.velocity.y *0.2f;
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
