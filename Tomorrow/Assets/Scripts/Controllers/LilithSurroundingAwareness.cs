using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(LilithMovement))]
public class LilithSurroundingAwareness : MonoBehaviour {

    [SerializeField]
    private LayerMask walkableLayerMask;

    private LilithMovement movement;

    public SurroundingCheckRay rightCheckRay;
    public SurroundingCheckRay leftCheckRay;
    public SurroundingCheckRay upCheckRay;
    public SurroundingCheckRay downCheckRay;

    public Vector3 leftBottomCorner;
    public Vector3 leftTopCorner;
    public Vector3 rightBottomCorner;
    public Vector3 rightTopCorner;
    public Vector3 topLeftCorner;
    public Vector3 topRightCorner;
    public Vector3 bottomLeftCorner;
    public Vector3 bottomRightCorner;

    public Vector3 distanceToCorner;

    [SerializeField]
    private float attachableDistance;

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

    [SerializeField]
    private float maxJumpDistance;
    [SerializeField]
    private float minJumpDistance;

    public bool canAttachRight
    {
        get
        {
            return rightCheckRay.distance < attachableDistance;
        }
    }
    public bool canAttachLeft
    {
        get
        {
            return leftCheckRay.distance < attachableDistance;
        }
    }
    public bool canAttachUp
    {
        get
        {
            return upCheckRay.distance < attachableDistance;
        }
    }
    public bool canAttachDown
    {
        get
        {
            return downCheckRay.distance < attachableDistance;
        }
    }
    public bool canAttach
    {
        get
        {
            return canAttachRight || canAttachLeft || canAttachDown || canAttachUp;
        }
    }

    public JumpCheckRay rightJumpRay;
    public JumpCheckRay leftJumpRay;
    public JumpCheckRay upJumpRay;
    public JumpCheckRay downJumpRay;
    public JumpCheckRay upLeftJumpRay;
    public JumpCheckRay upRightJumpRay;
    public JumpCheckRay downLeftJumpRay;
    public JumpCheckRay downRightJumpRay;

    public bool canJumpRight
    {
        get
        {
            return rightJumpRay.canJump;
        }
    }
    public bool canJumpLeft
    {
        get
        {
            return leftJumpRay.canJump;
        }
    }
    public bool canJumpUp
    {
        get
        {
            return upJumpRay.canJump;
        }
    }
    public bool canJumpDown
    {
        get
        {
            return downJumpRay.canJump;
        }
    }
    public bool canJumpUpLeft
    {
        get
        {
            return upLeftJumpRay.canJump;
        }
    }
    public bool canJumpUpRight
    {
        get
        {
            return upRightJumpRay.canJump;
        }
    }
    public bool canJumpDownLeft
    {
        get
        {
            return downLeftJumpRay.canJump;
        }
    }
    public bool canJumpDownRight
    {
        get
        {
            return downRightJumpRay.canJump;
        }
    }

