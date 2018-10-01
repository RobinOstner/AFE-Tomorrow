using System.Collections;
using System.Collections.Generic;
using UnityEngine;

struct SurroundingCheck
{
    public Ray2D ray;
    public Vector2 direction;
    public float distance;
    public LayerMask layer;
    public Vector2 normal;
}

public class LilithController : MonoBehaviour {

    private Rigidbody2D rigidbody;
    private Animator animator;

    [SerializeField]
    private LayerMask walkableLayerMask;

    [SerializeField]
    private float floorDistance;

    [SerializeField]
    private Transform groundCheckTransform;
    private bool isGrounded;
    [SerializeField]
    private float groundCheckRadius;
    [SerializeField]
    private float surroundingCheckRadius;
    [SerializeField]
    private float minWallDistance;
    private float distanceToWall;
    
    private SurroundingCheck downRay = new SurroundingCheck();
    private SurroundingCheck frontRay = new SurroundingCheck();
    private SurroundingCheck backRay = new SurroundingCheck();
    private SurroundingCheck upRay = new SurroundingCheck();
    private SurroundingCheck walkForwardRay = new SurroundingCheck();
    
    private Vector2 walkDirection;
    [SerializeField]
    private float walkSpeed;

    public bool flip;

    private bool isFlipped;

    // Use this for initialization
    void Start () {
        rigidbody = GetComponent<Rigidbody2D>();

        SetupSurroundingChecks();
	}
	
	// Update is called once per frame
	void Update () {
        if(flip && !isFlipped || !flip && isFlipped)
        {
            FlipCharacter();
        }

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
        frontRay.direction = Vector2.right;
        backRay.direction = Vector2.left;
        upRay.direction = Vector2.up;
    }

    private void CheckSurroundings()
    {
        downRay = CheckRay(downRay, true);
        frontRay = CheckRay(frontRay, true);
        backRay = CheckRay(backRay, true);
        upRay = CheckRay(upRay, true);
    }

    private SurroundingCheck CheckRay(SurroundingCheck surroundingCheck, bool useLocalRotation)
    {
        surroundingCheck.distance = surroundingCheckRadius;
        if (useLocalRotation)
        {
            surroundingCheck.ray = new Ray2D(transform.position, transform.rotation * surroundingCheck.direction * surroundingCheckRadius);
        }
        else
        {
            surroundingCheck.ray = new Ray2D(transform.position, surroundingCheck.direction * surroundingCheckRadius);
        }

        RaycastHit2D result = Physics2D.Raycast(transform.position, surroundingCheck.ray.direction, surroundingCheckRadius, walkableLayerMask);
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
        isGrounded = Physics2D.OverlapCircle(groundCheckTransform.position, groundCheckRadius, walkableLayerMask);

        rigidbody.isKinematic = isGrounded;

        if (isGrounded)
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

    private void HandleMovement()
    {
        if(isGrounded)
        {
            rigidbody.velocity = walkDirection * walkSpeed;

            if(walkForwardRay.distance <= floorDistance)
            {
                float angle = isFlipped ? -90 : 90;
                transform.Rotate(0, 0, angle);
            }
            else
            {
                transform.rotation = CalculateNormalRotation();
                AdjustToFloorDistance();
            }
        }
    }

    private Quaternion CalculateNormalRotation()
    {
        return Quaternion.LookRotation(Vector3.forward, downRay.normal);
    }
    
    private void AdjustToFloorDistance()
    {
        CheckSurroundings();

        Vector3 intersect = transform.position + transform.rotation*downRay.direction * downRay.distance;
        transform.position = intersect - transform.rotation*downRay.direction.normalized * floorDistance;
    }

    private void FlipCharacter()
    {

        isFlipped = !isFlipped;
        transform.localScale = !isFlipped ? new Vector3(1, 1, 1) : new Vector3(-1, 1, 1);

        frontRay.direction *= -1;
        backRay.direction *= -1;
    }
}
