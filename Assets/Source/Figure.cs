using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Events;
using System.Xml.Linq;

public class Figure : MonoBehaviour
{

    [SerializeField]
    [Range(.1f, 10f)]
    private float minLineLength = 1;

    [SerializeField]
    [Range(5f, 45f)]
    private float minCornerAllowed = 5f;   // In a figure

    [SerializeField]
    private Line linePrefab;

    [SerializeField]
    protected List<Line> figureLines;

    /// <summary>
    /// Is loaded figure valid?
    /// </summary>
    public bool IsValid { get; private set; }

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

    protected bool ValidateLines()
    {
        if (figureLines.Count < 3)
        {
            Debug.LogError("At least 3 lines required");
            IsValid = false;
            return false;
        }

        for (int i = 0; i < figureLines.Count; i++)
        {
            if (!figureLines[i].IsValid(minLineLength))
            {
                IsValid = false;
                return false;
            }
        }

        IsValid = true;
        return true;
    }

    /// <summary>
    /// Requires "Previous" fields of lines to be set.
    /// </summary>
    protected bool ValidateCornerAngles()
    {
        for (int i = 0; i < figureLines.Count; i++)
        {
            float angle = Line.Angle(figureLines[i], figureLines[i].Previous);

            if (angle < minCornerAllowed)
            {
                Debug.LogWarning("figureAngles[" + i + "] = " + angle + " is too small");
                IsValid = false;
                return false;
            }
        }

        IsValid = true;
        return true;
    }

    // Use this for initialization
    protected virtual void Start()
    {
        if (!linePrefab)
        {
            Debug.LogError("linePrefab was not set");
        }
    }

    protected Line CreateLine(int index)
    {
        var line = Object.Instantiate(linePrefab);
        line.transform.SetParent(transform);
        line.name = "Line " + index;
        figureLines.Add(line);
        return line;
    }

    protected void CleanUp()
    {
        foreach (var line in figureLines)
        {
            if (line)
            {
                Object.Destroy(line.gameObject);
            }
        }

        figureLines.Clear();
    }
}
