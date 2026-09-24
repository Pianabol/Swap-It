using System.Collections.Generic;
using System.Collections;
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

    [Header("Debug Settings")]
    [SerializeField] private bool showGizmos = true;
    [SerializeField] private Color gizmoColor = new Color(0, 1, 0, 0.5f);

    private BoxCollider boardCollider;
    private Item[,] gridItems;

    private bool hasSelection = false;
    private bool isResolving = false; // Tıklamaları kilitlemek için

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
        InitializeCollider();
        gridItems = new Item[width, height];

        if (itemRoot == null)
        {
            GameObject rootObj = new GameObject("ItemRoot");
            rootObj.transform.SetParent(this.transform);
            rootObj.transform.localPosition = Vector3.zero;
            itemRoot = rootObj.transform;
        }
    }

    private void OnValidate() { InitializeCollider(); }

    private void InitializeCollider()
    {
        if (boardCollider == null) boardCollider = GetComponent<BoxCollider>();

        if (boardCollider != null)
        {
            boardCollider.size = new Vector3(width * cellSize, 0.1f, height * cellSize);
            boardCollider.center = new Vector3((width * cellSize) / 2f, 0f, (height * cellSize) / 2f);
        }
    }

    public Vector2Int WorldToGrid(Vector3 worldPosition)
    {
        Vector3 localPosition = transform.InverseTransformPoint(worldPosition);
        int gridX = Mathf.FloorToInt(localPosition.x / cellSize);
        int gridY = Mathf.FloorToInt(localPosition.z / cellSize);
        return new Vector2Int(gridX, gridY);
    }

    public Vector3 GridToWorld(int x, int y)
    {
        Vector3 localPosition = new Vector3(
            (x * cellSize) + (cellSize / 2f),
            0f,
            (y * cellSize) + (cellSize / 2f)
        );
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
        item.transform.SetParent(itemRoot);
    }

    public Item GetItemAt(int x, int y)
    {
        if (!IsValidCoordinate(x, y)) return null;
        return gridItems[x, y];
    }

    public void OnCellClicked(int x, int y)
    {
        if (isResolving) return; // Oyun beklemedeyse tıklamayı yoksay

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
                SwapItems(selectedGridPos.x, selectedGridPos.y, x, y);
                CheckAndResolveMatches();
            }
        }
    }

    private void SelectCell(int x, int y)
    {
        selectedGridPos = new Vector2Int(x, y);
        hasSelection = true;
        Item selectedItem = gridItems[x, y];
        selectedItem.transform.position += Vector3.up * selectionLiftHeight;
    }

    private void DeselectCurrent()
    {
        if (!hasSelection) return;
        Item selectedItem = gridItems[selectedGridPos.x, selectedGridPos.y];
        if (selectedItem != null)
        {
            selectedItem.transform.position = GridToWorld(selectedGridPos.x, selectedGridPos.y);
        }
        hasSelection = false;
    }

    private void SwapItems(int x1, int y1, int x2, int y2)
    {
        Item item1 = gridItems[x1, y1];
        Item item2 = gridItems[x2, y2];

        gridItems[x1, y1] = item2;
        gridItems[x2, y2] = item1;

        if (item1 != null) item1.transform.position = GridToWorld(x2, y2);
        if (item2 != null) item2.transform.position = GridToWorld(x1, y1);

        hasSelection = false;
    }

    public void CheckAndResolveMatches()
    {
        HashSet<Vector2Int> matchedCoords = new HashSet<Vector2Int>();

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
        }
    }

    private IEnumerator ResolveMatchesRoutine(HashSet<Vector2Int> matchedCoords)
    {
        isResolving = true; 
        
        Debug.Log($"<color=orange>[DEBUG DELAY]</color> Toplam {matchedCoords.Count} blok eşleşti. Havaya kalkıyor, 3 saniye bekle...");

        foreach (Vector2Int coord in matchedCoords)
        {
            Item item = gridItems[coord.x, coord.y];
            if (item != null)
            {
                item.transform.position += Vector3.up * selectionLiftHeight; 
            }
        }

        yield return new WaitForSeconds(3f);

        foreach (Vector2Int coord in matchedCoords)
        {
            Item itemToDestroy = gridItems[coord.x, coord.y];
            if (itemToDestroy != null)
            {
                ItemDestroyedEvent?.Invoke(itemToDestroy);
                gridItems[coord.x, coord.y] = null;
            }
        }

        ApplyGravity();

        OnGravityFinished?.Invoke(); // Fabrika Müdürüne (LevelManager) gidecek sinyal
    }

    public void ApplyGravity()
    {
        bool hasMovedAny = false;

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                if (gridItems[x, y] == null)
                {
                    for (int searchY = y + 1; searchY < height; searchY++)
                    {
                        if (gridItems[x, searchY] != null)
                        {
                            Item itemToMove = gridItems[x, searchY];
                            gridItems[x, y] = itemToMove;       
                            gridItems[x, searchY] = null;       
                            
                            //itemToMove.transform.position = GridToWorld(x, y);

                            LeanTween.move(itemToMove.gameObject, GridToWorld(x, y), 0.3f).setEaseOutQuad();

                            hasMovedAny = true;
                            break; 
                        }
                    }
                }
            }
        }

        if (hasMovedAny)
        {
            Debug.Log("<color=yellow>[GRAVITY]</color> Bloklar aşağı kaydırıldı.");
        }
    }

    private void OnDrawGizmos()
    {
        if (!showGizmos) return;

        Gizmos.color = gizmoColor;
        Matrix4x4 previousMatrix = Gizmos.matrix;
        Gizmos.matrix = transform.localToWorldMatrix;

        for (int x = 0; x <= width; x++)
        {
            Gizmos.DrawLine(new Vector3(x * cellSize, 0, 0), new Vector3(x * cellSize, 0, height * cellSize));
        }

        for (int y = 0; y <= height; y++)
        {
            Gizmos.DrawLine(new Vector3(0, 0, y * cellSize), new Vector3(width * cellSize, 0, y * cellSize));
        }

        Gizmos.matrix = previousMatrix;
    }
}