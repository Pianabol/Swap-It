using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(BoxCollider))]
public class Board : MonoBehaviour
{
    [Header("Board Settings")]
    [SerializeField, Min(1)] private int width = 8;
    [SerializeField, Min(1)] private int height = 10;
    [SerializeField, Min(0.1f)] private float cellSize = 1.0f;

    [Header("Hierarchy Settings")]
    [SerializeField] private Transform itemRoot;

    [Header("Visual Settings")]
    [SerializeField] private GameObject tilePrefab;
    [SerializeField] private Transform tileRoot;
    [SerializeField] private GameObject borderPrefab;
    [SerializeField] private float borderThicknessOffset = 0.5f;
    [SerializeField] private GameObject obstaclePrefab;

    [Header("Debug Settings")]
    [SerializeField] private bool showGizmos = true;
    [SerializeField] private Color gizmoColor = new Color(0, 1, 0, 0.5f);

    private BoxCollider boardCollider;

    private Item[,] gridItems;
    private CellType[,] cellTypes;
    private GameObject[,] obstacleObjects;

    private bool hasSelection = false;
    private bool isResolving = false;

    private Vector2Int selectedGridPos;
    private float selectionLiftHeight = 0.5f;

    public delegate void OnItemDestroyed(Item item);
    public event OnItemDestroyed ItemDestroyedEvent;

    public event System.Action OnGravityFinished;

    public int Width => width;
    public int Height => height;
    public float CellSize => cellSize;

    private void Awake()
    {
        if (itemRoot == null)
        {
            GameObject rootObj = new GameObject("ItemRoot");
            rootObj.transform.SetParent(transform);
            rootObj.transform.localPosition = Vector3.zero;
            itemRoot = rootObj.transform;
        }

        if (tileRoot == null)
        {
            GameObject rootObj = new GameObject("TileRoot");
            rootObj.transform.SetParent(transform);
            rootObj.transform.localPosition = Vector3.zero;
            tileRoot = rootObj.transform;
        }
    }

#region Board Initialization

    private void OnValidate()
    {
        InitializeCollider();
    }
    
    private void InitializeCollider()
    {
        if (boardCollider == null)
        {
            boardCollider = GetComponent<BoxCollider>();
        }

        if (boardCollider != null)
        {
            boardCollider.size = new Vector3(width * cellSize, 0.1f, height * cellSize);
            boardCollider.center = new Vector3((width * cellSize) / 2f, 0f, (height * cellSize) / 2f);
        }
    }

    public void InitializeBoard(LevelData levelData)
    {
        width = levelData.width;
        height = levelData.height;

        gridItems = new Item[width, height];
        cellTypes = new CellType[width, height];
        obstacleObjects = new GameObject[width, height];

        if (tileRoot != null)
        {
            foreach (Transform child in tileRoot)
            {
                Destroy(child.gameObject);
            }
        }

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                if (y < levelData.layout.Count && x < levelData.layout[y].columns.Length)
                {
                    cellTypes[x, y] = levelData.layout[y].columns[x];
                }
                else
                {
                    cellTypes[x, y] = CellType.Normal;
                }
            }
        }

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                if (cellTypes[x, y] != CellType.Void && tilePrefab != null)
                {
                    Vector3 tilePos = GridToWorld(x, y);
                    tilePos.y = -0.508f;
                    Quaternion tileRot = Quaternion.Euler(90f, 0f, 0f);

                    GameObject bgTile = Instantiate(tilePrefab, tilePos, tileRot, tileRoot);
                    bgTile.name = $"Tile_{x}_{y}";

                    float offset = cellSize / 2f;

                    if (!HasBoardSurface(x, y + 1)) SpawnBorder(x, y, new Vector3(0, 0, offset), true);
                    if (!HasBoardSurface(x, y - 1)) SpawnBorder(x, y, new Vector3(0, 0, -offset), true);
                    if (!HasBoardSurface(x + 1, y)) SpawnBorder(x, y, new Vector3(offset, 0, 0), false);
                    if (!HasBoardSurface(x - 1, y)) SpawnBorder(x, y, new Vector3(-offset, 0, 0), false);
                }

                if (cellTypes[x, y] == CellType.Obstacle && obstaclePrefab != null)
                {
                    Vector3 obstaclePos = GridToWorld(x, y);
                    obstaclePos.y = 0f;

                    GameObject obstacle = Instantiate(obstaclePrefab, obstaclePos, Quaternion.identity, tileRoot);
                    obstacle.name = $"Obstacle_{x}_{y}";
                    obstacleObjects[x, y] = obstacle;
                }
            }
        }

        InitializeCollider();
    }

    private void SpawnBorder(int x, int y, Vector3 offset, bool isHorizontal)
    {
        if (borderPrefab == null) return;

        Vector3 cellCenter = GridToWorld(x, y);
        Vector3 borderPos = cellCenter + offset;
        borderPos.y = 0.565f;

        GameObject border = Instantiate(borderPrefab, borderPos, Quaternion.identity, tileRoot);
        border.name = $"Border_{x}_{y}";

        float borderThickness = 0.1f;
        float extendedLength = cellSize + borderThickness;

        if (isHorizontal)
        {
            border.transform.localScale = new Vector3(extendedLength, borderThickness, borderThickness);
        }
        else
        {
            border.transform.localScale = new Vector3(borderThickness, borderThickness, extendedLength);
        }
    }
