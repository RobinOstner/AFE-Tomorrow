using UnityEngine;
using System.Collections;

[System.Serializable]
public struct JumpCheckRay
{
    public Ray2D ray;
    public Vector2 direction;
    public Vector2 offset;
    public float distance;
    public RaycastHit2D result;

    public float maxJumpDistance;
    public float minJumpDistance;
    public bool canJump
    {
        get
        {
            return result.distance <= maxJumpDistance && result.distance >= minJumpDistance;
        }
    }

    public LayerMask layer;
    public Vector2 normal;
    public Color color;
    public Color gizmosColor
    {
        get
        {
            if (result.collider == null)
            {
                return color;
            }
            else
            {
                return Color.green;
            }
        }
    }

    public void CheckRay(Transform transform, LayerMask layerMask)
    {
        distance = maxJumpDistance;

        ray = new Ray2D(transform.position, direction * maxJumpDistance);

        result = Physics2D.Raycast(transform.position, ray.direction, maxJumpDistance, layerMask);
        if (result.collider != null)
        {
            distance = result.distance;
            layer = LayerMask.GetMask(LayerMask.LayerToName(result.collider.gameObject.layer));
            normal = result.normal;
        }
        else
        {
            layer = 0;
            normal = Vector2.zero;
        }
    }
}
