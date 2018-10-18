using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LilithMovement : MonoBehaviour {

    private LilithSurroundingAwareness surroundingAwareness;

    private new Rigidbody2D rigidbody;

    private Coroutine innerCornerCoroutine;
    private Coroutine outerCornerCoroutine;

    public bool isDead;
    public bool isWalkingAroundCorner;
    public bool isAttached;
    public bool isJumping;

    public float jumpLockTime;

    public float jumpProbability;

    public float walkSpeed;

    public float jumpSpeed;
    
	void Start () {
        surroundingAwareness = GetComponent<LilithSurroundingAwareness>();
        rigidbody = GetComponent<Rigidbody2D>();
        rigidbody.useFullKinematicContacts = true;
    }
	
	void Update () {
        if (!isJumping)
        {
            HandleAttaching();
        }

        if (!isWalkingAroundCorner)
        {
            if (isAttached)
            {
                HandleJumping();

                if (!isJumping)
                {
                    WalkForward();
                }
            }

            if (!isJumping && surroundingAwareness.possibleCorners.Count > 0)
            {
                HandleOuterCorner();
            }
        }
	}

    private void HandleAttaching()
    {
        if (!isDead && surroundingAwareness.canAttach)
        {
            rigidbody.velocity = Vector3.zero;
            rigidbody.isKinematic = true;
            isAttached = true;

            ChooseAttachSurface();
        }
        else
        {
            rigidbody.isKinematic = false;
            isAttached = false;
            surroundingAwareness.attachedSurface = LilithSurroundingAwareness.Surfaces.none;
        }
    }

    private void HandleJumping()
    {
        if(surroundingAwareness.possibleJumpLocations.Count > 0)
        {
            if(Random.Range(0f, 100f) <= jumpProbability)
            {
                isJumping = true;
                isAttached = false;

                int selectedJumpLocationIndex = Random.Range(0, surroundingAwareness.possibleJumpLocations.Count);

                Vector2 direction = surroundingAwareness.possibleJumpLocations[selectedJumpLocationIndex] - transform.position;

                rigidbody.velocity = direction.normalized * jumpSpeed;
                rigidbody.isKinematic = false;

                StartCoroutine(JumpLock());
            }
        }
    }

    private IEnumerator JumpLock()
    {
        yield return new WaitForSeconds(jumpLockTime);
        isJumping = false;
    }

    private void ChooseAttachSurface()
    {
        if (surroundingAwareness.canAttachUp && !surroundingAwareness.canAttachLeft && !surroundingAwareness.canAttachRight && !surroundingAwareness.canAttachDown )
        {
            surroundingAwareness.attachedSurface = LilithSurroundingAwareness.Surfaces.top;
        }
        else if (!surroundingAwareness.canAttachUp && !surroundingAwareness.canAttachLeft && surroundingAwareness.canAttachRight && !surroundingAwareness.canAttachDown)
        {
            surroundingAwareness.attachedSurface = LilithSurroundingAwareness.Surfaces.right;
        }
        else if (!surroundingAwareness.canAttachUp && surroundingAwareness.canAttachLeft && !surroundingAwareness.canAttachRight && !surroundingAwareness.canAttachDown)
        {
            surroundingAwareness.attachedSurface = LilithSurroundingAwareness.Surfaces.left;
        }
        else if (!surroundingAwareness.canAttachUp && !surroundingAwareness.canAttachLeft && !surroundingAwareness.canAttachRight && surroundingAwareness.canAttachDown)
        {
            surroundingAwareness.attachedSurface = LilithSurroundingAwareness.Surfaces.bottom;
        }
        else
        {
            HandleMultipleSurfaces();
        }
    }

    private void HandleMultipleSurfaces()
    {
        if (surroundingAwareness.attachedSurface == LilithSurroundingAwareness.Surfaces.left && surroundingAwareness.canAttachDown)
        {
            surroundingAwareness.attachedSurface = LilithSurroundingAwareness.Surfaces.bottom;
            HandleInnerCorner();
        }
        if(surroundingAwareness.attachedSurface == LilithSurroundingAwareness.Surfaces.bottom && surroundingAwareness.canAttachRight)
        {
            surroundingAwareness.attachedSurface = LilithSurroundingAwareness.Surfaces.right;
            HandleInnerCorner();
        }
        if(surroundingAwareness.attachedSurface == LilithSurroundingAwareness.Surfaces.right && surroundingAwareness.canAttachUp)
        {
            surroundingAwareness.attachedSurface = LilithSurroundingAwareness.Surfaces.top;
            HandleInnerCorner();
        }
        if(surroundingAwareness.attachedSurface == LilithSurroundingAwareness.Surfaces.top && surroundingAwareness.canAttachLeft)
        {
            surroundingAwareness.attachedSurface = LilithSurroundingAwareness.Surfaces.left;
            HandleInnerCorner();
        }

        ReAdjustToSurface();

    }

    private void ReAdjustToSurface()
    {
        Vector3 direction = -surroundingAwareness.surfaceNormal;

        RaycastHit2D result = Physics2D.Raycast(transform.position, direction, surroundingAwareness.bodySize * 2, surroundingAwareness.getWalkableLayerMask());

        if(result.collider != null)
        {
            transform.position += direction * (result.distance - surroundingAwareness.bodySize);
        }
    }

    private void WalkForward()
    {
        Vector2 walkDirection = Quaternion.Euler(0, 0, -90) * surroundingAwareness.surfaceNormal;

        rigidbody.velocity = walkDirection * walkSpeed;
    }

    private void HandleOuterCorner()
    {
        if (outerCornerCoroutine == null)
        {
            outerCornerCoroutine = StartCoroutine(OuterCornerCoroutine());
        }
    }

    private IEnumerator OuterCornerCoroutine()
    {
        isWalkingAroundCorner = true;

        yield return new WaitForSeconds(1);

        Vector3 difference = surroundingAwareness.possibleCorners[0] - transform.position;
        transform.position += difference * 2;

        isWalkingAroundCorner = false;
        outerCornerCoroutine = null;
    }

    private void HandleInnerCorner()
    {
        if(innerCornerCoroutine == null)
        {
            innerCornerCoroutine = StartCoroutine(InnerCornerCoroutine());
        }
    }

    private IEnumerator InnerCornerCoroutine()
    {
        isWalkingAroundCorner = true;
        yield return new WaitForSeconds(1);
        isWalkingAroundCorner = false;
        innerCornerCoroutine = null;
    }
}
