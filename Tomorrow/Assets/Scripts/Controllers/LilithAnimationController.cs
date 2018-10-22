using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LilithAnimationController : MonoBehaviour {

    private LilithSurroundingAwareness surroundingAwareness;
    private LilithMovement movement;

    private Animator animator;

    public bool innerCornerAnimationFinished = false;
    public bool outerCornerAnimationFinished = false;

    private LilithSurroundingAwareness.Surfaces nextSurface;

    private Vector3 outerCornerDifference;

    private Vector3 jumpDirection;

	// Use this for initialization
	void Start () {
        animator = GetComponent<Animator>();
        movement = GetComponentInParent<LilithMovement>();
        surroundingAwareness = GetComponentInParent<LilithSurroundingAwareness>();
	}
	
	// Update is called once per frame
	void Update () {
        HandleFlying();
	}

    public void AdjustRotation()
    {
        switch (surroundingAwareness.attachedSurface)
        {
            case LilithSurroundingAwareness.Surfaces.bottom:
                transform.rotation = Quaternion.Euler(0,0,0);
                break;
            case LilithSurroundingAwareness.Surfaces.right:
                transform.rotation = Quaternion.Euler(0, 0, 90);
                break;
            case LilithSurroundingAwareness.Surfaces.left:
                transform.rotation = Quaternion.Euler(0, 0, -90);
                break;
            case LilithSurroundingAwareness.Surfaces.top:
                transform.rotation = Quaternion.Euler(0, 0, 180);
                break;
            default:
                break;
        }
    }

    public void PlayVerticalJump(Vector3 direction)
    {
        jumpDirection = direction;
        animator.Play("Jump Vertical");
    }

    public void StartVerticalJump()
    {
        movement.Jump(jumpDirection);
        jumpDirection = Vector3.zero;
    }

    public void PlayDiagonalJump(Vector3 direction)
    {
        jumpDirection = direction;
        animator.Play("Jump Diagonal");
    }

    public void StartDiagonalJump()
    {
        movement.Jump(jumpDirection);
        jumpDirection = Vector3.zero;
    }

    public void PlayInnerCornerAnimation(LilithSurroundingAwareness.Surfaces nextSurface)
    {
        this.nextSurface = nextSurface;
        animator.Play("Inner Corner");
    }

    public void InnerCornerAnimationFinished()
    {
        innerCornerAnimationFinished = true;
        surroundingAwareness.attachedSurface = nextSurface;

        AdjustRotation();
    }

    public void PlayOuterCornerAnimation(LilithSurroundingAwareness.Surfaces nextSurface, Vector3 difference)
    {
        outerCornerDifference = difference;
        this.nextSurface = nextSurface;
        animator.Play("Outer Corner");
    }

    public void OuterCornerAnimationFinished()
    { 
        transform.parent.position += outerCornerDifference * 2;

        outerCornerAnimationFinished = true;
        surroundingAwareness.attachedSurface = nextSurface;

        AdjustRotation();
    }

    public void HandleFlying()
    {
        animator.SetFloat("Horizontal", movement.currentDirection.x);
        animator.SetFloat("Vertical", movement.currentDirection.y);

        if (animator.GetCurrentAnimatorStateInfo(0).IsName("Flying"))
        {
            if (movement.isAttached && !animator.GetCurrentAnimatorStateInfo(0).IsName("Walking"))
            {
                animator.Play("Walking");
            }
        }
    }

    public void TriggerFlying()
    {
        transform.rotation = Quaternion.identity;
    }
}
