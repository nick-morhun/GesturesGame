using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Events;

public class Figure : MonoBehaviour
{
    const float InvalidAngle = 1000;

    private Vector3 prevPointerPos;

    private float currentLineAngle;

    // TODO: compute this
    private float minVertexAngle = 45;

    private List<Line> uncheckedLines;

    private List<Line> candidateLines;

    [SerializeField]
    private List<Line> figureLines;

    public event UnityAction DrawSuccess = delegate { };

    public void Initialize()
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
    private void Start()
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
    }

    private bool WasLine(float drawnAngle)
    {
        bool wasLine = false;

        if (currentLineAngle != InvalidAngle && Mathf.Abs(currentLineAngle - drawnAngle) < MinAngle())
        {
            return false;
        }

        var newLines = new List<Line>();

        if (candidateLines.Count != 0)
        {
            // Other lines were complete
            foreach (var line in uncheckedLines)
            {
                if (line.Match(drawnAngle, MinAngle()))
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
            if (line.Match(drawnAngle, MinAngle()))
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

    private float MinAngle()
    {
        return minVertexAngle;
    }
}
