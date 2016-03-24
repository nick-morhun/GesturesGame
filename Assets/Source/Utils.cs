using System.Xml.Linq;
using UnityEngine;

public static class Utils
{

    /// <summary>
    /// Returns an angle between line vectors.
    /// </summary>
    /// <param name="l1">The line starting the angle.</param>
    /// <param name="l2">The line finishing the angle.</param>
    /// <returns>Angle in (0; 180) range.</returns>
    public static float Angle(float angle1, float angle2)
    {
        float angle = angle1 - angle2; // (-360;360)
        float angle3 = angle < -180 ? angle + 360 : (angle > 180 ? 360 - angle : angle);  // (-180; 180)
        return Mathf.Abs(angle3);  // (0;180)
    }

    /// <summary>
    /// Interpolate angles based on currentLinePointerMoves.
    /// </summary>
    /// <param name="previousAngle">Stable angle, in (-180;180) range</param>
    /// <param name="addedAngle">New angle, in (-180;180) range</param>
    /// <returns></returns>
    public static float InterpolateAngles(float previousAngle, float addedAngle, int numSteps)
    {
        if (numSteps < 0)
        {
            throw new System.ArgumentOutOfRangeException();
        }

        float A = Mathf.Deg2Rad * previousAngle, B = Mathf.Deg2Rad * addedAngle;
        float w = 1 / (float)(numSteps + 1);

        // http://stackoverflow.com/questions/2708476/rotation-interpolation
        float CS = (1 - w) * Mathf.Cos(A) + w * Mathf.Cos(B);
        float SN = (1 - w) * Mathf.Sin(A) + w * Mathf.Sin(B);
        float result = Mathf.Atan2(SN, CS) * Mathf.Rad2Deg;
        //Debug.Log("w = " + w + "previousAngle = " + previousAngle + " addedAngle = " + addedAngle + " result = " + result);
        return result;
    }
}
