using System.Collections;
using System.Collections.Generic;
using UnityEngine;

struct SurroundingCheck
{
    public Ray2D ray;
    public Vector2 direction;
    public Vector2 offset;
    public float distance;
    public LayerMask layer;
    public Vector2 normal;
}

public class LilithController : MonoBehaviour {

    private Rigidbody2D rigidbody;
    private Animator animator;

    [SerializeField]
    private LayerMask playerLayerMask;

    private enum WalkModes { stopped, walking }
    [SerializeField]
    private WalkModes walkMode;
    [SerializeField]
    private float minWalkModeWaitTime = 1, maxWalkModeWaitTime = 5;

    [SerializeField]
    private LayerMask walkableLayerMask;
    [SerializeField]
    private LayerMask floorLayerMask;

    [SerializeField]
    private float floorDistance;

    [SerializeField]
    private Transform groundCheckTransform;
    public bool isGrounded
    {
        get { return middleIsGrounded || frontIsGrounded || backIsGrounded; }
    }
    public bool middleIsGrounded;
    public bool frontIsGrounded;
    public bool backIsGrounded;
    [SerializeField]
    private float groundCheckRadius;
    [SerializeField]
    private float surroundingCheckRadius;
    [SerializeField]
    private float minWallDistance;
    private float distanceToWall;
    [SerializeField]
    private float rotationDistance;

    [SerializeField]
    private float frontDownRayDistance;
    
    private SurroundingCheck downRay = new SurroundingCheck();
    private SurroundingCheck frontDownRay = new SurroundingCheck();
    private SurroundingCheck backDownRay = new SurroundingCheck();
    private SurroundingCheck frontRay = new SurroundingCheck();
    private SurroundingCheck backRay = new SurroundingCheck();
    private SurroundingCheck upRay = new SurroundingCheck();
    private SurroundingCheck walkForwardRay = new SurroundingCheck();
    
    private Vector2 walkDirection;
    [SerializeField]
    private float walkSpeed;

    [SerializeField]
    private bool walksOnFloor;
    
    private bool isFlipped;

    private float hurtTimer;
    [SerializeField]
    private float maxHurtTimer = 0.5f;
    private bool isHurt = false;

    [SerializeField]
    private float minVerticalDifferenceToAttackFromAbove;
    [SerializeField]
    private float maxVerticalDifferenceToAttackFromSide;
    [SerializeField]
    private float maxHorizontalDifferenceToAttackFromAbove;
    [SerializeField]
    private float maxHorizontalDifferenceToAttackFromSide;

    private Vector2 playerDirection;

    private bool canAttackFromAbove;
    private bool canAttackFromSide;
    [SerializeField]
    private float attackFromAboveVelocity;
    [SerializeField]
    private float attackFromSideVelocity;

    // Use this for initialization
    void Start () {
        rigidbody = GetComponent<Rigidbody2D>();

        SetupSurroundingChecks();

        StartCoroutine(MovementBehaviour());
	}
	
	// Update is called once per frame
	void Update ()
    {
        HandleBulletHits();

        CheckSurroundings();
        CheckGrounded();

        CalculateWalkDirection();

        HandleMovement();
	}

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawRay(transform.position, downRay.ray.direction * downRay.distance);
        Gizmos.DrawWireSphere(transform.position + (Vector3)downRay.ray.direction * downRay.distance, 0.25f);

        Gizmos.DrawRay(transform.position + transform.rotation * frontDownRay.offset * frontDownRayDistance, frontDownRay.ray.direction * frontDownRay.distance);
        Gizmos.DrawWireSphere(transform.position + transform.rotation * frontDownRay.offset * frontDownRayDistance + (Vector3)frontDownRay.ray.direction * frontDownRay.distance, 0.25f);

        Gizmos.DrawRay(transform.position + transform.rotation * backDownRay.offset * frontDownRayDistance, backDownRay.ray.direction * backDownRay.distance);
        Gizmos.DrawWireSphere(transform.position + transform.rotation * backDownRay.offset * frontDownRayDistance + (Vector3)backDownRay.ray.direction * backDownRay.distance, 0.25f);

        Gizmos.color = Color.cyan;

        Gizmos.DrawRay(transform.position, frontRay.ray.direction * frontRay.distance);
        Gizmos.DrawWireSphere(transform.position + (Vector3)frontRay.ray.direction * frontRay.distance, 0.25f);
        
        Gizmos.DrawRay(transform.position, backRay.ray.direction * backRay.distance);
        Gizmos.DrawWireSphere(transform.position + (Vector3)backRay.ray.direction * backRay.distance, 0.25f);
        
        Gizmos.DrawRay(transform.position, upRay.ray.direction * upRay.distance);
        Gizmos.DrawWireSphere(transform.position + (Vector3)upRay.ray.direction * upRay.distance, 0.25f);

