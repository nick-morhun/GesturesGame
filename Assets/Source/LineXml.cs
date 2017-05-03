using System;
using System.Collections.Generic;
using System.IO;
using System.Xml.Linq;
using UnityEngine;
using UnityEngine.Events;

public sealed class LineXml : ILineReader, ILineWriter
{
    private XElement lineElement;

    public void Load(XElement lineElement)
    {
        this.lineElement = lineElement;
    }

    public XElement GetXml()
    {
        if (lineElement == null)
        {
            throw new InvalidOperationException();
        }

        return lineElement;
    }

    public string GetValue(string key)
    {
        if (key == null)
        {
            throw new ArgumentNullException("key");
        }

        return lineElement.Attribute(key).Value;
    }

    public void NewLine()
    {
        lineElement = new XElement("Line");
    }

    public void SetValue(string key, object value)
    {
        if (lineElement == null)
        {
            throw new InvalidOperationException();
        }

        lineElement.Add(new XAttribute(key, value));
    }
}
