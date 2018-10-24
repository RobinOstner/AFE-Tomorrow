using UnityEngine;
using System.Collections;

public class LeftRightTest
{
    /// <summary>
    /// Checks if the target is left or right of the forward direction. Returns -1 if left and 1 if right.
    /// </summary>
    public static float CheckLeftRight(Vector3 fwd, Vector3 up, Vector3 targetDir)
    {
        Vector3 perp = Vector3.Cross(fwd, targetDir);
        float dir = Vector3.Dot(perp, up);

        if (dir > 0f)
        {
            return 1f;
        }
        else if (dir < 0f)
        {
            return -1f;
        }
        else
        {
            return 0f;
        }
    }

}
