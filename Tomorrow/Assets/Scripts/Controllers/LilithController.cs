using System.Collections;
using System.Collections.Generic;
using UnityEngine;

struct SurroundingCheck
{
    public Ray2D ray;
    public Vector2 direction;
    public float distance;
    public LayerMask layer;
}

public class LilithController : MonoBehaviour {

    private Rigidbody2D rigidbody;
    private Animator animator;

    [SerializeField]
    private LayerMask walkableLayerMask;

    [SerializeField]
    private Transform groundCheckTransform;
    private bool isGrounded;
    [SerializeField]
    private float surroundingCheckRadius;
    
    private SurroundingCheck downRay = new SurroundingCheck();
    private SurroundingCheck frontRay = new SurroundingCheck();
    private SurroundingCheck backRay = new SurroundingCheck();
    private SurroundingCheck upRay = new SurroundingCheck();

    // Use this for initialization
    void Start () {
        rigidbody = GetComponent<Rigidbody2D>();

        rigidbody.isKinematic = true;

        SetupSurroundingChecks();
	}
	
	// Update is called once per frame
	void Update () {
        CheckSurroundings();
	}

    private void SetupSurroundingChecks()
    {
        downRay.direction = Vector2.down;
        frontRay.direction = Vector2.right;
        backRay.direction = Vector2.left;
        upRay.direction = Vector2.up;
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
    }

    private void CheckSurroundings()
    {
        downRay = CheckRay(downRay);
        frontRay = CheckRay(frontRay);
        backRay = CheckRay(backRay);
        upRay = CheckRay(upRay);
    }

    private SurroundingCheck CheckRay(SurroundingCheck surroundingCheck)
    {
        surroundingCheck.distance = surroundingCheckRadius;
        surroundingCheck.ray = new Ray2D(transform.position, transform.rotation * surroundingCheck.direction * surroundingCheckRadius);

        RaycastHit2D result = Physics2D.Raycast(transform.position, surroundingCheck.ray.direction, surroundingCheckRadius, walkableLayerMask);
        if (result.collider != null)
        {
            surroundingCheck.distance = result.distance;
            surroundingCheck.layer = LayerMask.GetMask(LayerMask.LayerToName(result.collider.gameObject.layer));
        }

        return surroundingCheck;
    }
}