    public List<Vector3> possibleJumpLocations
    {
        get
        {
            List<Vector3> jumpLocations = new List<Vector3>();

            if (canJumpLeft && !isWalkingDirection(leftJumpRay.direction)) { jumpLocations.Add(transform.position + (Vector3)leftJumpRay.direction * leftJumpRay.distance); }
            if (canJumpRight && !isWalkingDirection(rightJumpRay.direction)) { jumpLocations.Add(transform.position + (Vector3)rightJumpRay.direction * rightJumpRay.distance); }
            if (canJumpUp && !isWalkingDirection(upJumpRay.direction)) { jumpLocations.Add(transform.position + (Vector3)upJumpRay.direction * upJumpRay.distance); }
            if (canJumpDown && !isWalkingDirection(downJumpRay.direction)) { jumpLocations.Add(transform.position + (Vector3)downJumpRay.direction * downJumpRay.distance); }
            if (canJumpUpLeft && !isWalkingDirection(upLeftJumpRay.direction) && isForwardDirection(upLeftJumpRay.direction)) { jumpLocations.Add(transform.position + (Vector3)upLeftJumpRay.direction * upLeftJumpRay.distance); }
            if (canJumpUpRight && !isWalkingDirection(upRightJumpRay.direction) && isForwardDirection(upRightJumpRay.direction)) { jumpLocations.Add(transform.position + (Vector3)upRightJumpRay.direction * upRightJumpRay.distance); }
            if (canJumpDownLeft && !isWalkingDirection(downLeftJumpRay.direction) && isForwardDirection(downLeftJumpRay.direction)) { jumpLocations.Add(transform.position + (Vector3)downLeftJumpRay.direction * downLeftJumpRay.distance); }
            if (canJumpDownRight && !isWalkingDirection(downRightJumpRay.direction) && isForwardDirection(downRightJumpRay.direction)) { jumpLocations.Add(transform.position + (Vector3)downRightJumpRay.direction * downRightJumpRay.distance); }

            return jumpLocations;
        }
    }

    public List<Vector3> possibleCorners
    {
        get
        {
            List<Vector3> corners = new List<Vector3>();

            Vector3 nil = Vector3.back * 500;

            if(leftBottomCorner != nil) { corners.Add(leftBottomCorner); }
            if (leftTopCorner != nil) { corners.Add(leftTopCorner); }
            if (rightBottomCorner != nil) { corners.Add(rightBottomCorner); }
            if (rightTopCorner != nil) { corners.Add(rightTopCorner); }
            if (topLeftCorner != nil) { corners.Add(topLeftCorner); }
            if (topRightCorner != nil) { corners.Add(topRightCorner); }
            if (bottomLeftCorner != nil) { corners.Add(bottomLeftCorner); }
            if (bottomRightCorner != nil) { corners.Add(bottomRightCorner); }

            return corners;
        }
    }
    
    public float bodySize;
    
	void Start () {
        movement = GetComponent<LilithMovement>();

        InitializeCorners();
        SetupSurroundingCheckRays();
        SetupJumpCheckRays();
	}
	
	void Update () {
        CheckSurroundingRays();
        CheckCorners();
        CheckJumpRays();
	}

