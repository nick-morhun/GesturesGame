using UnityEngine;
using UnityEngine.SceneManagement;

[DisallowMultipleComponent]
public class EditorController : MonoBehaviour
{
    private FiguresXml figuresXml;

    [SerializeField]
    private EditorGraphics graphics;

    [SerializeField]
    private InputManager input;

    [SerializeField]
    private EditorFigure figure;

    public void NewFigure()
    {
        figure.StartNewFigure();
        input.Reset();
        input.AcceptInput = true;
    }

    public void SaveFigure()
    {
        figure.CompleteFigure();

        var figureXml = new FigureXml();
        figuresXml.figureElements.Add(figureXml.GetXml(figure));
        figuresXml.Save();
        figure.StartNewFigure();
    }

    public void CleanAllFigures()
    {
        figuresXml.figureElements.Clear();
        figuresXml.Save();
        figuresXml.Load();
        NewFigure();
    }

    public void SwitchToGame()
    {
        SceneManager.LoadScene("main");
    }

    private void Awake()
    {
        if (!figure || !input || !graphics)
        {
            Debug.LogError("EditorController: fields are not set");
            return;
        }

        figuresXml = new FiguresXml();
        figuresXml.Load();

        input.TouchStarted += OnInputTouchStarted;
        input.PointerMoved += OnInputPointerMoved;
        input.TouchEnded += OnInputTouchEnded;
        input.AcceptInput = true;
        figure.ValidationFailed += OnValidationFailed;
        figuresXml.SaveSuccessful += graphics.OnSaveSuccessful;
        figuresXml.SaveFailed += graphics.OnSaveSaveFailed;
    }

    private void OnDestroy()
    {
        input.TouchStarted -= OnInputTouchStarted;
        input.PointerMoved -= OnInputPointerMoved;
        input.TouchEnded -= OnInputTouchEnded;
        figure.ValidationFailed -= OnValidationFailed;
        figuresXml.SaveSuccessful -= graphics.OnSaveSuccessful;
        figuresXml.SaveFailed -= graphics.OnSaveSaveFailed;
    }

    private void OnInputTouchStarted(Vector3 pointerPos)
    {
        figure.AddLine(pointerPos);
    }

    private void OnInputPointerMoved(Vector3 pointerPos)
    {
        figure.DragLineEnd(pointerPos);
    }

    private void OnInputTouchEnded()
    {
        figure.CompleteLine();
    }

    private void OnValidationFailed()
    {
        input.AcceptInput = false;
    }
}
