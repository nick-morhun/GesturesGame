using System.Collections.Generic;
using System.Xml.Linq;
using UnityEngine;

public class FiguresXml
{
    public List<XElement> figureElements = new List<XElement>();

    private string path
    {
        get { return Application.persistentDataPath + "/figures.xml"; }
    }

    public void Load()
    {
        XDocument figuresXML = null;

        try
        {
            figuresXML = XDocument.Load(path);
        }
        catch (System.IO.IOException ex)
        {
            Debug.LogError(ex.Message);
            return;
        }

        figureElements = new List<XElement>(figuresXML.Root.Elements());
    }

    public void Save()
    {
        var figuresXML = new XDocument(new XElement("Figures"));

        foreach (var figureElement in figureElements)
        {
            figuresXML.Root.Add(figureElement);
        }
        
        figuresXML.Save(path);
        Debug.Log("Saved to " + path);
    }
}
