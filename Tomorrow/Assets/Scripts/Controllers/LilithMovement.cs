using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LilithMovement : MonoBehaviour {

    private LilithSurroundingAwareness surroundingAwareness;

    private LilithAnimationController animationController;

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

    private Vector3 oldPosition;

    public Vector2 currentDirection
    {
        get
        {
            return transform.position - oldPosition;
        }
    }
    
	void Start () {
        surroundingAwareness = GetComponent<LilithSurroundingAwareness>();
        animationController = GetComponentInChildren<LilithAnimationController>();
        rigidbody = GetComponent<Rigidbody2D>();
        rigidbody.useFullKinematicContacts = true;
    }

    void Update()
    {
        if (!isJumping)
        {
            HandleAttaching();
        }

        if (isAttached && !isWalkingAroundCorner)
        {
            HandleJumping();

            if (!isJumping)
            {
                WalkForward();
            }

            if (!isJumping && surroundingAwareness.possibleCorners.Count > 0)
            {
                HandleOuterCorner();
            }
        }

        UpdateOldValues();
    }

    private void UpdateOldValues()
    {
        oldPosition = transform.position;
    }

    private void HandleAttaching()
    {
        if(innerCornerCoroutine != null || outerCornerCoroutine != null)
        {
            rigidbody.velocity = Vector2.zero;
            rigidbody.isKinematic = true;
            isAttached = true;
            return;
        }

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

        animationController.AdjustRotation();
    }

    private void HandleJumping()
    {
        if(surroundingAwareness.possibleJumpLocations.Count > 0)
        {
            if(Random.Range(0f, 100f) <= jumpProbability)
            {
                isJumping = true;
                isAttached = true;

                int selectedJumpLocationIndex = Random.Range(0, surroundingAwareness.possibleJumpLocations.Count);

                Vector2 direction = surroundingAwareness.possibleJumpLocations[selectedJumpLocationIndex] - transform.position;

                if (isDiagonal(direction)) { animationController.PlayDiagonalJump(direction); }
                else {
                    animationController.PlayVerticalJump(direction);
                }
            }
        }
    }

    public void Jump(Vector3 direction)
    {
        rigidbody.velocity = (direction + Vector3.up * direction.magnitude/2) * jumpSpeed;
        rigidbody.isKinematic = false;

        StartCoroutine(JumpLock());
    }

    private bool isDiagonal(Vector3 jumpDirection)
    {
        jumpDirection = jumpDirection.normalized;

        if(jumpDirection == Vector3.up) { return false; }
        if(jumpDirection == Vector3.left) { return false; }
        if(jumpDirection == Vector3.right) { return false; }
        if(jumpDirection == Vector3.down) { return false; }

        return true;
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
            HandleInnerCorner(LilithSurroundingAwareness.Surfaces.bottom);
        }
        if(surroundingAwareness.attachedSurface == LilithSurroundingAwareness.Surfaces.bottom && surroundingAwareness.canAttachRight)
        {
            HandleInnerCorner(LilithSurroundingAwareness.Surfaces.right);
        }
        if(surroundingAwareness.attachedSurface == LilithSurroundingAwareness.Surfaces.right && surroundingAwareness.canAttachUp)
        {
            HandleInnerCorner(LilithSurroundingAwareness.Surfaces.top);
        }
        if(surroundingAwareness.attachedSurface == LilithSurroundingAwareness.Surfaces.top && surroundingAwareness.canAttachLeft)
        {
            HandleInnerCorner(LilithSurroundingAwareness.Surfaces.left);
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

        Vector3 cornerPosition = surroundingAwareness.possibleCorners[0];

        AdjustToOuterCornerPosition();

        LilithSurroundingAwareness.Surfaces nextSurface = CalculateNextSurfaceOuterCorner();

        Vector3 difference = cornerPosition - transform.position;

        animationController.PlayOuterCornerAnimation(nextSurface, difference);

        yield return new WaitUntil(() => animationController.outerCornerAnimationFinished);

        Debug.Log("Outer Corner Finished!");

        animationController.outerCornerAnimationFinished = false;

        yield return null;

        isWalkingAroundCorner = false;
        outerCornerCoroutine = null;
    }

    private void AdjustToOuterCornerPosition()
    {
        transform.position -= surroundingAwareness.distanceToCorner.normalized - surroundingAwareness.distanceToCorner;
    }

    private LilithSurroundingAwareness.Surfaces CalculateNextSurfaceOuterCorner()
    {
        switch (surroundingAwareness.attachedSurface)
        {
            case LilithSurroundingAwareness.Surfaces.bottom:
                return LilithSurroundingAwareness.Surfaces.left;
            case LilithSurroundingAwareness.Surfaces.left:
                return LilithSurroundingAwareness.Surfaces.top;
            case LilithSurroundingAwareness.Surfaces.top:
                return LilithSurroundingAwareness.Surfaces.right;
            case LilithSurroundingAwareness.Surfaces.right:
                return LilithSurroundingAwareness.Surfaces.bottom;
            default:
                return LilithSurroundingAwareness.Surfaces.none;
        }
    }

    private void HandleInnerCorner(LilithSurroundingAwareness.Surfaces nextSurface)
    {
        if(innerCornerCoroutine == null)
        {
            animationController.PlayInnerCornerAnimation(nextSurface);
            innerCornerCoroutine = StartCoroutine(InnerCornerCoroutine());
        }
    }

    private IEnumerator InnerCornerCoroutine()
    {
        isWalkingAroundCorner = true;

        yield return new WaitUntil(() => animationController.innerCornerAnimationFinished);

        Debug.Log("Inner Corner Finished!");

        animationController.innerCornerAnimationFinished = false;

        isWalkingAroundCorner = false;
        innerCornerCoroutine = null;
    }
}
