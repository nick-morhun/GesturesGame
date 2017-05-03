using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

public interface IFigureReader
{
    IEnumerable<ILineReader> LineReaders { get; }
}

public interface IFigureWriter
{
    void Save(IEnumerable<Line> figureLines);
}

public interface ILineReader
{
    string GetValue(string key);
}

public interface ILineWriter
{
    /// <summary>
    /// Erases current contents.
    /// </summary>
    void NewLine();

    void SetValue(string key, object value);
}
