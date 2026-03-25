using UnityEngine;
using System;
using System.Collections.Generic;

public enum GameState { Menu, Preparation, AttackerTurn, Battle, RoundEnd, GameOver }
public enum GameMode { SinglePlayer, HotSeat }

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    public GameState State { get; private set; }
    public GameMode Mode { get; private set; }
    public int Gold { get; private set; }
    public int BaseHP { get; private set; }
    public int MaxBaseHP { get; private set; }
    public int Round { get; private set; }
    public int AttackBudget { get; private set; }
    public bool DefenderWon { get; private set; }
    public int LastBonusGold { get; private set; }

    public const int MaxRounds = 10;
    public const int StartGold = 300;
    public const int StartBaseHP = 20;
    public const int StartAttackBudget = 200;
    public const int BudgetIncreasePerRound = 30;

    public event Action<GameState> OnStateChanged;
    public event Action OnStatsChanged;

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        SetState(GameState.Menu);
    }

    public void StartGame(GameMode mode)
    {
        Mode = mode;
        StartNewGame();
    }

    public void StartNewGame()
    {
        Gold = StartGold;
        BaseHP = StartBaseHP;
        MaxBaseHP = StartBaseHP;
        Round = 1;
        AttackBudget = StartAttackBudget;
        DefenderWon = false;
        LastBonusGold = 0;
        OnStatsChanged?.Invoke();
        SetState(GameState.Preparation);
    }

    public void StartBattle()
    {
        if (State == GameState.Preparation)
        {
            if (Mode == GameMode.HotSeat)
            {
                SetState(GameState.AttackerTurn);
                return;
            }
            SetState(GameState.Battle);
        }
        else if (State == GameState.AttackerTurn)
        {
            SetState(GameState.Battle);
        }
    }

    public void LaunchWave(List<AIAttacker.EnemyWaveEntry> wave)
    {
        if (WaveSpawner.Instance != null)
            WaveSpawner.Instance.SpawnWave(wave);
    }

    public void EnemyReachedBase()
    {
        if (State != GameState.Battle) return;
        BaseHP = Mathf.Max(0, BaseHP - 1);
        OnStatsChanged?.Invoke();
        if (BaseHP <= 0)
        {
            DefenderWon = false;
            SetState(GameState.GameOver);
        }
    }

    public void EnemyKilled(int goldReward)
    {
        AddGold(goldReward);
    }

    public void AddGold(int amount)
    {
        Gold += amount;
        OnStatsChanged?.Invoke();
    }

    public bool SpendGold(int amount)
    {
        if (Gold < amount) return false;
        Gold -= amount;
        OnStatsChanged?.Invoke();
        return true;
    }

    public void WaveComplete()
    {
        if (State != GameState.Battle) return;
        SetState(GameState.RoundEnd);
    }

    void SetState(GameState newState)
    {
        State = newState;
        OnStateChanged?.Invoke(State);

        if (newState == GameState.Battle)
        {
            if (Mode == GameMode.SinglePlayer)
            {
                var wave = AIAttacker.FormWave(AttackBudget, Round);
                LaunchWave(wave);
            }
            // In HotSeat, wave is launched from GameUIManager after attacker submits
        }
        else if (newState == GameState.RoundEnd)
        {
            ProcessRoundEnd();
        }
    }

    void ProcessRoundEnd()
    {
        if (BaseHP <= 0)
        {
            DefenderWon = false;
            SetState(GameState.GameOver);
            return;
        }

        LastBonusGold = 20 + Round * 5;
        AddGold(LastBonusGold);

        Round++;
        AttackBudget += BudgetIncreasePerRound;

        if (Round > MaxRounds)
        {
            DefenderWon = true;
            SetState(GameState.GameOver);
            return;
        }

        OnStatsChanged?.Invoke();
        // Delay transition to Preparation so RoundEnd panel is shown
    }

    public void ContinueAfterRoundEnd()
    {
        if (State == GameState.RoundEnd)
            SetState(GameState.Preparation);
    }
}
