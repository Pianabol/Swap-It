using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class GoalUIManager : MonoBehaviour
{
    [Header("Goal References (Left)")]
    [SerializeField] private Transform leftGoalsArea; 
    [SerializeField] private GoalUIItem goalItemPrefab; 

    [Header("Moves References (Right)")]
    [SerializeField] private TextMeshProUGUI movesText; 

    [System.Serializable]
    public struct GoalSpriteMap
    {
        public ItemType type;
        public Sprite icon;
    }

    [Header("Visual Assets")]
    public List<GoalSpriteMap> spriteDatabase; 

    private Dictionary<ItemType, GoalUIItem> spawnedGoalItems = new Dictionary<ItemType, GoalUIItem>();
    private Vector3 initialMovesScale; 

    private void Awake()
    {
        if (movesText != null)
        {
            initialMovesScale = movesText.transform.localScale;
        }
    }

    private void Start()
    {
        // Start'ta yapıyoruz ki GoalManager kesin uyanmış (Awake olmuş) olsun, sağır kalmasın!
        if (GoalManager.Instance != null)
        {
            GoalManager.Instance.OnGoalUpdated += UpdateGoalUI;
            GoalManager.Instance.OnMovesUpdated += UpdateMovesUI;
        }
    }

    private void OnDestroy()
    {
        if (GoalManager.Instance != null)
        {
            GoalManager.Instance.OnGoalUpdated -= UpdateGoalUI;
            GoalManager.Instance.OnMovesUpdated -= UpdateMovesUI;
        }
    }

    public void InitializeUI(List<LevelGoal> levelGoals, int startingMoves)
    {
        // 1. Hedefleri Diz
        foreach (Transform child in leftGoalsArea)
        {
            Destroy(child.gameObject);
        }
        spawnedGoalItems.Clear();

        foreach (var goal in levelGoals)
        {
            GoalUIItem newGoalUI = Instantiate(goalItemPrefab, leftGoalsArea);
            Sprite correctIcon = GetSpriteForType(goal.targetItemType);
            
            newGoalUI.Setup(goal.targetItemType, goal.targetAmount, correctIcon);
            spawnedGoalItems.Add(goal.targetItemType, newGoalUI);
        }

        if (movesText != null)
        {
            movesText.text = startingMoves.ToString();
        }
    }

    private void UpdateGoalUI(ItemType type, int remainingAmount)
    {
        if (spawnedGoalItems.ContainsKey(type))
        {
            spawnedGoalItems[type].UpdateAmount(remainingAmount);
        }
    }

    private void UpdateMovesUI(int moves)
    {
        if (movesText == null) return;

        movesText.text = moves.ToString();

        LeanTween.cancel(movesText.gameObject);
        movesText.transform.localScale = initialMovesScale;
        LeanTween.scale(movesText.gameObject, initialMovesScale * 1.3f, 0.2f).setEasePunch();
    }

    private Sprite GetSpriteForType(ItemType type)
    {
        foreach (var map in spriteDatabase)
        {
            if (map.type == type) return map.icon;
        }
        return null; 
    }
}