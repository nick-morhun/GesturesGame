using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Events;
using System.Xml.Linq;

public class Figure : MonoBehaviour
{
    const float InvalidAngle = 1000;

    private Vector3 prevPointerPos;

    private float currentLineAngle;

    private float minVertexAngle = 45;

    private FigureMatcher forwardMatcher;

    private FigureMatcher backwardMatcher;

    [SerializeField]
    [Range(1f, 100f)]
    private float sensitivity = 1;

    [SerializeField]
    [Range(.1f, 10f)]
    private int minLineLength = 1;

    [SerializeField]
    private Line linePrefab;

    [SerializeField]
    private List<Line> figureLines;

    public event UnityAction Ready = delegate { };

    public event UnityAction DrawSuccess = delegate { };

    /// <summary>
    /// Is loaded figure valid?
    /// </summary>
    public bool IsValid { get; private set; }

    /// <summary>
    /// Call this before every try.
    /// </summary>
    public void StartTry()
    {
        forwardMatcher.StartTry();
        backwardMatcher.StartTry();
        currentLineAngle = InvalidAngle;
    }

    public void OnInputTouchStarted(Vector3 pointerPos)
    {
        prevPointerPos = pointerPos;
    }

    public void OnInputPointerMoved(Vector3 pointerPos)
    {
        Vector3 drawnline = pointerPos - prevPointerPos;
        float angle = Mathf.Rad2Deg * Mathf.Atan2(drawnline.y, drawnline.x);

        if (WasLine(angle) || currentLineAngle == InvalidAngle)
        {
            currentLineAngle = angle;     // Complete current line
        }
        else
        {
            currentLineAngle = (currentLineAngle + angle) / 2;  // Interpolate
        }

        prevPointerPos = pointerPos;
    }

    public void Load(XElement figureElement)
    {
        if (figureElement != null)
        {
            int e = 0;

            foreach (var lineElement in figureElement.Elements())
            {
                var line = Object.Instantiate(linePrefab);
                line.transform.SetParent(transform);
                line.name = "Line " + e++;
                line.Load(lineElement);
                figureLines.Add(line);
            }
        }

        IsValid = Validate();

        if (IsValid)
        {
            figureLines[0].Previous = figureLines[figureLines.Count - 1];
            figureLines[figureLines.Count - 1].Next = figureLines[0];

            for (int i = 1; i < figureLines.Count; i++)
            {
                figureLines[i].Previous = figureLines[i - 1];
                figureLines[i - 1].Next = figureLines[i];
            }

            StartCoroutine(ComputeCornerThreshold());
        }
        else
        {
            Debug.LogError("Invalid figure");
            Ready();
        }
    }

    public XElement Save()
    {
        XElement figureElement = new XElement("Figure");
        IsValid = Validate();

        if (!IsValid)
        {
            Debug.LogError("Invalid figure");
            return null;
        }

        for (int i = 0; i < figureLines.Count; i++)
        {
            figureElement.Add(figureLines[i].Save());
        }

        return figureElement;
    }

    public void Unload()
    {
        foreach (var line in figureLines)
        {
            if (line)
            {
                Object.Destroy(line);
            }
        }

        figureLines.Clear();
        IsValid = false;
    }

    private bool Validate()
    {
        if (figureLines.Count < 3)
        {
            Debug.LogError("At least 3 lines required");
            return false;
        }

        for (int i = 0; i < figureLines.Count; i++)
        {
            if (!figureLines[i].IsValid(minLineLength))
            {
                return false;
            }
        }

        return true;
    }

    // Use this for initialization
    private void Start()
    {
        if (!linePrefab)
        {
            Debug.LogError("linePrefab was not set");
        }
    }

    private IEnumerator ComputeCornerThreshold()
    {
        yield return null;
        float[] figureAngles = new float[figureLines.Count];

        for (int i = 0; i < figureLines.Count; i++)
        {
            float ang = Line.Angle(figureLines[i], figureLines[i].Previous);
            ang += 180;
            figureAngles[i] = Mathf.Abs(ang >= 360 ? ang - 360 : ang);
            Debug.Log("figureAngles[" + i + "] = " + figureAngles[i]);
        }

        minVertexAngle = Mathf.Min(figureAngles) / (2f * sensitivity);
        Debug.Log("minVertexAngle = " + minVertexAngle);

        forwardMatcher = new FigureMatcher(figureLines, minVertexAngle, false);
        forwardMatcher.Match += () => DrawSuccess();
        backwardMatcher = new FigureMatcher(figureLines, minVertexAngle, true);
        backwardMatcher.Match += () => DrawSuccess();
        Ready();
    }

    /// <summary>
    /// Returns true and checks for matching figures if user made a corner.
    /// </summary>
    /// <param name="drawnAngle">Angle between line's direction and x axis, degrees (-180; 180).</param>
    private bool WasLine(float drawnAngle)
    {
        if (currentLineAngle != InvalidAngle && Mathf.Abs(currentLineAngle - drawnAngle) < minVertexAngle)
        {
            // No corner
            return false;
        }

        forwardMatcher.MatchLine(drawnAngle);
        backwardMatcher.MatchLine(drawnAngle);
        return true;
    }
}
