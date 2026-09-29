using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class LevelManager : MonoBehaviour, IGameStateListener
{
    public static LevelManager Instance { get; private set; }

    [Header("Core References")]
    [SerializeField] private Board board;
    [SerializeField] private ItemPool itemPool;

    [Header("Levels Setup")]
    [Tooltip("Oyundaki tüm levelleri sırasıyla buraya sürükle bırak.")]
    [SerializeField] private List<LevelData> allLevels;
    
    // Arka planda tuttuğumuz, oyuncunun kaçıncı sırada olduğunu belirten indeks
    private int currentLevelIndex = 0;

    // Arayüzlerin ve Board'un okuyacağı Aktif Level
    public LevelData CurrentLevel => allLevels != null && allLevels.Count > 0 ? allLevels[currentLevelIndex] : null;
    
    // Aktif levelin kendi içindeki "Level Number" verisi (PopUp için)
    public int CurrentLevelNum => CurrentLevel != null ? CurrentLevel.levelNumber : 0;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            
            currentLevelIndex = PlayerPrefs.GetInt("SavedLevelIndex", 0);
            
            if (allLevels != null && allLevels.Count > 0)
            {
                // Güvenlik: 
                // hata vermesin diye Modulo (%) ile başa 
                currentLevelIndex = currentLevelIndex % allLevels.Count;
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
            board.ItemDestroyedEvent += OnItemDestroyedOnBoard;
            board.OnGravityFinished += RefillBoard; 
        }

        // YENİ: Hedefler bittiğinde sıradaki levelin kilidini açmak için abone ol
        if (GoalManager.Instance != null)
        {
            GoalManager.Instance.OnLevelCompleted += SaveNextLevelProgress;
        }
    }

    private void OnDestroy()
    {
        GameManager.Instance?.UnregisterListener(this);

        if (board != null)
        {
            board.ItemDestroyedEvent -= OnItemDestroyedOnBoard;
            board.OnGravityFinished -= RefillBoard; 
        }

        if (GoalManager.Instance != null)
        {
            GoalManager.Instance.OnLevelCompleted -= SaveNextLevelProgress;
        }
    }

    private void SaveNextLevelProgress()
    {
        int nextIndex = PlayerPrefs.GetInt("SavedLevelIndex", 0) + 1;
        PlayerPrefs.SetInt("SavedLevelIndex", nextIndex);
        PlayerPrefs.Save();
        Debug.Log($"<color=cyan>[LEVEL MANAGER]</color> İlerleme kaydedildi! Sonraki level indeksi: {nextIndex}");
    }

    public void GameStateChangedCallBack(EGameState gameState)
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
            Debug.LogError("[LEVEL MANAGER] Level listesi boş veya LevelData atanmamış!");
            return;
        }

        board.InitializeBoard(CurrentLevel);
        GenerateLevel();

        if (GoalManager.Instance != null)
        {
            GoalManager.Instance.InitializeGoals(CurrentLevel.levelGoals, CurrentLevel.maxMoves, board);
        }

        if (CameraManager.Instance != null)
        {
            CameraManager.Instance.FrameBoard(board);
        }

        GoalUIManager uiManager = FindObjectOfType<GoalUIManager>();
        if (uiManager != null)
        {
            uiManager.InitializeUI(CurrentLevel.levelGoals, CurrentLevel.maxMoves);
        }
        
        Debug.Log($"<color=green>[LEVEL MANAGER]</color> Level {CurrentLevel.levelNumber} başarıyla yüklendi.");
    }

    private void GenerateLevel()
    {
        for (int x = 0; x < board.Width; x++)
        {
            for (int y = 0; y < board.Height; y++)
            {
                if (!board.IsCellPlayable(x, y)) continue; 

                Item safeItem = GetSafeItemForPosition(x, y);
                board.PlaceItemAt(safeItem, x, y);
            }
        }
    }

    private Item GetSafeItemForPosition(int x, int y)
    {
        int maxAttempts = 100;
        int attempts = 0;
        Item newItem;

        do
        {
            newItem = itemPool.GetRandomItem();
            attempts++;

            if (CausesMatch(newItem.ItemType, x, y))
            {
                itemPool.ReturnItem(newItem);
                newItem = null; 
            }

        } while (newItem == null && attempts < maxAttempts);

        if (attempts >= maxAttempts) newItem = itemPool.GetRandomItem(); 

        return newItem;
    }

    private bool CausesMatch(ItemType type, int x, int y)
    {
        if (x >= 2)
        {
            Item left1 = board.GetItemAt(x - 1, y);
            Item left2 = board.GetItemAt(x - 2, y);
            if (left1 != null && left2 != null && left1.ItemType == type && left2.ItemType == type) return true;
        }

        if (y >= 2)
        {
            Item down1 = board.GetItemAt(x, y - 1);
            Item down2 = board.GetItemAt(x, y - 2);
            if (down1 != null && down2 != null && down1.ItemType == type && down2.ItemType == type) return true;
        }

        return false;
    }

    private void OnItemDestroyedOnBoard(Item itemToReturn)
    {
        if (itemPool != null && itemToReturn != null)
        {
            itemPool.ReturnItem(itemToReturn);
        }
    }

    private void RefillBoard()
    {
        StartCoroutine(RefillRoutine());
    }

    private IEnumerator RefillRoutine()
    {
        bool refilledAny = false;
        int[] spawnCounts = new int[board.Width]; 

        for (int x = 0; x < board.Width; x++)
        {
            for (int y = 0; y < board.Height; y++)
            {
                if (board.IsCellPlayable(x, y) && board.GetItemAt(x, y) == null)
                {
                    Item newItem = itemPool.GetRandomItem();
                    board.PlaceItemAt(newItem, x, y); 

                    float spawnY = board.Height + spawnCounts[x];
                    newItem.transform.position = board.GridToWorld(x, (int)spawnY);

                    LeanTween.move(newItem.gameObject, board.GridToWorld(x, y), 0.4f).setEaseOutQuad();

                    spawnCounts[x]++; 
                    refilledAny = true;
                }
            }
        }

        if (refilledAny)
        {
            yield return new WaitForSeconds(0.5f);
            board.CheckAndResolveMatches(); 
        }
    }
}