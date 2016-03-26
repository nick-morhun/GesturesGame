using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[DisallowMultipleComponent]
public class GameController : MonoBehaviour
{
    private const int millisPerRoundMin = 500;

    private Game game;

    private Player player;

    private FiguresXml figuresXml;

    private int currentFigureIdx;

    [SerializeField]
    private int millisPerRoundMax = 10000;

    [SerializeField]
    private bool loadFigures;

    [SerializeField]
    private Graphics graphics;

    [SerializeField]
    private InputManager input;

    [SerializeField]
    private GameFigure figure;

    [SerializeField]
    private CometPointer pointer;

    public void StartGame()
    {
        graphics.Initialize(game);
        game.Start();
    }

    // Public for a cheat
    public void CompleteRound()
    {
        game.CompleteRound();
        input.Reset();
    }

    public void SaveFigure()
    {
        figuresXml.figureElements.Add(figure.Save());
        figuresXml.Save();
    }

    private void Awake()
    {
        if (!figure || !input || !graphics || !pointer)
        {
            Debug.LogError("GameController: fields are not set");
            return;
        }

        player = new Player();
        int roundsCount = 10;

        figuresXml = new FiguresXml();
        figuresXml.Load();

        if (loadFigures)
        {
            roundsCount = figuresXml.figureElements.Count;
        }

        game = new Game(player, CalculateRoundTimes(roundsCount), 1);
        game.RoundStarted += OnGameRoundStarted;
        game.RoundComplete += OnGameRoundComplete;
        game.GameStarted += OnGameStarted;
        game.GameOver += OnGameOver;
    }

    private void FixedUpdate()
    {
        if (game != null && game.IsRunning)
        {
            game.Update(Time.fixedDeltaTime);
            game.TimeTick(Time.fixedDeltaTime);
        }
    }

    private void OnDestroy()
    {
        game.RoundStarted -= OnGameRoundStarted;
        game.RoundComplete -= OnGameRoundComplete;
        game.GameStarted -= OnGameStarted;
        game.GameOver -= OnGameOver;
    }

    /// <summary>
    /// Calculate round times.
    /// </summary>
    /// <param name="count">rounds count.</param>
    /// <returns>Returns array of round times in seconds</returns>
    private double[] CalculateRoundTimes(int count)
    {
        if (count < 1)
        {
            count = 1;
        }

        double[] times = new double[count];

        for (int i = 0; i < count; i++)
        {
            double t = (double)millisPerRoundMax / 1000 / (i + 1);

            if (t < millisPerRoundMin / 1000)
            {
                break;
            }

            times[i] = t;
            Debug.Log("Round #" + i + " lasts " + t + " sec");
        }

        if (times[0] < millisPerRoundMin / 1000)
        {
            times[0] = 1;
        }

        return times;
    }

    private void OnGameStarted(object sender, System.EventArgs e)
    {
        currentFigureIdx = -1;
        figure.Loaded += OnFigureLoaded;
        LoadNextFigure();
    }

    private void OnGameOver(object sender, GameOverEventArgs e)
    {
        Unsubscribe();
        pointer.Hide();

        if (loadFigures)
        {
            figure.Unload();
        }

        figure.Loaded -= OnFigureLoaded;
    }

    private void OnGameRoundStarted(object sender, RoundStartedEventArgs e)
    {
        input.TouchStarted += OnInputTouchStarted;
        input.TouchEnded += OnInputTouchEnded;
        input.PointerMoved += OnInputPointerMoved;
        input.AcceptInput = true;
        figure.DrawSuccess += CompleteRound;
        figure.LineAngleChanged += OnLineDetected;
    }

    private void OnGameRoundComplete(object sender, RoundCompleteEventArgs e)
    {
        Unsubscribe();
        pointer.Hide();

        if (loadFigures)
        {
            figure.Unload();
        }

        StartCoroutine(NextRound());
    }

    private void OnInputTouchStarted(Vector3 pointerPos)
    {
        figure.OnInputTouchStarted(pointerPos);
    }

    private void OnInputTouchEnded()
    {
        pointer.Hide();
    }

    private void OnInputPointerMoved(Vector3 pointerPos)
    {
        pointer.OnInputPointerMoved(pointerPos);
        figure.OnInputPointerMoved(pointerPos);
    }

    private void OnLineDetected(float angle)
    {
        pointer.SetAngle(angle);
    }

    private void Unsubscribe()
    {
        input.AcceptInput = false;
        input.TouchStarted -= OnInputTouchStarted;
        input.TouchEnded -= OnInputTouchEnded;
        input.PointerMoved -= OnInputPointerMoved;
        figure.DrawSuccess -= CompleteRound;
        figure.LineAngleChanged -= OnLineDetected;
        figure.Loaded -= game.StartNextRound;
    }

    private IEnumerator NextRound()
    {
        yield return new WaitForSeconds(.5f);

        if (game != null)
        {
            LoadNextFigure();
        }
    }

    private void LoadNextFigure()
    {
        if (!loadFigures)
        {
            figure.Load(null);
            return;
        }

        currentFigureIdx++;

        if (currentFigureIdx >= figuresXml.figureElements.Count)
        {
            Debug.LogWarning("No more figures available");
            game.FinishGame();
            return;
        }

        figure.Load(figuresXml.figureElements[currentFigureIdx]);
    }

    private void OnFigureLoaded()
    {
        if (figure.IsValid)
        {
            game.StartNextRound();
        }
        else if (loadFigures)
        {
            LoadNextFigure();
        }
    }
}
