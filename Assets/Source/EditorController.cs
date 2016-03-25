using UnityEngine;
using UnityEngine.SceneManagement;

[DisallowMultipleComponent]
public class EditorController : MonoBehaviour
{
    private FiguresXml figuresXml;

    //[SerializeField]
    //private Graphics graphics;

    [SerializeField]
    private InputManager input;

    [SerializeField]
    private EditorFigure figure;

    public void NewFigure()
    {
        figure.StartNewFigure();
        input.Reset();
    }

    public void SaveFigure()
    {
        figure.CompleteFigure();
        figuresXml.figureElements.Add(figure.Save());
        figuresXml.Save();
        NewFigure();
    }

    public void SwitchToGame()
    {
        SceneManager.LoadScene("main");
    }

    private void Awake()
    {
        if (!figure || !input)
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
    }

    private void OnDestroy()
    {
        input.TouchStarted -= OnInputTouchStarted;
        input.PointerMoved -= OnInputPointerMoved;
        input.TouchEnded -= OnInputTouchEnded;
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
}