    private void OnDrawGizmos()
    {
        Gizmos.color = rightCheckRay.gizmosColor;
        Gizmos.DrawRay(transform.position + (Vector3)rightCheckRay.offset, rightCheckRay.direction * rightCheckRay.distance);
        Gizmos.DrawWireSphere(transform.position + (Vector3)rightCheckRay.offset + (Vector3)rightCheckRay.direction * rightCheckRay.distance, 0.1f);

        Gizmos.color = leftCheckRay.gizmosColor;
        Gizmos.DrawRay(transform.position + (Vector3)leftCheckRay.offset, leftCheckRay.direction * leftCheckRay.distance);
        Gizmos.DrawWireSphere(transform.position + (Vector3)leftCheckRay.offset + (Vector3)leftCheckRay.direction * leftCheckRay.distance, 0.1f);

        Gizmos.color = upCheckRay.gizmosColor;
        Gizmos.DrawRay(transform.position + (Vector3)upCheckRay.offset, upCheckRay.direction * upCheckRay.distance);
        Gizmos.DrawWireSphere(transform.position + (Vector3)upCheckRay.offset + (Vector3)upCheckRay.direction * upCheckRay.distance, 0.1f);

        Gizmos.color = downCheckRay.gizmosColor;
        Gizmos.DrawRay(transform.position + (Vector3)downCheckRay.offset, downCheckRay.direction * downCheckRay.distance);
        Gizmos.DrawWireSphere(transform.position + (Vector3)downCheckRay.offset + (Vector3)downCheckRay.direction * downCheckRay.distance, 0.1f);

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(leftBottomCorner, 0.1f);
        Gizmos.DrawWireSphere(leftTopCorner, 0.1f);
        Gizmos.DrawWireSphere(rightTopCorner, 0.1f);
        Gizmos.DrawWireSphere(rightBottomCorner, 0.1f);
        Gizmos.DrawWireSphere(topLeftCorner, 0.1f);
        Gizmos.DrawWireSphere(topRightCorner, 0.1f);
        Gizmos.DrawWireSphere(bottomLeftCorner, 0.1f);
        Gizmos.DrawWireSphere(bottomRightCorner, 0.1f);

        Gizmos.color = leftJumpRay.canJump ? Color. green :Color.blue;
        Gizmos.DrawRay(transform.position, leftJumpRay.direction * leftJumpRay.distance);
        Gizmos.color = rightJumpRay.canJump ? Color.green : Color.blue;
        Gizmos.DrawRay(transform.position, rightJumpRay.direction * rightJumpRay.distance);
        Gizmos.color = upJumpRay.canJump ? Color.green : Color.blue;
        Gizmos.DrawRay(transform.position, upJumpRay.direction * upJumpRay.distance);
        Gizmos.color = downJumpRay.canJump ? Color.green : Color.blue;
        Gizmos.DrawRay(transform.position, downJumpRay.direction * downJumpRay.distance);
        Gizmos.color = upLeftJumpRay.canJump ? Color.green : Color.blue;
        Gizmos.DrawRay(transform.position, upLeftJumpRay.direction * upLeftJumpRay.distance);
        Gizmos.color = upRightJumpRay.canJump ? Color.green : Color.blue;
        Gizmos.DrawRay(transform.position, upRightJumpRay.direction * upRightJumpRay.distance);
        Gizmos.color = downLeftJumpRay.canJump ? Color.green : Color.blue;
        Gizmos.DrawRay(transform.position, downLeftJumpRay.direction * downLeftJumpRay.distance);
        Gizmos.color = downRightJumpRay.canJump ? Color.green : Color.blue;
        Gizmos.DrawRay(transform.position, downRightJumpRay.direction * downRightJumpRay.distance);
    }

    private void SetupSurroundingCheckRays()
    {
        rightCheckRay.direction = Vector2.right;
        rightCheckRay.offset = Vector2.right * bodySize;
        rightCheckRay.color = Color.cyan;
        leftCheckRay.direction = Vector2.left;
        leftCheckRay.offset = Vector2.left * bodySize;
        leftCheckRay.color = Color.blue;
        upCheckRay.direction = Vector2.up;
        upCheckRay.offset = Vector2.up * bodySize;
        upCheckRay.color = Color.yellow;
        downCheckRay.direction = Vector2.down;
        downCheckRay.offset = Vector2.down * bodySize;
        downCheckRay.color = Color.magenta;
    }

    private void SetupJumpCheckRays()
    {
        rightJumpRay.direction = Vector2.right;
        rightJumpRay.maxJumpDistance = maxJumpDistance;
        rightJumpRay.minJumpDistance = minJumpDistance;
        leftJumpRay.direction = Vector2.left;
        leftJumpRay.maxJumpDistance = maxJumpDistance;
        leftJumpRay.minJumpDistance = minJumpDistance;
        upJumpRay.direction = Vector2.up;
        upJumpRay.maxJumpDistance = maxJumpDistance;
        upJumpRay.minJumpDistance = minJumpDistance;
        downJumpRay.direction = Vector2.down;
        downJumpRay.maxJumpDistance = maxJumpDistance;
        downJumpRay.minJumpDistance = minJumpDistance;
        upLeftJumpRay.direction = new Vector2(-1, 1).normalized;
        upLeftJumpRay.maxJumpDistance = maxJumpDistance;
        upLeftJumpRay.minJumpDistance = minJumpDistance;
        upRightJumpRay.direction = new Vector2(1,1).normalized;
        upRightJumpRay.maxJumpDistance = maxJumpDistance;
        upRightJumpRay.minJumpDistance = minJumpDistance;
        downLeftJumpRay.direction = new Vector2(-1,-1).normalized;
        downLeftJumpRay.maxJumpDistance = maxJumpDistance;
        downLeftJumpRay.minJumpDistance = minJumpDistance;
        downRightJumpRay.direction = new Vector2(1,-1).normalized;
        downRightJumpRay.maxJumpDistance = maxJumpDistance;
        downRightJumpRay.minJumpDistance = minJumpDistance;
    }

