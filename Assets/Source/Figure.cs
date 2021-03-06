﻿using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Events;
using System;

public class Figure : MonoBehaviour
{

    [SerializeField]
    [Range(.1f, 10f)]
    protected float minLineLength = 1;

    [SerializeField]
    [Range(5f, 45f)]
    protected float minCornerAllowed = 5f;   // In a figure

    [SerializeField]
    private Line linePrefab;

    [SerializeField]
    protected List<Line> figureLines;

    /// <summary>
    /// Is loaded figure valid?
    /// </summary>
    public bool IsValid { get; private set; }

    public event UnityAction ValidationFailed = delegate { };

    public void Save(IFigureWriter figureWriter)
    {
        if (figureWriter == null)
        {
            throw new ArgumentNullException("figureWriter");
        }

        figureWriter.Save(figureLines);
    }

    protected bool ValidateLines()
    {
        if (figureLines.Count < 3)
        {
            Debug.LogWarning("At least 3 lines required");
            IsValid = false;
            ValidationFailed();
            return false;
        }

        for (int i = 0; i < figureLines.Count; i++)
        {
            if (!figureLines[i].IsValid(minLineLength))
            {
                IsValid = false;
                ValidationFailed();
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
        figureLines[0].Previous = figureLines[figureLines.Count - 1];
        figureLines[figureLines.Count - 1].Next = figureLines[0];

        for (int i = 1; i < figureLines.Count; i++)
        {
            figureLines[i].Previous = figureLines[i - 1];
            figureLines[i - 1].Next = figureLines[i];
        }

        for (int i = 0; i < figureLines.Count; i++)
        {
            float angle = Line.Angle(figureLines[i], figureLines[i].Previous);

            if (angle < minCornerAllowed)
            {
                Debug.LogWarning("figureAngles[" + i + "] = " + angle + " is too small");
                IsValid = false;
                ValidationFailed();
                return false;
            }
        }

        IsValid = true;
        return true;
    }

    protected virtual void Start()
    {
        if (!linePrefab)
        {
            Debug.LogError("linePrefab was not set");
        }
    }

    protected Line CreateLine(int index)
    {
        var line = Instantiate(linePrefab);
        line.transform.SetParent(transform);
        line.name = "Line " + index;
        return line;
    }

    public void CleanUp()
    {
        foreach (var line in figureLines)
        {
            if (line)
            {
                Destroy(line.gameObject);
            }
        }

        figureLines.Clear();
    }
}
