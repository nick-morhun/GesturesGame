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

    private List<Line> uncheckedLines;

    private List<Line> candidateLines;

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

    private string path
    {
        get { return Application.persistentDataPath + "/figures.xml"; }
    }

    public event UnityAction Ready = delegate { };

    public event UnityAction DrawSuccess = delegate { };

    public bool IsValid { get; private set; }

    /// <summary>
    /// Call this before every try.
    /// </summary>
    public void StartTry()
    {
        candidateLines = new List<Line>(figureLines.Count);
        uncheckedLines = new List<Line>(figureLines);

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

    public void Save()
    {
        IsValid = Validate();

        if (!IsValid)
        {
            Debug.LogError("Invalid figure");
            return;
        }

        var figuresXML = new XDocument(new XElement("Figures"));

        var figureElement = new XElement("Figure");

        for (int i = 0; i < figureLines.Count; i++)
        {
            figureElement.Add(figureLines[i].Save());
        }

        figuresXML.Root.Add(figureElement);
        figuresXML.Save(path);
        Debug.Log("Saved to " + path);
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
            figureAngles[i] = ang > 360 ? ang - 360 : ang;
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
            return false;
        }

        var newLines = new List<Line>();

        if (candidateLines.Count != 0)
        {
            // Other lines were complete
            foreach (var line in uncheckedLines)
            {
                if (line.Match(drawnAngle, minVertexAngle))
                {
                    int prevLineIdx = candidateLines.IndexOf(line.Previous);
                    if (prevLineIdx != -1)
                    {
                        // Other lines were complete on this path
                        candidateLines[prevLineIdx] = line;
                    }
                    else
                    {
                        //candidateLines.Add(line);
                        newLines.Add(line);
                    }

                    wasLine = true;
                }
                else
                {
                    newLines.Add(line);
                }
            }

            uncheckedLines = newLines;

            if (uncheckedLines.Count == 0)
            {
                DrawSuccess();
            }

            return wasLine;
        }

        foreach (var line in uncheckedLines)
        {
            if (line.Match(drawnAngle, minVertexAngle))
            {
                candidateLines.Add(line);
                wasLine = true;
            }
            else
            {
                newLines.Add(line);
            }
        }

        uncheckedLines = newLines;

        if (uncheckedLines.Count == 0)
        {
            DrawSuccess();
        }

        return wasLine;
    }
}
