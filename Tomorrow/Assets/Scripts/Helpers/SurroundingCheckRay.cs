using UnityEngine;

[System.Serializable]
public struct SurroundingCheckRay
{
    public Ray2D ray;
    public Vector2 direction;
    public Vector2 offset;
    public float distance;
    public RaycastHit2D result;
    public LayerMask layer;
    public Vector2 normal;
    public Color color;
    public Color gizmosColor
    {
        get
        {
            if(result.collider == null)
            {
                return color;
            }
            else
            {
                return Color.green;
            }
        }
    }

    public void CheckRay(Transform transform, float checkDistance, LayerMask layerMask)
    {
        distance = checkDistance;

        ray = new Ray2D(transform.position + (Vector3)offset, direction * checkDistance);

        result = Physics2D.Raycast(transform.position + (Vector3)offset, ray.direction, checkDistance, layerMask);
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