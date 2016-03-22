using System.Collections.Generic;
using UnityEngine.Events;

public class FigureMatcher
{

    private readonly float minVertexAngle = 45;

    private readonly List<Line> figureLines;

    private readonly bool isBackward;

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

    public event UnityAction Match = delegate { };

    public FigureMatcher(List<Line> figureLines, float minVertexAngle, bool isBackward)
    {
        this.figureLines = new List<Line>(figureLines);
        this.minVertexAngle = minVertexAngle;
        this.isBackward = isBackward;
    }

    /// <summary>
    /// Call this before every try.
    /// </summary>
    public void StartTry()
    {
        candidateLines = new List<Line>(figureLines.Count);
        candidatePathsLengths = new List<int>(figureLines.Count);
        replacedIndices = new List<int>();
        isFirstLineDrawn = false;
    }

    public bool MatchLine(float drawnAngle)
    {
        bool wasMatch = false;
        replacedIndices.Clear();

        if (isBackward)
        {
            drawnAngle = (drawnAngle < 0 ? drawnAngle + 180 : drawnAngle - 180);
        }

        for (int i = 0; i < figureLines.Count; i++)
        {
            Line line = figureLines[i];

            if (line.Match(drawnAngle, minVertexAngle))
            {
                if (candidateLines.Count != 0)
                {
                    // Other paths were started

                    Line previous = (this.isBackward ? line.Next : line.Previous);

                    int prevLineIdx = candidateLines.IndexOf(previous);

                    if (prevLineIdx != -1)
                    {
                        // Other lines were on this path / candidate figure

                        candidateLines[prevLineIdx] = line;
                        candidatePathsLengths[prevLineIdx]++;
                        replacedIndices.Add(prevLineIdx);
                        wasMatch = true;
                        continue;
                    }
                }

                if (!isFirstLineDrawn)
                {
                    // Start a new path / candidate figure
                    replacedIndices.Add(candidateLines.Count);
                    candidateLines.Add(line);
                    candidatePathsLengths.Add(1);
                    wasMatch = true;
                }
            }
        }

        // Clean up paths
        int removed = 0;

        for (int i = 0; i < candidateLines.Count; i++)
        {
            if (candidatePathsLengths[i] == figureLines.Count)
            {
                Match();
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
        return wasMatch;
    }
}