    private bool isWalkingDirection(Vector3 jumpDirection)
    {
        return (jumpDirection == Quaternion.Euler(0, 0, 90) * surfaceNormal || jumpDirection == Quaternion.Euler(0, 0, -90) * surfaceNormal);
    }

    private bool isForwardDirection(Vector3 jumpDirection)
    {
        if (!movement.oppositeDirection)
        {
            if (surfaceNormal == Vector3.left && jumpDirection.y > 0) { return true; }
            if (surfaceNormal == Vector3.right && jumpDirection.y < 0) { return true; }
            if (surfaceNormal == Vector3.up && jumpDirection.x > 0) { return true; }
            if (surfaceNormal == Vector3.down && jumpDirection.x < 0) { return true; }
        }
        else
        {
            if (surfaceNormal == Vector3.left && jumpDirection.y < 0) { return true; }
            if (surfaceNormal == Vector3.right && jumpDirection.y > 0) { return true; }
            if (surfaceNormal == Vector3.up && jumpDirection.x < 0) { return true; }
            if (surfaceNormal == Vector3.down && jumpDirection.x > 0) { return true; }
        }
        return false;
    }

    private void InitializeCorners()
    {
        Vector3 nil = Vector3.back * 500;

        leftBottomCorner = nil;
        leftTopCorner = nil;
        rightBottomCorner = nil;
        rightTopCorner = nil;
        topLeftCorner = nil;
        topRightCorner = nil;
        bottomLeftCorner = nil;
        bottomRightCorner = nil;
    }

    private void CheckSurroundingRays()
    {
        rightCheckRay.CheckRay(transform, attachableDistance, walkableLayerMask);
        leftCheckRay.CheckRay(transform, attachableDistance, walkableLayerMask);
        upCheckRay.CheckRay(transform, attachableDistance, walkableLayerMask);
        downCheckRay.CheckRay(transform, attachableDistance, walkableLayerMask);
    }

    private void CheckJumpRays()
    {
        rightJumpRay.CheckRay(transform, walkableLayerMask);
        leftJumpRay.CheckRay(transform, walkableLayerMask);
        upJumpRay.CheckRay(transform, walkableLayerMask);
        downJumpRay.CheckRay(transform, walkableLayerMask);
        upLeftJumpRay.CheckRay(transform, walkableLayerMask);
        upRightJumpRay.CheckRay(transform, walkableLayerMask);
        downLeftJumpRay.CheckRay(transform, walkableLayerMask);
        downRightJumpRay.CheckRay(transform, walkableLayerMask);
    }

    private void CheckCorners()
    {
        if (!movement.oppositeDirection)
        {
            leftBottomCorner = CheckCorner(canAttachLeft, true, leftCheckRay);
            rightTopCorner = CheckCorner(canAttachRight, true, rightCheckRay);
            topLeftCorner = CheckCorner(canAttachUp, true, upCheckRay);
            bottomRightCorner = CheckCorner(canAttachDown, true, downCheckRay);
        }
        else
        {
            leftTopCorner = CheckCorner(canAttachLeft, false, leftCheckRay);
            rightBottomCorner = CheckCorner(canAttachRight, false, rightCheckRay);
            topRightCorner = CheckCorner(canAttachUp, false, upCheckRay);
            bottomLeftCorner = CheckCorner(canAttachDown, false, downCheckRay);
        }
    }

