using UnityEngine;
using System.Collections;
using UnityEngine.UI;

class Graphics : MonoBehaviour
{
    private const float WaitTime = 4f;

    [SerializeField]
    private RectTransform menuScreen;

    [SerializeField]
    private RectTransform gameGui;

    [SerializeField]
    private Button startButton;

    [SerializeField]
    private Button cheatButton;

    [SerializeField]
    private Text timer;

    [SerializeField]
    private Text points;

    [SerializeField]
    private RectTransform resultsGui;

    [SerializeField]
    private Text gamePoints;


    private void Start()
    {
        if (!menuScreen || !gameGui || !timer || !points ||
            !resultsGui || !gamePoints || !cheatButton || !startButton)
        {
            Debug.LogError("Graphics: fields are not set");
            return;
        }

        startButton.interactable = false;
        cheatButton.onClick.AddListener(() => cheatButton.gameObject.SetActive(false));
        ShowMenu();
    }

    public void ShowMenu()
    {
        menuScreen.gameObject.SetActive(true);
        startButton.interactable = true;
        resultsGui.gameObject.SetActive(false); 
        gameGui.gameObject.SetActive(false);
    }

    public void Initialize(Game game)
    {
        if (game == null)
        {
            Debug.LogError("Graphics: Init() failed: game is null");
        }

        game.GameStarted += OnGameStarted;
        game.GameOver += OnGameComplete;
        game.RoundStarted += OnRoundStarted;
        game.RoundComplete += OnRoundComplete;
        game.TimeUpdated += OnTimeUpdated;
    }

    private void OnGameStarted(object sender, System.EventArgs e)
    {
        menuScreen.gameObject.SetActive(false);
        gameGui.gameObject.SetActive(true);
    }

    private void OnTimeUpdated(object sender, TimeUpdatedEventArgs e)
    {
        timer.text = string.Format("{0:D2},{1:D3} c", e.Time.Seconds, e.Time.Milliseconds);
    }

    private void OnRoundStarted(object sender, RoundStartedEventArgs e)
    {
        cheatButton.gameObject.SetActive(true);
    }

    private void OnRoundComplete(object sender, RoundCompleteEventArgs e)
    {
        if (points.gameObject.activeInHierarchy)
        {
            points.text = e.TotalPoints.ToString();
        }
    }

    private void OnGameComplete(object sender, GameOverEventArgs e)
    {
        var game = sender as Game;
        game.GameStarted -= OnGameStarted;
        game.TimeUpdated -= OnTimeUpdated;
        game.RoundStarted -= OnRoundStarted;
        game.RoundComplete -= OnRoundComplete;
        game.GameOver -= OnGameComplete;
        
        StartCoroutine(GameComplete(e));
    }

    private IEnumerator GameComplete(GameOverEventArgs e)
    {
        resultsGui.gameObject.SetActive(true);
        gamePoints.text = "Вы набрали очков: " + e.Points.ToString();
        yield return null;
    }
}

