using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[DisallowMultipleComponent]
public class GameController : MonoBehaviour
{
    private const int millisPerRoundMin = 500;

    private Game game;

    private Player player;

    private List<Line> candidateLines;

    [SerializeField]
    private int millisPerRoundMax = 10000;

    [SerializeField]
    private Transform gameLevel;

    [SerializeField]
    private Graphics graphics;

    [SerializeField]
    private InputManager input;

    [SerializeField]
    private Figure figure;

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

    private void Awake()
    {
        if (!gameLevel || !input || !graphics)
        {
            Debug.LogError("GameController: fields are not set");
            return;
        }

        player = new Player();

        game = new Game(player, CalculateRoundTimes(50), 1);
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

        if (times[0] < millisPerRoundMin / millisPerRoundMin)
        {
            times[0] = 1;
        }

        return times;
    }

    private void OnGameStarted(object sender, System.EventArgs e)
    {
        LoadFigure();
    }

    private void OnGameOver(object sender, GameOverEventArgs e)
    {
        Unsubscribe();
        RemoveFigure();
    }

    private void OnGameRoundStarted(object sender, RoundStartedEventArgs e)
    {
        input.TouchStarted += figure.OnInputTouchStarted;
        input.TouchEnded += figure.StartTry;
        input.PointerMoved += figure.OnInputPointerMoved;
        input.AcceptInput = true;
        figure.DrawSuccess += CompleteRound;
    }

    private void OnGameRoundComplete(object sender, RoundCompleteEventArgs e)
    {
        Unsubscribe();
        RemoveFigure();
        StartCoroutine(NextRound());
    }

    private void Unsubscribe()
    {
        input.AcceptInput = false;
        input.TouchEnded -= figure.StartTry;
        input.TouchStarted -= figure.OnInputTouchStarted;
        input.PointerMoved -= figure.OnInputPointerMoved;
        figure.DrawSuccess -= CompleteRound;
        figure.Ready -= game.StartNextRound;
    }

    private IEnumerator NextRound()
    {
        yield return new WaitForSeconds(.5f);

        if (game != null)
        {
            LoadFigure();
        }
    }

    private void LoadFigure()
    {
        // TODO: Load new figure
        figure.gameObject.SetActive(true);
        figure.Init();
        figure.StartTry();
        figure.Ready += game.StartNextRound;
    }

    private void RemoveFigure()
    {
        // TODO: destroy GO
        figure.gameObject.SetActive(false);
    }
}
