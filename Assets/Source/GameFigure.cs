﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Events;
using System;

public class GameFigure : Figure
{
    private float minVertexAngle = 45;

    private CornerDetector detector;

    private FigureMatcher forwardMatcher;

    private FigureMatcher backwardMatcher;

    private bool isLoaded;

    [SerializeField]
    [Range(2f, 60f)]
    private float sensitivity = 2;   // Fraction of min. figure's corner at which a line drawn matches figure's edge. For 90 deg. corner and sensitivity 2 it's 45 deg.

    public event UnityAction Loaded = delegate { };

    public event UnityAction<float> LineDetected = delegate { };

    public event UnityAction<float> LineAngleChanged = delegate { };

    public event UnityAction DrawSuccess = delegate { };

    /// <summary>
    /// Call this before every try.
    /// </summary>
    public void StartTry()
    {
        forwardMatcher.StartTry();
        backwardMatcher.StartTry();
        detector.StartTry();
    }

    public void Load(IFigureReader figureReader)
    {
        if (figureReader != null)
        {
            if (isLoaded)
            {
                Unload();
            }
            else
            {
                CleanUp();
            }

            int e = 0;

            foreach (var lineReader in figureReader.LineReaders)
            {
                var line = CreateLine(e);
                figureLines.Add(line);
                line.Load(lineReader);
                e++;
            }
        }
        else
        {
            Debug.LogWarning("Argument figureElement is null. Default figure can be used.");
        }

        ValidateLines();

        if (!IsValid)
        {
            Debug.LogError("Invalid figure");
            Loaded();
            return;
        }

        ValidateCornerAngles();

        minVertexAngle = ComputeCornerThreshold();

        forwardMatcher = new FigureMatcher(figureLines, minVertexAngle, false);
        forwardMatcher.Match += () => DrawSuccess();
        backwardMatcher = new FigureMatcher(figureLines, minVertexAngle, true);
        backwardMatcher.Match += () => DrawSuccess();
        detector = new CornerDetector(minVertexAngle, 4);
        detector.LineDetected += OnLineDetected;
        detector.LineAngleChanged += OnLineAngleChanged;

        isLoaded = true;
        Loaded();
    }

    public void Unload()
    {
        if (!isLoaded)
        {
            return;
        }

        CleanUp();
        forwardMatcher.Match -= () => DrawSuccess();
        backwardMatcher.Match -= () => DrawSuccess();
        detector.LineDetected -= OnLineDetected;
        detector.LineAngleChanged -= OnLineAngleChanged;
        ValidateLines();
        isLoaded = false;
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

    // Use this for initialization
    protected override void Start()
    {
        base.Start();
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
