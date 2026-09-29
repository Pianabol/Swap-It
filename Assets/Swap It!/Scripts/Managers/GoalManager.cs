using System.Collections.Generic;
using UnityEngine;
using System;

public class GoalManager : MonoBehaviour
{
    public static GoalManager Instance;

    public event Action<ItemType, int> OnGoalUpdated; 
    public event Action<int> OnMovesUpdated;
    public event Action OnLevelFailed;
    public event Action OnLevelCompleted;

    private Dictionary<ItemType, int> activeGoals = new Dictionary<ItemType, int>();
    private int remainingMoves;
    private bool isGameOver = false;

    private void Awake()
    {
        Instance = this;
    }

    public void InitializeGoals(List<LevelGoal> goals, int moves, Board board)
    {
        activeGoals.Clear();
        isGameOver = false;
        remainingMoves = moves;

        foreach (var goal in goals)
        {
            if (!activeGoals.ContainsKey(goal.targetItemType))
            {
                activeGoals.Add(goal.targetItemType, goal.targetAmount);
            }
        }

        board.ItemDestroyedEvent -= CheckGoalProgress;
        board.ItemDestroyedEvent += CheckGoalProgress;
        
        OnMovesUpdated?.Invoke(remainingMoves);
    }

    private void CheckGoalProgress(Item destroyedItem)
    {

        ItemType type = destroyedItem.ItemType;

        if (activeGoals.ContainsKey(type) && activeGoals[type] > 0)
        {
            activeGoals[type]--;
            OnGoalUpdated?.Invoke(type, activeGoals[type]);
        }
    }

    public void DecreaseMove()
    {
        if (isGameOver) return;

        remainingMoves--;
        OnMovesUpdated?.Invoke(remainingMoves);
    }

    // YENİ HAKEM FONKSİYONU: Board.cs "Tahta duruldu" dediğinde tetiklenecek.
    public void EvaluateGameState()
    {
        if (isGameOver) return;

        bool isWin = true;
        foreach (var count in activeGoals.Values)
        {
            if (count > 0) 
            { 
                isWin = false; 
                break; 
            }
        }

        if (isWin)
        {
            isGameOver = true;
            OnLevelCompleted?.Invoke();
        }
        else if (remainingMoves <= 0)
        {
            isGameOver = true;
            OnLevelFailed?.Invoke();
        }
    }
}