#endregion

    #region Grid Coordinate Conversion

    public Vector2Int WorldToGrid(Vector3 worldPosition)
    {
        Vector3 localPosition = transform.InverseTransformPoint(worldPosition);
        int gridX = Mathf.FloorToInt(localPosition.x / cellSize);
        int gridY = Mathf.FloorToInt(localPosition.z / cellSize);
        return new Vector2Int(gridX, gridY);
    }

    public Vector3 GridToWorld(int x, int y)
    {
        Vector3 localPosition = new Vector3((x * cellSize) + (cellSize / 2f), 0f, (y * cellSize) + (cellSize / 2f));
        return transform.TransformPoint(localPosition);
    }

    public bool IsValidCoordinate(int x, int y)
    {
        return x >= 0 && x < width && y >= 0 && y < height;
    }

    public void PlaceItemAt(Item item, int x, int y)
    {
        if (item == null || !IsValidCoordinate(x, y)) return;

        gridItems[x, y] = item;

        item.transform.position = GridToWorld(x, y);
        item.transform.rotation = Quaternion.identity;
        
        // KRİTİK EKLENTİ: Havuzdan çekerken küçülmüş (0 scale) taşları geri tam boyutuna getir.
        item.transform.localScale = Vector3.one; 
        
        item.transform.SetParent(itemRoot);
    }

    public Item GetItemAt(int x, int y)
    {
        if (!IsValidCoordinate(x, y)) return null;
        return gridItems[x, y];
    }

    #endregion

    #region Cell Interaction

    public bool IsCellPlayable(int x, int y)
    {
        if (!IsValidCoordinate(x, y)) return false;
        if (cellTypes[x, y] == CellType.Void || cellTypes[x, y] == CellType.Obstacle) return false;
        return true;
    }

    private bool HasBoardSurface(int x, int y)
    {
        if (!IsValidCoordinate(x, y)) return false;
        if (cellTypes[x, y] == CellType.Void) return false;
        return true;
    }

    public void OnCellClicked(int x, int y)
    {
        if (GameManager.Instance != null && GameManager.Instance.CurrentState != EGameState.GAME) return;
        if (isResolving) return;

        if (!IsCellPlayable(x, y))
        {
            if (hasSelection) DeselectCurrent();
            return;
        }

        if (!hasSelection)
        {
            if (gridItems[x, y] != null)
            {
                SelectCell(x, y);
            }
        }
        else
        {
            if (selectedGridPos.x == x && selectedGridPos.y == y)
            {
                DeselectCurrent();
            }
            else
            {
                // KİLİT: Kayma sırasında tıklamayı engelle
                isResolving = true; 
                
                SwapItems(selectedGridPos.x, selectedGridPos.y, x, y);
                GoalManager.Instance?.DecreaseMove();
                
                // Taşlar uçarken patlama yapmasın, animasyon bitene kadar (0.35s) bekle!
                StartCoroutine(WaitAndResolveMatches(0.35f)); 
            }
        }
    }

    private IEnumerator WaitAndResolveMatches(float delay)
    {
        yield return new WaitForSeconds(delay);
        CheckAndResolveMatches();
    }

    private void SelectCell(int x, int y)
    {
        selectedGridPos = new Vector2Int(x, y);
        hasSelection = true;
        Item selectedItem = gridItems[x, y];

        LeanTween.cancel(selectedItem.gameObject);
        LeanTween.moveY(selectedItem.gameObject, GridToWorld(x, y).y + selectionLiftHeight, 0.15f).setEaseOutQuad();
        LeanTween.scale(selectedItem.gameObject, Vector3.one * 1.15f, 0.15f).setEaseOutQuad();
    }

    private void DeselectCurrent()
    {
        if (!hasSelection) return;
        Item selectedItem = gridItems[selectedGridPos.x, selectedGridPos.y];

        if (selectedItem != null)
        {
            LeanTween.cancel(selectedItem.gameObject);
            LeanTween.move(selectedItem.gameObject, GridToWorld(selectedGridPos.x, selectedGridPos.y), 0.15f).setEaseInQuad();
            LeanTween.scale(selectedItem.gameObject, Vector3.one, 0.15f).setEaseInQuad();
        }

        hasSelection = false;
    }

    private void SwapItems(int x1, int y1, int x2, int y2)
    {
        Item item1 = gridItems[x1, y1];
        Item item2 = gridItems[x2, y2];

        gridItems[x1, y1] = item2;
        gridItems[x2, y2] = item1;

        SoundManager.Instance?.PlaySwap();

        if (item1 != null)
        {
            LeanTween.cancel(item1.gameObject);
            LeanTween.move(item1.gameObject, GridToWorld(x2, y2), 0.3f).setEaseOutQuad();
            LeanTween.scale(item1.gameObject, Vector3.one, 0.15f); // Seçiliyken şişmişti, indir.
        }

        if (item2 != null)
        {
            LeanTween.cancel(item2.gameObject);
            LeanTween.move(item2.gameObject, GridToWorld(x1, y1), 0.3f).setEaseOutQuad();
        }

        hasSelection = false;
    }

    #endregion

    #region Match Detection and Resolution

    public void CheckAndResolveMatches()
    {
        HashSet<Vector2Int> matchedCoords = new HashSet<Vector2Int>();

        // Horizontal
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width - 2; x++)
            {
                Item current = gridItems[x, y];
                if (current == null) continue;

                Item next1 = gridItems[x + 1, y];
                Item next2 = gridItems[x + 2, y];

                if (next1 != null && next2 != null && current.ItemType == next1.ItemType && current.ItemType == next2.ItemType)
                {
                    matchedCoords.Add(new Vector2Int(x, y));
                    matchedCoords.Add(new Vector2Int(x + 1, y));
                    matchedCoords.Add(new Vector2Int(x + 2, y));
                }
            }
        }

        // Vertical
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height - 2; y++)
            {
                Item current = gridItems[x, y];
                if (current == null) continue;

                Item next1 = gridItems[x, y + 1];
                Item next2 = gridItems[x, y + 2];

                if (next1 != null && next2 != null && current.ItemType == next1.ItemType && current.ItemType == next2.ItemType)
                {
                    matchedCoords.Add(new Vector2Int(x, y));
                    matchedCoords.Add(new Vector2Int(x, y + 1));
                    matchedCoords.Add(new Vector2Int(x, y + 2));
                }
            }
        }

        if (matchedCoords.Count > 0)
        {
            StartCoroutine(ResolveMatchesRoutine(matchedCoords));
        }
        else
        {
            isResolving = false;
            GoalManager.Instance?.EvaluateGameState();
        }
    }

    private IEnumerator ResolveMatchesRoutine(HashSet<Vector2Int> matchedCoords)
    {
        isResolving = true;

        // MATCH SESİ
        SoundManager.Instance?.PlayMatch();

        foreach (Vector2Int coord in matchedCoords)
        {
            Item item = gridItems[coord.x, coord.y];

            if (item != null)
            {
                LeanTween.cancel(item.gameObject);
                
                // 1. Z Ekseninde sağa sola titreme (PingPong)
                LeanTween.rotateAroundLocal(item.gameObject, Vector3.forward, 15f, 0.1f).setLoopPingPong(2);
                
                // 2. Küçülerek merkeze doğru içine çökme
                LeanTween.scale(item.gameObject, Vector3.zero, 0.25f).setDelay(0.1f).setEaseInBack();
            }
        }

        // Animasyonların izlenmesi için 0.4 saniye bekle
        yield return new WaitForSeconds(0.4f);

        foreach (Vector2Int coord in matchedCoords)
        {
            int x = coord.x;
            int y = coord.y;
            Item itemToDestroy = gridItems[x, y];

            if (itemToDestroy != null)
            {
                ItemDestroyedEvent?.Invoke(itemToDestroy);
                gridItems[x, y] = null;

                DamageObstacleAt(x + 1, y);
                DamageObstacleAt(x - 1, y);
                DamageObstacleAt(x, y + 1);
                DamageObstacleAt(x, y - 1);
            }
        }

        ApplyGravity();
    }

    private void DamageObstacleAt(int x, int y)
    {
        if (!IsValidCoordinate(x, y)) return;

        if (cellTypes[x, y] == CellType.Obstacle)
        {
            if (obstacleObjects != null && obstacleObjects[x, y] != null)
            {
                Destroy(obstacleObjects[x, y]);
                obstacleObjects[x, y] = null;
            }

            cellTypes[x, y] = CellType.Normal;
        }
    }

    public void ApplyGravity()
    {
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                if (!IsCellPlayable(x, y)) continue;
                if (gridItems[x, y] != null) continue;

                for (int searchY = y + 1; searchY < height; searchY++)
                {
                    if (!IsCellPlayable(x, searchY)) continue;

                    if (gridItems[x, searchY] != null)
                    {
                        Item itemToMove = gridItems[x, searchY];
                        gridItems[x, y] = itemToMove;
                        gridItems[x, searchY] = null;

                        LeanTween.move(itemToMove.gameObject, GridToWorld(x, y), 0.3f).setEaseOutQuad();
                        break;
                    }
                }
            }
        }

        OnGravityFinished?.Invoke();
    }

    #endregion

    private void OnDrawGizmos()
    {
        if (!showGizmos) return;

        Gizmos.color = gizmoColor;
        Matrix4x4 previousMatrix = Gizmos.matrix;
        Gizmos.matrix = transform.localToWorldMatrix;

        for (int x = 0; x <= width; x++) Gizmos.DrawLine(new Vector3(x * cellSize, 0, 0), new Vector3(x * cellSize, 0, height * cellSize));
        for (int y = 0; y <= height; y++) Gizmos.DrawLine(new Vector3(0, 0, y * cellSize), new Vector3(width * cellSize, 0, y * cellSize));

        Gizmos.matrix = previousMatrix;
    }
}