        Gizmos.color = Color.magenta;
        Gizmos.DrawRay(transform.position, (Vector3)walkDirection * 3f);
        Gizmos.DrawWireSphere(transform.position + (Vector3)walkDirection * 3f, 0.25f);
    }

    private void SetupSurroundingChecks()
    {
        downRay.direction = Vector2.down;
        frontDownRay.direction = Vector2.down;
        frontDownRay.offset = Vector2.right * frontDownRayDistance;
        backDownRay.direction = Vector2.down;
        backDownRay.offset = Vector2.left * frontDownRayDistance;
        frontRay.direction = Vector2.right;
        backRay.direction = Vector2.left;
        upRay.direction = Vector2.up;
    }

    private void CheckSurroundings()
    {
        downRay = CheckRay(downRay, true);
        frontDownRay = CheckRay(frontDownRay, true);
        backDownRay = CheckRay(backDownRay, true);
        frontRay = CheckRay(frontRay, true);
        backRay = CheckRay(backRay, true);
        upRay = CheckRay(upRay, true);
    }

    private SurroundingCheck CheckRay(SurroundingCheck surroundingCheck, bool useLocalRotation)
    {
        surroundingCheck.distance = surroundingCheckRadius;
        if (useLocalRotation)
        {
            surroundingCheck.ray = new Ray2D(transform.position + transform.rotation * surroundingCheck.offset, transform.rotation * surroundingCheck.direction * surroundingCheckRadius);
        }
        else
        {
            surroundingCheck.ray = new Ray2D(transform.position + (Vector3)surroundingCheck.offset, surroundingCheck.direction * surroundingCheckRadius);
        }

        RaycastHit2D result = Physics2D.Raycast(transform.position + transform.rotation * surroundingCheck.offset, surroundingCheck.ray.direction, surroundingCheckRadius, walkableLayerMask);
        if (result.collider != null)
        {
            surroundingCheck.distance = result.distance;
            surroundingCheck.layer = LayerMask.GetMask(LayerMask.LayerToName(result.collider.gameObject.layer));
            surroundingCheck.normal = result.normal;
        }

        return surroundingCheck;
    }

    private void CheckGrounded()
    {
        middleIsGrounded = downRay.distance <= groundCheckRadius;
        frontIsGrounded = frontDownRay.distance <= groundCheckRadius;
        backIsGrounded = backDownRay.distance <= groundCheckRadius;

        rigidbody.isKinematic = isGrounded && !isHurt;

        if (isGrounded && !isHurt)
        {
            rigidbody.velocity = Vector2.zero;
        }
        else
        {
            transform.rotation = Quaternion.identity;
        }
    }

    private void CalculateWalkDirection()
    {
        if (isGrounded)
        {
            walkDirection = Quaternion.Euler(0,0,-90) * downRay.normal;
            walkDirection *= isFlipped ? -1 : 1;
            walkForwardRay.direction = walkDirection;
            walkForwardRay = CheckRay(walkForwardRay, false);
        }
        else
        {
            walkDirection = Vector2.zero;
        }
    }

    private IEnumerator MovementBehaviour()
    {
        float waitTime = Random.Range(minWalkModeWaitTime, maxWalkModeWaitTime);
        yield return new WaitForSeconds(waitTime);

        if (walkMode == WalkModes.walking)
        {
            walkMode = WalkModes.stopped;

            if(Random.Range(0,5) == 0)
            {
                FlipCharacter();
            }
        }
        else
        {
            walkMode = WalkModes.walking;
        }

        StartCoroutine(MovementBehaviour());
    }

    private void HandleMovement()
    {
        if(isGrounded && !isHurt)
        {
            switch (walkMode)
            {
                case WalkModes.stopped:
                    ReadjustPositionRotation();
                    break;
                case WalkModes.walking:
                    Walk();
                    break;
                default:
                    break;
            }
        }
    }

    private void HandleBulletHits()
    {
        hurtTimer -= Time.deltaTime;

        isHurt = hurtTimer > 0;
    }

    private void Walk()
    {
        rigidbody.velocity = walkDirection * walkSpeed;

        if(!walksOnFloor && walkForwardRay.layer == floorLayerMask)
        {
            FlipCharacter();
            CalculateWalkDirection();
        }

        if (walkForwardRay.distance <= floorDistance)
        {
            float angle = isFlipped ? -90 : 90;
            transform.Rotate(0, 0, angle);
        }
        else if (!frontIsGrounded && !middleIsGrounded && backIsGrounded)
        {
            float angle = isFlipped ? 90 : -90;
            Vector3 throughPoint = transform.position + (Vector3)downRay.ray.direction * rotationDistance;
            Vector3 axis = new Vector3(0, 0, 1);
            transform.RotateAround(throughPoint, axis, angle);
        }
        else if (!backIsGrounded && !middleIsGrounded && frontIsGrounded)
        {
            float angle = isFlipped ? 90 : -90;
            Vector3 throughPoint = transform.position + (Vector3)downRay.ray.direction * rotationDistance;
            Vector3 axis = new Vector3(0, 0, 1);
            transform.RotateAround(throughPoint, axis, angle);
        }
        else
        {
            ReadjustPositionRotation();
        }
    }

    private void ReadjustPositionRotation()
    {
        transform.rotation = CalculateNormalRotation();
        AdjustToFloorDistance();
    }

    private Quaternion CalculateNormalRotation()
    {
        Vector3 normal = (downRay.normal + frontDownRay.normal + backDownRay.normal) / 3f;
        return Quaternion.LookRotation(Vector3.forward, downRay.normal);
    }

    private void AdjustToFloorDistance()
    {
        CheckSurroundings();

        Vector3 intersect = transform.position + transform.rotation * downRay.direction * downRay.distance;
        transform.position = intersect - transform.rotation * downRay.direction.normalized * floorDistance;
    }

    public void Hit()
    {
        hurtTimer = maxHurtTimer;
    }

    private void FlipCharacter()
    {
        isFlipped = !isFlipped;
        transform.localScale = !isFlipped ? new Vector3(1, 1, 1) : new Vector3(-1, 1, 1);

        frontRay.direction *= -1;
        backRay.direction *= -1;
    }
}
