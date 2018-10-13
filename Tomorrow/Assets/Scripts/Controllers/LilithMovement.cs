﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LilithMovement : MonoBehaviour {

    private LilithSurroundingAwareness surroundingAwareness;

    private new Rigidbody2D rigidbody;

    public bool isAttached;

    public enum Surfaces { none, multiple, top, right, left, bottom }
    public Surfaces attachedSurface;

    public Vector3 surfaceNormal
    {
        get
        {
            switch (attachedSurface)
            {
                case Surfaces.left:
                    return Vector3.right;
                case Surfaces.right:
                    return Vector3.left;
                case Surfaces.top:
                    return Vector3.down;
                case Surfaces.bottom:
                    return Vector3.up;
                default:
                    return Vector3.forward;
            }
        }
    }

    public float walkSpeed;
    
	void Start () {
        surroundingAwareness = GetComponent<LilithSurroundingAwareness>();
        rigidbody = GetComponent<Rigidbody2D>();
        rigidbody.useFullKinematicContacts = true;

    }
	
	void Update () {
        HandleAttaching();

        if (isAttached)
        {
            WalkForward();
        }

        if (surroundingAwareness.possibleCorners.Count > 0)
        {
            HandleCorners();
        }
	}

    private void HandleAttaching()
    {
        if (surroundingAwareness.canAttach)
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
            attachedSurface = Surfaces.none;
        }
    }

    private void ChooseAttachSurface()
    {
        if (surroundingAwareness.canAttachUp && !surroundingAwareness.canAttachLeft && !surroundingAwareness.canAttachRight && !surroundingAwareness.canAttachDown )
        {
            attachedSurface = Surfaces.top;
        }
        else if (!surroundingAwareness.canAttachUp && !surroundingAwareness.canAttachLeft && surroundingAwareness.canAttachRight && !surroundingAwareness.canAttachDown)
        {
            attachedSurface = Surfaces.right;
        }
        else if (!surroundingAwareness.canAttachUp && surroundingAwareness.canAttachLeft && !surroundingAwareness.canAttachRight && !surroundingAwareness.canAttachDown)
        {
            attachedSurface = Surfaces.left;
        }
        else if (!surroundingAwareness.canAttachUp && !surroundingAwareness.canAttachLeft && !surroundingAwareness.canAttachRight && surroundingAwareness.canAttachDown)
        {
            attachedSurface = Surfaces.bottom;
        }
        else
        {
            HandleMultipleSurfaces();
        }
    }

    private void HandleMultipleSurfaces()
    {
        if (attachedSurface == Surfaces.left && surroundingAwareness.canAttachDown)
        {
            attachedSurface = Surfaces.bottom;
        }
        if(attachedSurface == Surfaces.bottom && surroundingAwareness.canAttachRight)
        {
            attachedSurface = Surfaces.right;
        }
        if(attachedSurface == Surfaces.right && surroundingAwareness.canAttachUp)
        {
            attachedSurface = Surfaces.top;
        }
        if(attachedSurface == Surfaces.top && surroundingAwareness.canAttachLeft)
        {
            attachedSurface = Surfaces.left;
        }

        ReAdjustToSurface();
    }

    private void ReAdjustToSurface()
    {
        Vector3 direction = -surfaceNormal;

        RaycastHit2D result = Physics2D.Raycast(transform.position, direction, surroundingAwareness.bodySize * 2, surroundingAwareness.getWalkableLayerMask());

        if(result.collider != null)
        {
            transform.position += direction * (result.distance - surroundingAwareness.bodySize);
        }
    }

    private void WalkForward()
    {
        Vector2 walkDirection = Quaternion.Euler(0, 0, -90) * surfaceNormal;

        rigidbody.velocity = walkDirection * walkSpeed;
    }

    private void HandleCorners()
    {
        Vector3 difference = surroundingAwareness.possibleCorners[0] - transform.position;
        transform.position += difference * 2;
    }
}
