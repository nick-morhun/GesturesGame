using System;
using System.Collections.Generic;
using UnityEngine;

class Game
{
    private int currentRoundIndex = 0;

    private double roundTimeLeft = 0;

    private double roundTimeLeftSec = 0;

    private double[] roundTimes = null;

    private bool hasStarted;

    private Player player;

    private int totalPoints;

    private int pointsPerRound = 1;

    public event EventHandler<TimeUpdatedEventArgs> TimeUpdated = delegate { };

    public event EventHandler GameStarted = delegate { };

    public event EventHandler<RoundStartedEventArgs> RoundStarted = delegate { };

    public event EventHandler<RoundCompleteEventArgs> RoundComplete = delegate { };

    public event EventHandler<GameOverEventArgs> GameOver = delegate { };

    public bool IsRunning { get; private set; }

    public Game(Player player, double[] roundTimes, int pointsPerRound)
    {
        this.player = player;
        this.pointsPerRound = Mathf.Clamp(pointsPerRound, 1, int.MaxValue);

        this.roundTimes = new double[roundTimes.Length];
        Array.Copy(roundTimes, this.roundTimes, roundTimes.Length);
    }

    public void Start()
    {
        if (hasStarted)
        {
            return;
        }

        hasStarted = true;
        currentRoundIndex = -1;
        totalPoints = 0;
        GameStarted(this, new EventArgs());
    }

    public void StartNextRound()
    {
        currentRoundIndex++;
        roundTimeLeftSec = roundTimeLeft = roundTimes[currentRoundIndex];
        IsRunning = true;
        ClockTick();
        RoundStarted(this, new RoundStartedEventArgs() { Index = currentRoundIndex });
        Debug.Log("Round " + currentRoundIndex + " started");
    }

    public void CompleteRound()
    {
        totalPoints += pointsPerRound;
        IsRunning = false;
        RoundComplete(this, new RoundCompleteEventArgs() { TotalPoints = totalPoints });
        Debug.Log("Round " + currentRoundIndex + " complete");
    }

    public void Update(double dt)
    {
        if (!IsRunning)
        {
            return;
        }

        if (roundTimeLeft <= 0)
        {
            FinishGame();
            return;
        }

        roundTimeLeft -= dt;
    }

    public void TimeTick(double dt)
    {
        if (IsRunning)
        {
            roundTimeLeftSec -= dt;
            ClockTick();
        }
    }

    private void ClockTick()
    {
        var args = new TimeUpdatedEventArgs() { Time = TimeSpan.FromSeconds(roundTimeLeftSec) };
        TimeUpdated(this, args);
    }

    public void FinishGame()
    {
        Debug.Log("Game over");

        if (currentRoundIndex > 0)
        {
            player.AddPoints(totalPoints);
        }

        IsRunning = false;
        hasStarted = false;
        GameOver(this, new GameOverEventArgs()
        {
            RoundsCompleted = currentRoundIndex,
            Points = totalPoints
        });
    }
}

class TimeUpdatedEventArgs : EventArgs
{
    public TimeSpan Time { get; set; }
}

class RoundStartedEventArgs : EventArgs
{
    public int Index { get; set; }
}

class RoundCompleteEventArgs : EventArgs
{
    public int TotalPoints { get; set; }
}

class GameOverEventArgs : EventArgs
{
    public int RoundsCompleted { get; set; }

    public int Points { get; set; }
}