    private Vector3 CheckCorner(bool canAttach, bool option, SurroundingCheckRay surroundingCheckRay)
    {
        if (canAttach)
        {
            if (IsCorner(surroundingCheckRay, option))
            {
                Vector2 checkPosition = CalculateCornerCheckPosition(surroundingCheckRay, option);
                Vector2 checkDirection = CalculateCornerCheckDirection(surroundingCheckRay, option);

                RaycastHit2D result = Physics2D.Raycast(checkPosition, checkDirection, bodySize, walkableLayerMask);

                if (result.collider != null)
                {
                    Vector2 cornerPosition = checkPosition + checkDirection * result.distance + surroundingCheckRay.direction * -0.01f;
                    CalculateCornerDistance(surroundingCheckRay, cornerPosition);
                    return cornerPosition;
                }
            }
        }

        return Vector3.back * 500;
    }

    private bool IsCorner(SurroundingCheckRay surroundingCheckRay, bool option)
    {
        if (option)
        {
            Vector3 start = transform.position + (Vector3)surroundingCheckRay.direction * bodySize + (Vector3)surroundingCheckRay.direction * surroundingCheckRay.distance;
            Vector3 offset = Quaternion.Euler(0, 0, 90) * surroundingCheckRay.direction * bodySize;
            Vector3 checkPosition = start + offset;

            Vector3 checkDirection = surroundingCheckRay.direction;

            RaycastHit2D result = Physics2D.Raycast(checkPosition, checkDirection, bodySize, walkableLayerMask);

            return result.collider == null;
        }
        else
        {
            Vector3 start = transform.position + (Vector3)surroundingCheckRay.direction * bodySize + (Vector3)surroundingCheckRay.direction * surroundingCheckRay.distance;
            Vector3 offset = Quaternion.Euler(0, 0, -90) * surroundingCheckRay.direction * bodySize;
            Vector3 checkPosition = start + offset;

            Vector3 checkDirection = surroundingCheckRay.direction;

            RaycastHit2D result = Physics2D.Raycast(checkPosition, checkDirection, bodySize, walkableLayerMask);

            return result.collider == null;
        }
    }

    private Vector2 CalculateCornerCheckPosition(SurroundingCheckRay surroundingCheckRay, bool option)
    {
        if (option)
        {
            Vector3 start = transform.position + (Vector3)surroundingCheckRay.direction * bodySize + (Vector3)surroundingCheckRay.direction * surroundingCheckRay.distance;
            Vector3 offset = Quaternion.Euler(0, 0, 90) * surroundingCheckRay.direction * bodySize;
            Vector3 corner = surroundingCheckRay.direction * 0.01f;
            return start + offset + corner;
        }
        else
        {
            Vector3 start = transform.position + (Vector3)surroundingCheckRay.direction * bodySize + (Vector3)surroundingCheckRay.direction * surroundingCheckRay.distance;
            Vector3 offset = Quaternion.Euler(0, 0, -90) * surroundingCheckRay.direction * bodySize;
            Vector3 corner = surroundingCheckRay.direction * 0.01f;
            return start + offset + corner;
        }
    }
    
    private Vector2 CalculateCornerCheckDirection(SurroundingCheckRay surroundingCheckRay, bool option)
    {
        if (option)
        {
            return Quaternion.Euler(0, 0, -90) * surroundingCheckRay.direction;
        }
        else
        {
            return Quaternion.Euler(0, 0, 90) * surroundingCheckRay.direction;
        }
    }

    private void CalculateCornerDistance(SurroundingCheckRay surroundingCheckRay, Vector2 cornerPosition)
    {
        Vector3 contactPoint = transform.position + (Vector3)surroundingCheckRay.direction * bodySize + (Vector3)surroundingCheckRay.direction * surroundingCheckRay.distance;
        distanceToCorner =  cornerPosition - (Vector2)contactPoint;
    }

    public LayerMask getWalkableLayerMask() { return walkableLayerMask; }
}
