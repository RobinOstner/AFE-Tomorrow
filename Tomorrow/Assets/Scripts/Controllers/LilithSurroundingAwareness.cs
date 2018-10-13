using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LilithSurroundingAwareness : MonoBehaviour {

    [SerializeField]
    private LayerMask walkableLayerMask;

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

    [SerializeField]
    private float attachableDistance;

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
        InitializeCorners();
        SetupSurroundingCheckRays();
	}
	
	void Update () {
        CheckSurroundingRays();
        CheckCorners();
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

    private void CheckCorners()
    {
        leftBottomCorner = CheckCorner(canAttachLeft, true, leftCheckRay);
        leftTopCorner = CheckCorner(canAttachLeft, false, leftCheckRay);
        rightTopCorner = CheckCorner(canAttachRight, true, rightCheckRay);
        rightBottomCorner = CheckCorner(canAttachRight, false, rightCheckRay);
        topLeftCorner = CheckCorner(canAttachUp, true, upCheckRay);
        topRightCorner = CheckCorner(canAttachUp, false, upCheckRay);
        bottomRightCorner = CheckCorner(canAttachDown, true, downCheckRay);
        bottomLeftCorner = CheckCorner(canAttachDown, false, downCheckRay);
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
                    return checkPosition + checkDirection * result.distance + surroundingCheckRay.direction * -0.01f;
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

    public LayerMask getWalkableLayerMask() { return walkableLayerMask; }
}
