using System.Xml.Linq;
using UnityEngine;

public class Line : MonoBehaviour
{
    /// <summary>
    /// Angle btween this line's forward vector and X axis. Available after Load()
    /// </summary>
    private float angle;

    [SerializeField]
    [Range(.05f, 1f)]
    private float width;

    [SerializeField]
    private Transform start;

    [SerializeField]
    private Transform end;

    public Line Previous = null;

    public Line Next = null;

    public void Load(XElement lineElement)
    {
        float centerX = float.Parse(lineElement.Attribute("CenterX").Value);
        float centerY = float.Parse(lineElement.Attribute("CenterY").Value);
        float length = float.Parse(lineElement.Attribute("Length").Value);
        float angle = float.Parse(lineElement.Attribute("Angle").Value);

        this.angle = angle;
        transform.position = new Vector3(centerX, centerY);
        transform.localScale = new Vector3(length, width, 1f);
        transform.localRotation = Quaternion.Euler(0, 0, angle);
        Debug.Log("Line loaded: " + name);
    }

    public XElement Save()
    {
        XElement lineElement = new XElement("Line",
            new XAttribute("CenterX", transform.position.x),
            new XAttribute("CenterY", transform.position.y),
            new XAttribute("Length", transform.localScale.x),
            new XAttribute("Angle", angle));
        return lineElement;
    }

    public bool Match(float angle, float minMatchAngle)
    {
        float diff = Mathf.Abs(angle - this.angle);

        if (diff > 360 - minMatchAngle)
        {
            diff = 360 - diff;
        }

        return diff < minMatchAngle;
    }

    /// <summary>
    /// Returns an angle between line vectors.
    /// </summary>
    /// <param name="l1">The line starting the angle.</param>
    /// <param name="l2">The line finishing the angle.</param>
    /// <returns>Angle in (0; 180) range.</returns>
    public static float Angle(Line l1, Line l2)
    {
        float angle = l1.angle - l2.angle; // (-360;360)
        float angle2 = angle < -180 ? angle + 360 : (angle > 180 ? 360 - angle : angle);  // (-180; 180)
        return Mathf.Abs(angle2);  // (0;180)
    }

    public bool IsValid(float minLength)
    {
        return (end.position - start.position).magnitude >= minLength;
    }

    // Use this for initialization
    private void Start()
    {
        angle = transform.localRotation.eulerAngles.z;
        angle = angle > 180 ? angle - 360 : angle;
        //Debug.Log("Line at angle " + angle);
    }
}
