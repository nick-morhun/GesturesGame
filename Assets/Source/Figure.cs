using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Events;

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
    private List<Line> figureLines;


    public event UnityAction Ready = delegate { };

    public event UnityAction DrawSuccess = delegate { };

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

    // Use this for initialization
    public void Init()
    {
        if (figureLines.Count < 3)
        {
            Debug.LogError("At least 3 lines required");
            return;
        }

        figureLines[0].Previous = figureLines[figureLines.Count - 1];

        for (int i = 1; i < figureLines.Count; i++)
        {
            figureLines[i].Previous = figureLines[i - 1];
        }

        StartCoroutine(ComputeCornerThreshold());
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
