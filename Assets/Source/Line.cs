using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Line : MonoBehaviour
{
    /// <summary>
    /// Available after Start()
    /// </summary>
    float angle;

    [SerializeField]
    private Transform start;

    [SerializeField]
    private Transform end;

    public Line Previous = null;

    public Vector3 StartPoint { get { return start.position; } }

    public Vector3 EndPoint { get { return end.position; } }

    public bool Match(float angle, float minMatchAngle)
    {
        return Mathf.Abs(angle - this.angle) < minMatchAngle;
    }

    public static float Angle(Line l1, Line l2)
    {
        // Debug.Log(l1.angle - l2.angle);
        return l1.angle - l2.angle;
    }

    // Use this for initialization
    private void Start()
    {
        angle = transform.localRotation.eulerAngles.z;
        angle = angle > 180 ? angle - 360 : angle;
        Debug.Log("Line at angle " + angle);
    }
}
