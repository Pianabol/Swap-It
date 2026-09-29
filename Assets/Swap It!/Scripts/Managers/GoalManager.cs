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
        if (isGameOver) return;

        ItemType type = destroyedItem.ItemType;

        if (activeGoals.ContainsKey(type) && activeGoals[type] > 0)
        {
            activeGoals[type]--;
            OnGoalUpdated?.Invoke(type, activeGoals[type]);
            CheckWinCondition();
        }
    }

    public void DecreaseMove()
    {
        if (isGameOver) return;

        remainingMoves--;
        OnMovesUpdated?.Invoke(remainingMoves);

        if (remainingMoves <= 0)
        {
            CheckWinCondition(); 
            if (!isGameOver) 
            {
                isGameOver = true;
                OnLevelFailed?.Invoke();
            }
        }
    }

    private void CheckWinCondition()
    {
        foreach (var count in activeGoals.Values)
        {
            if (count > 0) return; 
        }

        isGameOver = true;
        OnLevelCompleted?.Invoke();
    }
}