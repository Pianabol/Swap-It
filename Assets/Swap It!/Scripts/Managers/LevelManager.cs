using UnityEngine;
using System.Collections;

public class LevelManager : MonoBehaviour, IGameStateListener
{
    public static LevelManager Instance { get; private set; }

    [Header("Core References")]
    [SerializeField] private Board board;
    [SerializeField] private ItemPool itemPool;

    [Header("Current Level")]
    [SerializeField] private LevelData currentLevel; 
    [SerializeField] private int currentLevelNum = 1;

    public int CurrentLevelNum => currentLevelNum;
    public LevelData CurrentLevel => currentLevel;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void OnEnable()
    {
        GameManager.Instance?.RegisterListener(this);

        if (board != null)
        {
            board.ItemDestroyedEvent += OnItemDestroyedOnBoard;
            board.OnGravityFinished += RefillBoard; 
        }
    }

    private void OnDisable()
    {
        GameManager.Instance?.UnregisterListener(this);

        if (board != null)
        {
            board.ItemDestroyedEvent -= OnItemDestroyedOnBoard;
            board.OnGravityFinished -= RefillBoard; 
        }
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
        if (currentLevel == null)
        {
            Debug.LogError("[LEVEL MANAGER] LevelData atanmamış!");
            return;
        }

        board.InitializeBoard(currentLevel);
        GenerateLevel();
    }

    private void GenerateLevel()
    {
        for (int x = 0; x < board.Width; x++)
        {
            for (int y = 0; y < board.Height; y++)
            {
                if (!board.IsCellPlayable(x, y))
                {
                    continue; 
                }

                Item safeItem = GetSafeItemForPosition(x, y);
                board.PlaceItemAt(safeItem, x, y);
            }
        }
        Debug.Log("<color=cyan>[LEVEL MANAGER]</color> Level veriye göre dolduruldu.");
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

        if (attempts >= maxAttempts)
        {
            newItem = itemPool.GetRandomItem(); 
        }

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
