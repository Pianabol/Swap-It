using UnityEngine;
using System.Collections;

public class CameraManager : MonoBehaviour
{
    public static CameraManager Instance;

    [Header("Camera Settings")]
    [SerializeField] private float screenYOffset = -1.0f;

    [Tooltip("Board ekran genişliğinin yaklaşık ne kadarını kaplasın.")]
    [SerializeField, Range(0.5f, 0.98f)]
    private float screenFill = 0.88f;

    [Header("Dynamic Camera Distance")]
    [SerializeField] private float smallGridDistance = 15f;
    [SerializeField] private float mediumGridDistance = 18f;
    [SerializeField] private float largeGridDistance = 20f;

    private void Awake()
    {
        Instance = this;
    }

    public void FrameBoard(Board board, int gridWidth, int gridHeight)
    {
        StartCoroutine(
            FrameRoutine(board, gridWidth, gridHeight)
        );
    }

    private IEnumerator FrameRoutine(
        Board board,
        int gridWidth,
        int gridHeight)
    {
        yield return new WaitForEndOfFrame();

        Camera cam = Camera.main;

        if (cam == null || board == null)
            yield break;

        Renderer[] allRenderers =
            board.GetComponentsInChildren<Renderer>();

        if (allRenderers.Length == 0)
            yield break;

        // =====================================================
        // 1. BOARD SINIRLARINI BUL
        // =====================================================

        Bounds totalBounds = allRenderers[0].bounds;

        foreach (Renderer r in allRenderers)
        {
            totalBounds.Encapsulate(r.bounds);
        }

        // =====================================================
        // 2. GRID BOYUTUNA GÖRE CAMERA DISTANCE
        // =====================================================

        float cameraDistance =
            GetCameraDistance(gridWidth, gridHeight);

        // =====================================================
        // 3. ESKİ ÇALIŞAN ORTALAMA SİSTEMİ
        // =====================================================

        Vector3 targetCenter = totalBounds.center;

        cam.transform.position =
            targetCenter -
            (cam.transform.forward * cameraDistance);

        cam.transform.position +=
            cam.transform.up * screenYOffset;

        // =====================================================
        // 4. ZOOM
        // =====================================================

        float boardWorldWidth =
            gridWidth * board.CellSize;

        float boardWorldHeight =
            gridHeight * board.CellSize;

        float aspect = cam.aspect;

        float sizeFromWidth =
            boardWorldWidth /
            (2f * aspect * screenFill);

        float angle =
            cam.transform.eulerAngles.x *
            Mathf.Deg2Rad;

        float visibleHeight =
            boardWorldHeight *
            Mathf.Abs(Mathf.Sin(angle));

        float sizeFromHeight =
            visibleHeight /
            (2f * screenFill);

        cam.orthographicSize =
            Mathf.Max(
                sizeFromWidth,
                sizeFromHeight
            );

        Debug.Log(
            $"[CAMERA] Grid: {gridWidth}x{gridHeight} | " +
            $"Distance: {cameraDistance:F1} | " +
            $"Ortho: {cam.orthographicSize:F2}"
        );
    }

    private float GetCameraDistance(
        int gridWidth,
        int gridHeight)
    {
        int largestDimension =
            Mathf.Max(gridWidth, gridHeight);

        // Örn: 6x6
        if (largestDimension <= 6)
        {
            return smallGridDistance;
        }

        // Örn: 6x9, 7x8
        if (largestDimension <= 9)
        {
            return mediumGridDistance;
        }

        // Örn: 8x10 ve daha büyük
        return largeGridDistance;
    }
}