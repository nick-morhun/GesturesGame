using UnityEngine;
using UnityEngine.Events;

public class EditorFigure : Figure
{
    private int lineIndex = 0;

    private Line currentLine;

    private Line previousLine;

    private Line lastLine;

    private Line firstLine;

    private bool isLineComplete = false;

    [SerializeField]
    private Transform debugObject;

    public event UnityAction FigureStarted = delegate { };

    public void StartNewFigure()
    {
        base.CleanUp();
        GameObject.Destroy(lastLine.gameObject);
        currentLine = null;
        isLineComplete = false;
        lineIndex = 0;
    }

    public void AddLine(Vector3 start)
    {
        start = new Vector3(start.x, start.y, 0);

        if (!currentLine)
        {
            NewLine(start);     // The 1st line
            firstLine = currentLine;
            lastLine = CreateLine(-1);
            AdjustLastLine();
        }
        else
        {
            if (isLineComplete)
            {
                NewLine(currentLine.EndPoint);  // The second etc. lines
            }

            DragLineEnd(start);
        }
    }

    public void DragLineEnd(Vector3 end)
    {
        end = new Vector3(end.x, end.y, 0);

        if (!currentLine || !lastLine)
        {
            return;
        }

        Vector3 start = currentLine.StartPoint;
        float angle = Utils.Angle(start, end);

        if (previousLine)
        {
            float angleWithPrevious = Utils.AnglesDiff(angle, previousLine.AngleFromX);

            if (angleWithPrevious < minCornerAllowed)
            {
                angle = previousLine.AngleFromX + minCornerAllowed + 1;
                angleWithPrevious = Utils.AnglesDiff(angle, previousLine.AngleFromX);
                //Debug.Log("angleWithPrevious = " + angleWithPrevious);
            }
        }

        Vector3 currentLineVector = end - start;

        if (!CheckLastLine(currentLineVector, angle))
        {
            Debug.LogWarning("CheckLastLine failed");
            return;
        }

        currentLine.SetAngle(angle);
        currentLine.SetLength(currentLineVector.magnitude, minLineLength);
        AdjustLastLine();
    }

    private bool CheckLastLine(Vector3 currentLineVector, float angle)
    {
        float lengthCurrent = currentLineVector.magnitude;

        if (lengthCurrent < minLineLength)
        {
            lengthCurrent = minLineLength + .1f;
        }

        // Project current line's end
        Vector3 newEndpointCurrent = currentLine.StartPoint + new Vector3(Mathf.Cos(Mathf.Deg2Rad * angle), Mathf.Sin(Mathf.Deg2Rad * angle), 0) * lengthCurrent;
        debugObject.transform.position = newEndpointCurrent;

        float angleLast = Utils.Angle(newEndpointCurrent, firstLine.StartPoint);
        Vector3 lastLineVector = firstLine.StartPoint - newEndpointCurrent;

        //Debug.Log("CheckLastLine: length = " + lastLineVector.magnitude + " angle = " + angleLast + " firstAngle = " + firstLine.AngleFromX);

        if (Utils.AnglesDiff(angleLast, firstLine.AngleFromX) < minCornerAllowed)
        {
            return false;
        }

        if (lastLineVector.magnitude <= minLineLength)
        {
            return false;
        }
        return true;
    }

    private void AdjustLastLine()
    {
        lastLine.transform.position = currentLine.EndPoint; // is now lastLine.StartPoint
        float angle0 = Utils.Angle(lastLine.StartPoint, firstLine.StartPoint);
        lastLine.SetAngle(angle0);
        Vector3 lastLineVector = firstLine.StartPoint - lastLine.StartPoint;
        lastLine.SetLength(lastLineVector.magnitude, minLineLength);

        if (lastLineVector.magnitude <= minLineLength)
        {
            Debug.LogError("AdjustLastLine: Safety failed. lastLineVector is too short ");
        }

        if (Utils.AnglesDiff(angle0, firstLine.AngleFromX) < minCornerAllowed)
        {
            Debug.LogError("AdjustLastLine: Safety failed. angle is too small: " + angle0);
        }
    }

    public void CompleteLine()
    {
        if (!currentLine)
        {
            return;
        }

        isLineComplete = true;
        float angleWithPrevious = previousLine ? Line.Angle(currentLine, previousLine) : 1000;
        Debug.Log("Line complete. angle = " + currentLine.AngleFromX
            + " angleWithPrevious = " + angleWithPrevious);

        if (figureLines.Count > 1)
        {
            FigureStarted();
        }
    }

    public void CompleteFigure()
    {
        figureLines.Add(lastLine);
        Debug.Log("Figure complete");
    }

    // Use this for initialization
    protected override void Start()
    {
        base.Start();
    }

    private void NewLine(Vector3 pos)
    {
        previousLine = currentLine;
        currentLine = CreateLine(lineIndex++);
        figureLines.Add(currentLine);
        currentLine.transform.position = pos;
        currentLine.SetLength(minLineLength, minLineLength);
        Debug.Log("Added a line '" + currentLine.name + "' at " + pos);
    }
}
