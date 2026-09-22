using UnityEngine;

[RequireComponent(typeof(BoxCollider))]
public class Board : MonoBehaviour
{
    [Header("Board Settings")]
    [SerializeField, Min(1)] private int width = 8;
    [SerializeField, Min(1)] private int height = 10;
    [SerializeField, Min(0.1f)] private float cellSize = 1.0f;

    [Header("Spawn Settings")]
    [SerializeField] private GameObject[] itemPrefabs; // Cube1, Cube2, Cube3 buraya sürüklenecek
    [SerializeField] private Transform itemRoot; // Sahne kirlenmesin diye objelerin ekleneceği ebeveyn

    [Header("Debug Settings")]
    [SerializeField] private bool showGizmos = true;
    [SerializeField] private Color gizmoColor = new Color(0, 1, 0, 0.5f);

    private BoxCollider boardCollider;
    
    // Mantıksal Grid: Hangi koordinatta hangi obje var bilgisini burada tutacağız
    private GameObject[,] gridItems; 

    public int Width => width;
    public int Height => height;
    public float CellSize => cellSize;

    private void Awake()
    {
        InitializeCollider();
        
        // Mantıksal dizimizi tahta boyutlarına göre başlatıyoruz
        gridItems = new GameObject[width, height];

        // Eğer Inspector'dan bir ItemRoot atanmamışsa, kodu bozmamak için otomatik oluşturuyoruz
        if (itemRoot == null)
        {
            GameObject rootObj = new GameObject("ItemRoot");
            rootObj.transform.SetParent(this.transform);
            rootObj.transform.localPosition = Vector3.zero;
            itemRoot = rootObj.transform;
        }
    }

    private void OnValidate()
    {
        InitializeCollider();
    }

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

    // YENİ EKLENEN METOT: Tıklanan hücrede obje oluşturma mantığı
    public void OnCellClicked(int x, int y)
    {
        // 1. Hücre dolu mu kontrolü (Üst üste obje spawn olmasını engelliyoruz)
        if (gridItems[x, y] != null)
        {
            Debug.Log($"<color=orange>[OCCUPIED]</color> [{x}, {y}] hücresi zaten dolu!");
            return;
        }

        // 2. Prefab listesi kontrolü
        if (itemPrefabs == null || itemPrefabs.Length == 0)
        {
            Debug.LogError("Item Prefabs listesi boş! Inspector'dan küpleri eklemedin.");
            return;
        }

        // 3. Rastgele bir küp seç
        int randomIndex = Random.Range(0, itemPrefabs.Length);
        GameObject selectedPrefab = itemPrefabs[randomIndex];

        // 4. Hücrenin merkez dünya koordinatını al
        Vector3 spawnPosition = GridToWorld(x, y);

        // 5. Objeyi oluştur ve ItemRoot'un içine hiyerarşik olarak ekle
        GameObject newItem = Instantiate(selectedPrefab, spawnPosition, Quaternion.identity, itemRoot);
        
        // 6. En önemli kısım: Oluşan objeyi mantıksal grid'e kaydet!
        gridItems[x, y] = newItem;

        Debug.Log($"<color=green>[SPAWNED]</color> [{x}, {y}] konumuna {newItem.name} başarıyla yerleştirildi.");
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