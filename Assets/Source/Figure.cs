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

    /// <summary>
    /// Each line is the last line of candidate figure # i
    /// </summary>
    private List<Line> candidateLines;

    /// <summary>
    /// Each number is the line count of candidate figure # i
    /// </summary>
    private List<int> candidatePathsLengths;

    private List<int> replacedIndices;

    private bool isFirstLineDrawn;

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
        candidateLines = new List<Line>(figureLines.Count);
        candidatePathsLengths = new List<int>(figureLines.Count);

        currentLineAngle = InvalidAngle;
        isFirstLineDrawn = false;
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

            for (int i = 1; i < figureLines.Count; i++)
            {
                figureLines[i].Previous = figureLines[i - 1];
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

        replacedIndices = new List<int>();
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
        Ready();
    }

    private bool WasLine(float drawnAngle)
    {
        bool wasLine = false;

        if (currentLineAngle != InvalidAngle && Mathf.Abs(currentLineAngle - drawnAngle) < minVertexAngle)
        {
            // No corner
            return false;
        }

        replacedIndices.Clear();

        for (int i = 0; i < figureLines.Count; i++)
        {
            Line line = figureLines[i];

            if (line.Match(drawnAngle, minVertexAngle))
            {
                if (candidateLines.Count != 0)
                {
                    // Other paths were started

                    int prevLineIdx = candidateLines.IndexOf(line.Previous);

                    if (prevLineIdx != -1)
                    {
                        // Other lines were on this path / candidate figure

                        candidateLines[prevLineIdx] = line;
                        candidatePathsLengths[prevLineIdx]++;
                        replacedIndices.Add(prevLineIdx);
                        wasLine = true;
                        continue;
                    }
                }

                if (!isFirstLineDrawn)
                {
                    // Start a new path / candidate figure
                    replacedIndices.Add(candidateLines.Count);
                    candidateLines.Add(line);
                    candidatePathsLengths.Add(1);
                    wasLine = true;
                }
            }
        }

        // Clean up paths
        int removed = 0;

        for (int i = 0; i < candidateLines.Count; i++)
        {
            if (candidatePathsLengths[i] == figureLines.Count)
            {
                DrawSuccess();
                break;
            }

            if (!replacedIndices.Contains(i + removed))
            {
                candidateLines.RemoveAt(i);
                candidatePathsLengths.RemoveAt(i);
                i--;
                removed++;
            }
        }

        isFirstLineDrawn = true;
        return wasLine;
    }
}
