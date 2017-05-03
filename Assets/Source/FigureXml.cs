using UnityEngine;
using System.Collections.Generic;
using System.Xml.Linq;
using UnityEngine.Events;
using System;

public sealed class FigureXml : IFigureReader, IFigureWriter
{
    private XElement figureElement;
    private List<ILineReader> lineReaders = new List<ILineReader>();

    public void Load(XElement figureElement, GameFigure targetFigure)
    {
        if (figureElement != null)
        {
            foreach (var lineElement in figureElement.Elements())
            {
                var lineReader = new LineXml();
                lineReader.Load(lineElement);
                lineReaders.Add(lineReader);
            }

            targetFigure.Load(this);
        }
        else
        {
            Debug.LogWarning("Argument figureElement is null. Default figure can be used.");
        }
    }

    public XElement GetXml(Figure sourceFigure)
    {
        sourceFigure.Save(this);

        if (figureElement == null)
        {
            throw new InvalidOperationException();
        }

        return figureElement;
    }

    public IEnumerable<ILineReader> LineReaders
    {
        get { return lineReaders; }
    }

    public void Save(IEnumerable<Line> figureLines)
    {
        figureElement = new XElement("Figure");
            
        foreach (var line in figureLines)
        {
            var lineWriter = new LineXml();
            line.Save(lineWriter);
            figureElement.Add(lineWriter.GetXml());
        }
    }
}
