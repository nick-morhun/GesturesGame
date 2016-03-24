using UnityEngine;
using UnityEngine.Events;

public class CornerDetector
{
    const float InvalidAngle = 1000;

    const float MinCornerAngle = 3;

    private Vector3 prevPointerPos;

    private float currentLineAngle;

    private int currentLinePointerMoves;

    private bool currentLineAccepted = false;

    private float minVertexAngle;

    private int minPointerMovesPerLine;

    private float CurrentLineAngle
    {
        get { return currentLineAngle; }
        set
        {
            currentLineAngle = value;
            //Debug.LogWarning("currentLineAngle = " + currentLineAngle);
            LineAngleChanged(currentLineAngle);
        }
    }

    public event UnityAction<float> LineDetected = delegate { };

    public event UnityAction<float> LineAngleChanged = delegate { };

    public CornerDetector(float minVertexAngle, int minPointerMovesPerLine)
    {
        this.minVertexAngle = minVertexAngle;
        this.minPointerMovesPerLine = minPointerMovesPerLine;
    }

    /// <summary>
    /// Call this before every try.
    /// </summary>
    public void StartTry()
    {
        currentLineAngle = InvalidAngle;
    }

    public void OnInputTouchStarted(Vector3 pointerPos)
    {
        prevPointerPos = pointerPos;
        currentLinePointerMoves = 0;
        currentLineAccepted = false;
    }

    public void OnInputPointerMoved(Vector3 pointerPos)
    {
        Vector3 drawnline = pointerPos - prevPointerPos;
        float angle = Mathf.Rad2Deg * Mathf.Atan2(drawnline.y, drawnline.x);

        if (!WasCorner(currentLineAngle, angle) && CurrentLineAngle != InvalidAngle)
        {
            if (currentLinePointerMoves % minPointerMovesPerLine == 0)
            {
                prevPointerPos = pointerPos;
            }

            if (!currentLineAccepted && minPointerMovesPerLine <= currentLinePointerMoves)
            {
                // Accept line
                Debug.Log("line: currentLineAngle = " + CurrentLineAngle);
                currentLineAccepted = true;
                LineDetected(CurrentLineAngle);
            }

            CurrentLineAngle = Utils.InterpolateAngles(CurrentLineAngle, angle, currentLinePointerMoves);
            currentLinePointerMoves++;
            return;
        }

        if (CurrentLineAngle != InvalidAngle && minPointerMovesPerLine <= currentLinePointerMoves)
        {
            // Was corner
            //Debug.Log("corner: currentLineAngle = " + CurrentLineAngle);
            currentLinePointerMoves = 0;
            //Corner(currentLineAngle);
            currentLineAccepted = false;
        }

        CurrentLineAngle = angle;
        prevPointerPos = pointerPos;
    }

    /// <summary>
    /// Returns true and checks for matching figures if user made a corner.
    /// </summary>
    /// <param name="drawnAngle">Angle between line's direction and x axis, degrees (-180; 180).</param>
    private bool WasCorner(float previousAngle, float drawnAngle)
    {
        if (previousAngle == InvalidAngle || Utils.Angle(previousAngle, drawnAngle) < minVertexAngle)
        {
            // No corner
            return false;
        }

        return true;
    }
}
