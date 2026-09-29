using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelManager : MonoBehaviour, IGameStateListener
{
    public static LevelManager Instance { get; private set; }

    [Header("Core References")]
    [SerializeField] private Board board;
    [SerializeField] private ItemPool itemPool;

    [Header("Levels Setup")]
    [Tooltip("Oyundaki tüm levelleri sırasıyla buraya sürükle bırak.")]
    [SerializeField] private List<LevelData> allLevels;

    private int currentLevelIndex = 0;

    public LevelData CurrentLevel =>
        allLevels != null && allLevels.Count > 0
            ? allLevels[currentLevelIndex]
            : null;

    public int CurrentLevelNum =>
        CurrentLevel != null
            ? CurrentLevel.levelNumber
            : 0;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;

            currentLevelIndex =
                PlayerPrefs.GetInt(
                    "SavedLevelIndex",
                    0
                );

            if (allLevels != null &&
                allLevels.Count > 0)
            {
                currentLevelIndex %=
                    allLevels.Count;
            }
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        GameManager.Instance?.RegisterListener(this);

        if (board != null)
        {
            board.ItemDestroyedEvent +=
                OnItemDestroyedOnBoard;

            board.OnGravityFinished +=
                RefillBoard;
        }

        if (GoalManager.Instance != null)
        {
            GoalManager.Instance.OnLevelCompleted +=
                SaveNextLevelProgress;
        }
    }

    private void OnDestroy()
    {
        GameManager.Instance?.UnregisterListener(this);

        if (board != null)
        {
            board.ItemDestroyedEvent -=
                OnItemDestroyedOnBoard;

            board.OnGravityFinished -=
                RefillBoard;
        }

        if (GoalManager.Instance != null)
        {
            GoalManager.Instance.OnLevelCompleted -=
                SaveNextLevelProgress;
        }
    }

    private void SaveNextLevelProgress()
    {
        int nextIndex =
            PlayerPrefs.GetInt(
                "SavedLevelIndex",
                0
            ) + 1;

        PlayerPrefs.SetInt(
            "SavedLevelIndex",
            nextIndex
        );

        PlayerPrefs.Save();

        Debug.Log(
            $"<color=cyan>[LEVEL MANAGER]</color> " +
            $"İlerleme kaydedildi! " +
            $"Sonraki level indeksi: {nextIndex}"
        );
    }

    public void GameStateChangedCallBack(
        EGameState gameState)
    {
        if (gameState == EGameState.GAME)
        {
            LoadCurrentLevel();
        }
    }

    private void LoadCurrentLevel()
    {
        if (CurrentLevel == null)
        {
            Debug.LogError(
                "[LEVEL MANAGER] " +
                "Level listesi boş veya " +
                "LevelData atanmamış!"
            );

            return;
        }

        board.InitializeBoard(CurrentLevel);

        GenerateLevel();

        if (GoalManager.Instance != null)
        {
            GoalManager.Instance.InitializeGoals(
                CurrentLevel.levelGoals,
                CurrentLevel.maxMoves,
                board
            );
        }

        if (CameraManager.Instance != null)
        {
           CameraManager.Instance.FrameBoard(
        board,
        CurrentLevel.width,
        CurrentLevel.height
    );
        }

        GoalUIManager uiManager =
            FindObjectOfType<GoalUIManager>();

        if (uiManager != null)
        {
            uiManager.InitializeUI(
                CurrentLevel.levelGoals,
                CurrentLevel.maxMoves
            );
        }

        Debug.Log(
            $"<color=green>[LEVEL MANAGER]</color> " +
            $"Level {CurrentLevel.levelNumber} " +
            $"başarıyla yüklendi."
        );
    }

    private void GenerateLevel()
    {
        for (int x = 0; x < board.Width; x++)
        {
            for (int y = 0; y < board.Height; y++)
            {
                if (!board.IsCellPlayable(x, y))
                    continue;

                Item safeItem =
                    GetSafeItemForPosition(x, y);

                board.PlaceItemAt(
                    safeItem,
                    x,
                    y
                );
            }
        }
    }

    private Item GetAllowedRandomItem()
    {
        if (CurrentLevel != null &&
            CurrentLevel.allowedColors != null &&
            CurrentLevel.allowedColors.Count > 0)
        {
            int randomIndex =
                Random.Range(
                    0,
                    CurrentLevel.allowedColors.Count
                );

            ItemType randomType =
                CurrentLevel.allowedColors[randomIndex];

            return itemPool.GetItem(randomType);
        }

        // Fallback
        return itemPool.GetRandomItem();
    }

    private Item GetSafeItemForPosition(
        int x,
        int y)
    {
        const int maxAttempts = 100;

        int attempts = 0;
        Item newItem = null;

        do
        {
            newItem =
                GetAllowedRandomItem();

            attempts++;

            if (newItem != null &&
                CausesMatch(
                    newItem.ItemType,
                    x,
                    y
                ))
            {
                itemPool.ReturnItem(newItem);
                newItem = null;
            }

        } while (
            newItem == null &&
            attempts < maxAttempts
        );

        // Çok ekstrem durumda fallback.
        if (newItem == null)
        {
            newItem =
                GetAllowedRandomItem();
        }

        return newItem;
    }

    private bool CausesMatch(
        ItemType type,
        int x,
        int y)
    {
        // Horizontal başlangıç match kontrolü
        if (x >= 2)
        {
            Item left1 =
                board.GetItemAt(x - 1, y);

            Item left2 =
                board.GetItemAt(x - 2, y);

            if (left1 != null &&
                left2 != null &&
                left1.ItemType == type &&
                left2.ItemType == type)
            {
                return true;
            }
        }

        // Vertical başlangıç match kontrolü
        if (y >= 2)
        {
            Item down1 =
                board.GetItemAt(x, y - 1);

            Item down2 =
                board.GetItemAt(x, y - 2);

            if (down1 != null &&
                down2 != null &&
                down1.ItemType == type &&
                down2.ItemType == type)
            {
                return true;
            }
        }

        return false;
    }

    private void OnItemDestroyedOnBoard(
        Item itemToReturn)
    {
        if (itemPool == null ||
            itemToReturn == null)
        {
            return;
        }

        itemPool.ReturnItem(itemToReturn);
    }

    private void RefillBoard()
    {
        StartCoroutine(
            RefillRoutine()
        );
    }

    private IEnumerator RefillRoutine()
    {
        bool refilledAny = false;

        int[] spawnCounts =
            new int[board.Width];

        for (int x = 0; x < board.Width; x++)
        {
            for (int y = 0; y < board.Height; y++)
            {
                if (!board.IsCellPlayable(x, y))
                    continue;

                if (board.GetItemAt(x, y) != null)
                    continue;

                Item newItem =
                    GetAllowedRandomItem();

                if (newItem == null)
                    continue;

                board.PlaceItemAt(
                    newItem,
                    x,
                    y
                );

                int spawnY =
                    board.Height + spawnCounts[x];

                newItem.transform.position =
                    board.GridToWorld(
                        x,
                        spawnY
                    );

                LeanTween
                    .move(
                        newItem.gameObject,
                        board.GridToWorld(x, y),
                        0.4f
                    )
                    .setEaseOutQuad();

                spawnCounts[x]++;

                refilledAny = true;
            }
        }

        if (refilledAny)
        {
            yield return new WaitForSeconds(0.5f);
        }

        board.CheckAndResolveMatches();
    }
}