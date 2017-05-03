using System.Collections.Generic;
using System.IO;
using System.Xml.Linq;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// Loads and saves <see cref="FiguresXml.figureElements"/> to a file.
/// </summary>
public class FiguresXml
{
    public List<XElement> figureElements = new List<XElement>();

    private string defaultXmlResourcePath
    {
        get { return "figures"; }
    }

    private string path
    {
        get { return Application.persistentDataPath + "/figures.xml"; }
    }

    public event UnityAction SaveSuccessful = delegate { };

    public event UnityAction SaveFailed = delegate { };

    public void Load()
    {
        Debug.LogFormat("Figures file path: {0}", path);
        XDocument figuresXML = null;

        try
        {
            if (File.Exists(path))
            {
                figuresXML = XDocument.Load(path);
            }
            else
            {
                TextAsset xml = Resources.Load<TextAsset>(defaultXmlResourcePath);
                figuresXML = XDocument.Load(new StringReader(xml.text));
            }
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

        try
        {
            figuresXML.Save(path);
            Debug.Log("Saved to " + path);
            SaveSuccessful();
        }
        catch (System.IO.IOException ex)
        {
            Debug.LogError(ex.Message);
            SaveFailed();
        }
    }
}
