using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Events;
using System.Xml.Linq;

public class Figure : MonoBehaviour
{
    private float minVertexAngle = 45;

    private CornerDetector detector;

    private FigureMatcher forwardMatcher;

    private FigureMatcher backwardMatcher;

    [SerializeField]
    [Range(2f, 60f)]
    private float sensitivity = 2;   // Fraction of min. figure's corner at which a line drawn matches figure's edge. For 90 deg. corner and sensitivity 2 it's 45 deg.

    [SerializeField]
    [Range(.1f, 10f)]
    private float minLineLength = 1;

    [SerializeField]
    [Range(5f, 45f)]
    private float minCornerAllowed = 5f;   // In a figure

    [SerializeField]
    private Line linePrefab;

    [SerializeField]
    private List<Line> figureLines;

    public event UnityAction Ready = delegate { };

    public event UnityAction<float> LineDetected = delegate { };

    public event UnityAction<float> LineAngleChanged = delegate { };

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
        detector.StartTry();
    }

    public void Load(XElement figureElement)
    {
        if (figureElement != null)
        {
            figureLines.Clear();
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
        else
        {
            Debug.LogWarning("Argument figureElement is null. Default figure can be used.");
        }

        IsValid = ValidateLines();

        if (IsValid)
        {
            figureLines[0].Previous = figureLines[figureLines.Count - 1];
            figureLines[figureLines.Count - 1].Next = figureLines[0];

            for (int i = 1; i < figureLines.Count; i++)
            {
                figureLines[i].Previous = figureLines[i - 1];
                figureLines[i - 1].Next = figureLines[i];
            }

            IsValid = ValidateCornerAngles();

            minVertexAngle = ComputeCornerThreshold();

            forwardMatcher = new FigureMatcher(figureLines, minVertexAngle, false);
            forwardMatcher.Match += () => DrawSuccess();
            backwardMatcher = new FigureMatcher(figureLines, minVertexAngle, true);
            backwardMatcher.Match += () => DrawSuccess();
            detector = new CornerDetector(minVertexAngle, 4);
            detector.LineDetected += OnLineDetected;
            detector.LineAngleChanged += OnLineAngleChanged;
            Ready();
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
        IsValid = ValidateLines() && ValidateCornerAngles();

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
                Object.Destroy(line.gameObject);
            }
        }

        figureLines.Clear();
        forwardMatcher.Match -= () => DrawSuccess();
        backwardMatcher.Match -= () => DrawSuccess();
        detector.LineDetected -= OnLineDetected;
        detector.LineAngleChanged -= OnLineAngleChanged;
        IsValid = false;
    }

    internal void OnInputTouchStarted(Vector3 pos)
    {
        StartTry();
        if (detector != null)
        {
            detector.OnInputTouchStarted(pos);
        }
    }

    internal void OnInputPointerMoved(Vector3 pos)
    {
        if (detector != null)
        {
            detector.OnInputPointerMoved(pos);
        }
    }

    private bool ValidateLines()
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

    /// <summary>
    /// Requires "Previous" fields of lines to be set.
    /// </summary>
    private bool ValidateCornerAngles()
    {
        for (int i = 0; i < figureLines.Count; i++)
        {
            float angle = Line.Angle(figureLines[i], figureLines[i].Previous);

            if (angle < minCornerAllowed)
            {
                Debug.LogWarning("figureAngles[" + i + "] = " + angle + " is too small");
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

    private float ComputeCornerThreshold()
    {
        float[] figureAngles = new float[figureLines.Count];

        for (int i = 0; i < figureLines.Count; i++)
        {
            figureAngles[i] = Line.Angle(figureLines[i], figureLines[i].Previous);
            Debug.Log("figureAngles[" + i + "] = " + figureAngles[i]);
        }

        float threshold = Mathf.Min(figureAngles) / sensitivity;
        return threshold;
    }

    /// <summary>
    /// Returns true and checks for matching figures if user draws a line.
    /// </summary>
    /// <param name="angle">Angle between line's direction and x axis, degrees (-180; 180).</param>
    private void OnLineDetected(float angle)
    {
        LineDetected(angle);
        forwardMatcher.MatchLine(angle);
        backwardMatcher.MatchLine(angle);
    }

    private void OnLineAngleChanged(float angle)
    {
        LineAngleChanged(angle);
    }
}
